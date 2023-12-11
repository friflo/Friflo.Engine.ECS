using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_EntitySerializer
{
    private const string SampleJson =
"""
{
    "id": 10,
    "children": [
        11
    ],
    "components": {
        "pos": {"x":1,"y":2,"z":3},
        "script1": {"val1":10}
    },
    "tags": [
        "test-tag",
        "TestTag3",
        "xyz"
    ]
}
""";
    
    [Test]
    public static void Test_Entity_DebugJSON_set()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity(1);
        
        entity.DebugJSON = SampleJson;
        
        AreEqual(1,                     entity.Id); // "id" in passed sample JSON is ignored
        AreEqual(1,                     entity.ChildCount);
        AreEqual(11,                    entity.ChildIds[0]);
        
        AreEqual(2,                     entity.Components.Count);
        AreEqual(new Position(1, 2, 3), entity.Position);
        
        var unresolved = entity.GetComponent<Unresolved>();
        AreEqual(1,                     unresolved.tags.Length);
        AreEqual("xyz",                 unresolved.tags[0]);
        
        AreEqual(1,                     entity.Scripts.Length);
        var script =                    entity.GetScript<TestScript1>();
        AreEqual(10,                    script.val1);
        
        AreEqual(2,                     entity.Tags.Count);
        IsTrue  (                       entity.Tags.Has<TestTag>());
        IsTrue  (                       entity.Tags.Has<TestTag3>());
    }
    
    [Test]
    public static void Test_Entity_DebugJSON_set_errors()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity(1);
        {
            var e = Throws<ArgumentNullException>(() => {
                entity.DebugJSON = null;
            });
            AreEqual("Value cannot be null. (Parameter 'json')", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                entity.DebugJSON = "";
            });
            AreEqual("Error: unexpected EOF on root path: '(root)' at position: 0", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                entity.DebugJSON = "[]";
            });
            AreEqual("Error: expect object entity. was: ArrayStart at position: 1 path: '[]' at position: 1", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                entity.DebugJSON = "{}x";
            });
            AreEqual("Error: Expected EOF path: '(root)' at position: 3", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                entity.DebugJSON = "{";
            });
            AreEqual("Error: unexpected EOF > expect key path: '(root)' at position: 1", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                entity.DebugJSON = "{\"components\":[]}";
            });
            AreEqual("Error: expect 'components' == object. was: array. path: 'components[]' at position: 15", e!.Message);
        }
    }
}


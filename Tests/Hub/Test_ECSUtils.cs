// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming
namespace Tests.Hub;


public static class Test_ECSUtils
{
#region JsonArrayToDataEntities()
    /// <summary> Cover <see cref="ECSUtils.JsonArrayToDataEntities"/> </summary>
    [Test]
    public static void Test_ECSUtils_JsonArrayToDataEntities()
    {
        var json = new JsonValue(
        """
        [{
            "id": 2,
            "tags": ["Tag1"],
            "components": {
                "name": { "value": "test" }
            },
            "children": [3]
        }]
        """);
        var dataEntities = new List<DataEntity>();
        IsNull(ECSUtils.JsonArrayToDataEntities(json, dataEntities));
        
        AreEqual(1, dataEntities.Count);
        var data0 = dataEntities[0];
        AreEqual(2,                 data0.pid);
        AreEqual(new [] { "Tag1" }, data0.tags);
        AreEqual(new [] { 3 },      data0.children);
        var components = "{\n        \"name\": { \"value\": \"test\" }\n    }";
        AreEqual(components,        data0.components.ToString());
    }
    
    /// <summary> Cover <see cref="ECSUtils.JsonArrayToDataEntities"/> errors </summary>
    [Test]
    public static void Test_ECSUtils_JsonArrayToDataEntities_errors()
    {
        var dataEntities = new List<DataEntity>();
        {
            var error = ECSUtils.JsonArrayToDataEntities(new JsonValue(), dataEntities);
            AreEqual("expect array. was: ValueNull at position: 4", error);
        } {
            var error = ECSUtils.JsonArrayToDataEntities(new JsonValue(""), dataEntities);
            AreEqual("unexpected EOF on root path: '(root)' at position: 0", error);
        } {
            var error = ECSUtils.JsonArrayToDataEntities(new JsonValue("{}"), dataEntities);
            AreEqual("expect array. was: ObjectStart at position: 1", error);
        }
    }
    #endregion
    
    
#region DuplicateEntities()
    /// <summary> Cover <see cref="ECSUtils.DuplicateEntities"/> and <see cref="ECSUtils.DuplicateChildren"/> </summary>
    [Test]
    public static void Test_ECSUtils_DuplicateEntities()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        child2.AddComponent(new EntityName("child-2"));
        child3.AddComponent(new EntityName("child-3"));
        
        store.SetStoreRoot(root);
        root.AddChild(child2);
        child2.AddChild(child3);
        AreEqual(1,             root.ChildCount);
        AreEqual(1,             child2.ChildCount);
        
        // --- Duplicate two child entities
        var entities    = new List<Entity>{ child2, child3 };
        var indexes     = ECSUtils.DuplicateEntities(entities);
        
        AreEqual(2,             indexes.Length);
        AreEqual(2,             root.ChildCount);
        AreEqual(2,             child2.ChildCount);
        
        var clone2 =            root.ChildEntities  [indexes[0]];
        AreEqual("child-2",     clone2.Name.value);
        
        var clone3 =            child2.ChildEntities[indexes[1]];
        AreEqual("child-3",     clone3.Name.value);
        
        // --- Duplicate root
        entities        = new List<Entity>{ root };
        indexes         = ECSUtils.DuplicateEntities(entities);
        
        AreEqual(2,             root.ChildCount);
        AreEqual(1,             indexes.Length);
        AreEqual(-1,            indexes[0]);
    }
    #endregion
    
#region EntitiesToJsonArray()
    /// <summary> Cover <see cref="ECSUtils.EntitiesToJsonArray"/> and <see cref="ECSUtils.AddChildren"/></summary>
    [Test]
    public static void Test_ECSUtils_EntitiesToJsonArray()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        root.AddChild(child2);
        root.AddChild(child3);
        root.  AddComponent(new EntityName("root"));
        child2.AddComponent(new EntityName("child-2"));
        child3.AddComponent(new EntityName("child-3"));
        
        var entities        = new [] { root, child2 };
        var jsonEntities    = ECSUtils.EntitiesToJsonArray(entities);
        
        AreEqual(3, jsonEntities.count);
        var json =
"""
[{
    "id": 1,
    "children": [
        2,
        3
    ],
    "components": {
        "name": {"value":"root"}
    }
},{
    "id": 2,
    "components": {
        "name": {"value":"child-2"}
    }
},{
    "id": 3,
    "components": {
        "name": {"value":"child-3"}
    }
}]
""";
        AreEqual(json, jsonEntities.entities.ToString());
    }
    #endregion
}
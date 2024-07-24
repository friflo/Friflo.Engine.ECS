using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public static class Test_ComponentWriter
{
    [Test]
    public static void Test_ComponentWriter_write_components()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var entity  = store.CreateEntity(10);
        var child   = store.CreateEntity(11);
        entity.AddChild(child);
        entity.AddTag<TestTag>();
        entity.AddTag<TestTag3>();
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddScript(new TestScript1 { val1 = 10 });
        
        var dataEntity = converter.EntityToDataEntity(entity, null, false);
        
        AreEqual(10,                dataEntity.pid);
        
        AreEqual(2,                 dataEntity.tags.Count);
        AreEqual("test-tag",        dataEntity.tags[0]);
        AreEqual(nameof(TestTag3),  dataEntity.tags[1]);
        
        AreEqual(1,                 dataEntity.children.Count);
        AreEqual(11,                dataEntity.children[0]);
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"script1\":{\"val1\":10}}", dataEntity.components.AsString());
        
var expect =
@"{
    ""id"": 10,
    ""children"": [
        11
    ],
    ""components"": {
        ""pos"": {""x"":1,""y"":2,""z"":3},
        ""script1"": {""val1"":10}
    },
    ""tags"": [
        ""test-tag"",
        ""TestTag3""
    ]
}";
        var json = dataEntity.DebugJSON;
        AreEqual(expect, json);
        
        dataEntity.components = new JsonValue("xxx");
        json = dataEntity.DebugJSON;
        AreEqual("'components' error: unexpected character while reading value. Found: x path: '(root)' at position: 1", json);
    }
    
    [Test]
    public static void Test_ComponentWriter_write_EntityName()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var entity      = store.CreateEntity();
        entity.AddComponent(new EntityName("test"));
        var dataEntity = converter.EntityToDataEntity(entity, null, false);
        
        AreEqual("{\"name\":{\"value\":\"test\"}}", dataEntity.components.AsString());
    }
    
    [Test]
    public static void Test_ComponentWriter_write_empty_components()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var entity      = store.CreateEntity(10);
        var dataEntity  = converter.EntityToDataEntity(entity, null, false);
        
        AreEqual(10,    dataEntity.pid);
        IsNull  (dataEntity.children);
    }
    
    [Test]
    public static void Test_ComponentWriter_write_non_serialized_component()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var entity      = store.CreateEntity(10);
        entity.AddComponent(new NonSerializedComponent());
        entity.AddComponent(new EntityName("test"));
        var dataEntity = converter.EntityToDataEntity(entity, null, false);
        AreEqual("{\"name\":{\"value\":\"test\"}}", dataEntity.components.ToString());
        
        var serializer = new EntitySerializer();
        var json = serializer.WriteEntity(entity);
        const string expect =
@"{
    ""id"": 10,
    ""components"": {
        ""name"": {""value"":""test""}
    }
}";
        AreEqual(expect, json);
    }
    
    [Test]
    public static void Test_ComponentWriter_write_tags()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var entity      = store.CreateEntity(10);
        entity.AddTag<TestTag>();
        entity.AddTag<TestTag3>();
        var dataEntity  = converter.EntityToDataEntity(entity, null, false);
        
        AreEqual(10,                dataEntity.pid);
        AreEqual(2,                 dataEntity.tags.Count);
        Contains("test-tag",        dataEntity.tags);
        Contains(nameof(TestTag3),  dataEntity.tags);
    }
    
    [Test]
    public static void Test_ComponentWriter_write_components_Perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddScript(new TestScript1 { val1 = 10 });

        int count = 10; // 2_000_000 ~ #PC: 1.935 ms
        DataEntity dataEntity = null;
        for (int n = 0; n < count; n++) {
            dataEntity = converter.EntityToDataEntity(entity, null, false);
        }
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"script1\":{\"val1\":10}}", dataEntity!.components.AsString());
    }
    
    [Test]
    public static void Test_ComponentWriter_DataEntity()
    {
        var dataEntity = new DataEntity { pid = 1234 };
        AreEqual("pid: 1234", dataEntity.ToString());
    }
    
    [Test]
    public static void Test_Entity_DebugJSON_get()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var entity      = store.CreateEntity(10);
        var child       = store.CreateEntity(11);
        var unresolved  = new Unresolved { tags = new [] { "xyz" } };
        entity.AddChild(child);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddComponent(unresolved);
        entity.AddTag<TestTag>();
        entity.AddTag<TestTag3>();
        entity.AddScript(new TestScript1 { val1 = 10 });

        AreEqual(SampleJson, entity.DebugJSON);
    }
    
    private const string SampleJson =
@"{
    ""id"": 10,
    ""children"": [
        11
    ],
    ""components"": {
        ""pos"": {""x"":1,""y"":2,""z"":3},
        ""script1"": {""val1"":10}
    },
    ""tags"": [
        ""test-tag"",
        ""TestTag3"",
        ""xyz""
    ]
}";

    [Test]
    public static void Test_ComponentWriter_JsonMembers()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var entity = store.CreateEntity();
        entity.AddComponent(new EntityName("test"));
        entity.AddComponent<Position>();
        entity.AddComponent<Rotation>();
        entity.AddScript(new TestScript1 { val1 = 42 });
        var unresolved = new Unresolved {
            components = new[] {
                new UnresolvedComponent ("unknown", new JsonValue("{\"value\": 1}"))
            }
        };
        entity.AddComponent(unresolved);
        
        var members = new List<JsonValue>();
        
        converter.EntityComponentsToJsonMembers(entity, members);
        AreEqual(5, members.Count);
        AreEqual("\"name\":{\"value\":\"test\"}",               members[0].ToString());
        AreEqual("\"pos\":{\"x\":0,\"y\":0,\"z\":0}",           members[1].ToString());
        AreEqual("\"rot\":{\"x\":0,\"y\":0,\"z\":0,\"w\":0}",   members[2].ToString());
        AreEqual("\"unknown\":{\"value\": 1}",                  members[3].ToString());
        AreEqual("\"script1\":{\"val1\":42}",                   members[4].ToString());
        
        Throws<ArgumentNullException>(() => {
            converter.EntityComponentsToJsonMembers(default, members);
        });
    }
}

}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;
using static Tests.Utils.SingleThreadSynchronizationContext;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public static class Test_Serializer
{
#region Happy path
    [Test]
    public  static void  Test_Serializer_write_store() => Run(async () => await Serializer_write_store());
    private static async Task Serializer_write_store()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddScript(new TestScript1 { val1 = 10 });
        entity.AddTag<TestTag>();
        entity.AddTag<TestTag3>();
        
        var child   = store.CreateEntity(11);
        store.OnChildEntitiesChanged += args => {
            AreEqual("entity: 10 - event > Add Child[0] = 11", args.ToString());
        };
        entity.AddChild(child);
        AreEqual(2, store.Count);
        
        // --- write store entities sync
        {
            var fileName    = TestUtils.GetBasePath() + "assets/write_scene.json";
            var file        = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            serializer.WriteStore(store, file);
            file.Close();
        }
        // --- write store entities async
        {
            var fileName    = TestUtils.GetBasePath() + "assets/write_scene_async.json";
            var file        = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            await serializer.WriteStoreAsync(store, file);
            file.Close();
        }
    }
    
    [Test]
    public static void Test_Serializer_write_entities()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddScript(new TestScript1 { val1 = 10 });
        entity.AddTag<TestTag>();
        entity.AddTag<TestTag3>();
        
        var child   = store.CreateEntity(11);
        store.OnChildEntitiesChanged += args => {
            AreEqual("entity: 10 - event > Add Child[0] = 11", args.ToString());
        };
        entity.AddChild(child);
        AreEqual(2, store.Count);
        
        var entities = new List<Entity> { entity, child, default };
        
        // --- store entities sync
        {
            var fileName    = TestUtils.GetBasePath() + "assets/write_entities.json";
            var file        = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            serializer.WriteEntities(entities, file);
            file.Close();
        }
    }
    
    [Test]
    public static void Test_Serializer_write_empty_store()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        var stream = new MemoryStream();
        serializer.WriteStore(store, stream);
        var str = MemoryStreamAsString(stream);
        stream.Close();
        AreEqual("[]", str);
        
        AreEqual(0, store.Count);
    }
    
    [Test]
    public static void Test_Serializer_read_into_store()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        // --- read entities into store sync
        for (int n = 0; n < 2; n++)
        {
            var fileName    = TestUtils.GetBasePath() + "assets/read_scene.json";
            var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var result      = serializer.ReadIntoStore(store, file);
            file.Close();
            AssertReadIntoStoreResult(result, store);
        }
    }
    
    [Test]
    public  static void  Test_Serializer_read_into_store_async() => Run(async () => await Serializer_read_into_store_async());
    private static async Task Serializer_read_into_store_async()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        // --- load entities into store sync
        for (int n = 0; n < 2; n++)
        {
            var fileName    = TestUtils.GetBasePath() + "assets/read_scene.json";
            var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var result      = await serializer.ReadIntoStoreAsync(store, file);
            file.Close();
            AssertReadIntoStoreResult(result, store);
        }
    }
    
    private static void AssertReadIntoStoreResult(ReadResult result, EntityStore store)
    {
        AreEqual("entityCount: 2", result.ToString());
        IsNull(result.error);
        AreEqual(2, result.entityCount);
        AreEqual(2, store.Count);
            
        var root        = store.GetEntityById(10);
        AreEqual(11,    root.ChildIds[0]);
        IsTrue  (new Position(1,2,3) == root.Position);
        AreEqual(2,     root.Tags.Count);
        IsTrue  (root.Tags.Has<TestTag>());
        IsTrue  (root.Tags.Has<TestTag3>());
            
        var child       = store.GetEntityById(11);
        AreEqual(0,     child.ChildCount);
        AreEqual(0,     child.Components.Count);
        AreEqual(0,     child.Tags.Count);
            
        AreEqual("Components: [Position, TreeNode]",    root.Archetype.ComponentTypes.ToString());
        AreEqual("Components: []",                      child.Archetype.ComponentTypes.ToString());
    }
    
    [Test]
    public static void Test_Serializer_read_entities()
    {
        var serializer  = new EntitySerializer();
        var fileName    = TestUtils.GetBasePath() + "assets/read_scene.json";
        var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var entities    = new List<DataEntity>();
        
        var result      = serializer.ReadEntities(entities, file);
        file.Close();
        
        AreEqual("entityCount: 2", result.ToString());
        AssertReadEntitiesResult(entities);
    }
    
    [Test]
    public static void Test_Serializer_read_entities_MemoryStream()
    {
        var serializer  = new EntitySerializer();
        var fileName    = TestUtils.GetBasePath() + "assets/read_scene.json";
        var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var memory      = new MemoryStream();
        file.CopyTo(memory);
        file.Close();
        var entities    = new List<DataEntity>();
        
        var result      = serializer.ReadEntities(entities, memory);
        
        AreEqual("entityCount: 2", result.ToString());
        AssertReadEntitiesResult(entities);
    }
    
    private static void AssertReadEntitiesResult(List<DataEntity> entities) {
        var root = entities[0];
        AreEqual(10,                root.pid);
        AreEqual(1,                 root.children.Count);
        AreEqual(11,                root.children[0]);
        AreEqual("{\n        \"pos\": {\"x\":1,\"y\":2,\"z\":3},\n        \"script1\": {\"val1\":10}\n    }",
                                    root.components.AsString());
        AreEqual(2,                 root.tags.Count);
        AreEqual("test-tag",        root.tags[0]);
        AreEqual(nameof(TestTag3),  root.tags[1]);
        
        var child  = entities[1];
        AreEqual(11,        child.pid);
    }
    
    [Test]
    public static void Test_Serializer_write_store_Perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        int count = 10;  // 1_000_000 ~ #PC: 1.227 ms
        for (int n = 0; n < count; n++) {
        var entity  = store.CreateEntity();
            entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
            entity.AddTag<TestTag>();
        }
        
        AreEqual(count, store.Count);
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var stream      = new MemoryStream();
        serializer.WriteStore(store, stream);
        var sizeKb = stream.Length / 1024;
        stream.Close();
        Console.WriteLine($"Write store: entities: {count}, size: {sizeKb} kb, duration: {stopwatch.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_Serializer_read_into_store_Perf()
    {
        int entityCount = 100; // 1_000_000 ~ #PC: 2367 ms
        var stream      = new MemoryStream();
        // --- create JSON store file with EntitySerializer
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var serializer  = new EntitySerializer();

            for (int n = 0; n < entityCount; n++) {
                var entity  = store.CreateEntity();
                entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
                entity.AddTag<TestTag>();
            }
            AreEqual(entityCount, store.Count);
            serializer.WriteStore(store, stream);
            MemoryStreamAsString(stream);
        }
        // --- read created JSON store with EntitySerializer
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var serializer  = new EntitySerializer();
            stream.Position = 0;
            var result = serializer.ReadIntoStore(store, stream);
            IsNull  (result.error);
            AreEqual(entityCount, result.entityCount);
            AreEqual(entityCount, store.Count);
        }
        Console.WriteLine($"Read(). JSON size: {stream.Length}, entities: {entityCount}, duration: {stopwatch.ElapsedMilliseconds} ms");
        stream.Close();
    }
    
    private static string MemoryStreamAsString(MemoryStream stream) {
        stream.Flush();
        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }
    
    private static Stream StringAsStream(string json) {
        var bytes = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes.Length);
        stream.Write(bytes);
        stream.Position = 0;
        return stream;
    }
    #endregion
    
#region read coverage
    /// <summary>Cover <see cref="EntitySerializer.ReadEntity"/></summary>
    [Test]
    public static void Test_Serializer_read_unknown_JSON_members()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        var fileName    = TestUtils.GetBasePath() + "assets/read_unknown_members.json";
        var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var result      = serializer.ReadIntoStore(store, file);
        file.Close();
        AssertReadIntoStoreResult(result, store);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadIntoStoreAsync"/></summary>
    [Test]
    public  static void  Test_Serializer_ReadAsync_MemoryStream() => Run(async () => await Serializer_ReadAsync_MemoryStream());
    private static async Task Serializer_ReadAsync_MemoryStream()
    {
        var stream      = new MemoryStream();
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        serializer.WriteStore(store, stream);
        stream.Position = 0;
        var result = await serializer.ReadIntoStoreAsync(store, stream);
        IsNull(result.error);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadIntoStoreSync"/></summary>
    [Test]
    public static void Test_Serializer_Read_error_ReadSync()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var entities    = new List<DataEntity>();
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("xxx");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("unexpected character while reading value. Found: x path: '(root)' at position: 1", result.error);
        result          = serializer.ReadEntities(entities, stream);
        AreEqual("unexpected character while reading value. Found: x path: '(root)' at position: 1", result.error);
        
        stream          = StringAsStream("{}");
        result          = serializer.ReadIntoStore(store, stream);
        AreEqual("expect array. was: ObjectStart at position: 1", result.error);
        result          = serializer.ReadEntities(entities, stream);
        AreEqual("expect array. was: ObjectStart at position: 1", result.error);
        
        stream          = StringAsStream("[}");
        result          = serializer.ReadIntoStore(store, stream);
        AreEqual("unexpected character while reading value. Found: } path: '[0]' at position: 2", result.error);
        result          = serializer.ReadEntities(entities, stream);
        AreEqual("unexpected character while reading value. Found: } path: '[0]' at position: 2", result.error);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadEntity"/></summary>
    [Test]
    public static void Test_Serializer_Read_error_ReadEntity()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[{ xxx");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("unexpected character > expect key. Found: x path: '[0]' at position: 4", result.error);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadEntitiesArrayIntoStore"/></summary>
    [Test]
    public static void Test_Serializer_Read_error_ReadEntities()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var entities    = new List<DataEntity>();
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[1]");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("expect object entity. was: ValueNumber path: '[0]' at position: 2", result.error);
        AreEqual("entityCount: 0 error: expect object entity. was: ValueNumber path: '[0]' at position: 2", result.ToString());
        
        result          = serializer.ReadEntities(entities, stream);
        AreEqual("expect object entity. was: ValueNumber path: '[0]' at position: 2", result.error);
        AreEqual("entityCount: 0 error: expect object entity. was: ValueNumber path: '[0]' at position: 2", result.ToString());
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadChildren"/></summary>
    [Test]
    public static void Test_Serializer_Read_error_ReadChildren()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var entities    = new List<DataEntity>();
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[ {\"children\":[true] } }");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("expect child id number. was: ValueBool path: '[0].children[0]' at position: 19", result.error);
        result          = serializer.ReadEntities(entities, stream);
        AreEqual("expect child id number. was: ValueBool path: '[0].children[0]' at position: 19", result.error);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadTags"/></summary>
    [Test]
    public static void Test_Serializer_Read_error_ReadTags()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[ {\"tags\":[1] } }");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("expect tag string. was: ValueNumber path: '[0].tags[0]' at position: 12", result.error);
    }
    
    [Test]
    public static void Test_Serializer_Read_tags_error()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[ {\"tags\":[1] } }");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("expect tag string. was: ValueNumber path: '[0].tags[0]' at position: 12", result.error);
    }
    
    [Test]
    public static void Test_Serializer_Read_component_error()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[ {\"id\":10, \"components\":{\"pos\":{\"x\":false}} } }");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("'components[pos]' - Cannot assign bool to float. got: false path: 'x' at position: 10 path: '[0]' at position: 46", result.error);
    }
    #endregion
    
    [Test]
    public static void Test_Serializer_Non_CLS()
    {
        var storeWrite  = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        
        var nonClsWrite = new NonClsTypes {
            int8        = 1,
            uint16      = 2,
            uint32      = 3,
            uint64      = 4,
            int8Null    = 5,
            uint16Null  = 6,
            uint32Null  = 7,
            uint64Null  = 8
        };
        
        var entityWrite = storeWrite.CreateEntity();
        entityWrite.AddComponent(nonClsWrite);
        
        // --- write
        var stream = new MemoryStream();
        serializer.WriteStore(storeWrite, stream);
        var json = MemoryStreamAsString(stream);
        var expect =
@"[{
    ""id"": 1,
    ""components"": {
        ""NonClsTypes"": {""int8"":1,""uint16"":2,""uint32"":3,""uint64"":4,""int8Null"":5,""uint16Null"":6,""uint32Null"":7,""uint64Null"":8}
    }
}]";
        AreEqual(expect, json);
        
        // --- read
        var storeRead   = new EntityStore(PidType.UsePidAsId);
        stream.Position = 0;
        var result = serializer.ReadIntoStore(storeRead, stream);
        AreEqual(1, result.entityCount);
        
        var entityRead = storeRead.GetEntityById(1);
        var nonClsRead = entityRead.GetComponent<NonClsTypes>();
        AreEqual(nonClsWrite, nonClsRead);
    }
    
    [Test]
    public static void Test_Serializer_ignored_field_types()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        
        var entity = store.CreateEntity();
        entity.AddComponent(new ComponentWithGenerics());
        var json = serializer.WriteEntity(entity);
        var expect =
@"{
    ""id"": 1,
    ""components"": {
        ""ComponentWithGenerics"": {}
    }
}";
        AreEqual(expect, json);
    }
    
    [Test]
    public static void Test_Serializer_unsupported_field_types()
    {
        var store       = new EntityStore();
        var serializer  = new EntitySerializer();
        
        var entity = store.CreateEntity();
        entity.AddComponent(new BlittableUri());
        
        var e = Throws<TargetInvocationException>(() => {
            serializer.WriteEntity(entity);
        });
        var inner = e!.InnerException as ArgumentException;
        NotNull(inner);
        StringAssert.StartsWith("Type 'System.Uri' does not have a default constructor", inner.Message);
    }
}

}
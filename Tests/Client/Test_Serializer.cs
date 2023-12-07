using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;
using static NUnit.Framework.Assert;

#pragma warning disable CS0618 // Type or member is obsolete

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.Client;

public static class Test_Serializer
{
#region Happy path
    [Test]
    public static async Task Test_Serializer_write_store()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddScript(new TestScript1 { val1 = 10 });
        entity.AddTag<TestTag>();
        
        var child   = store.CreateEntity(11);
        store.ChildNodesChanged = (object _, in ChildNodesChangedArgs args) => {
            AreEqual("entity: 10 - Add ChildIds[0] = 11", args.ToString());
        };
        entity.AddChild(child);
        AreEqual(2, store.EntityCount);
        
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
        
        var child   = store.CreateEntity(11);
        store.ChildNodesChanged = (object _, in ChildNodesChangedArgs args) => {
            AreEqual("entity: 10 - Add ChildIds[0] = 11", args.ToString());
        };
        entity.AddChild(child);
        AreEqual(2, store.EntityCount);
        
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
        
        AreEqual(0, store.EntityCount);
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
            AssertReadResult(result, store);
        }
    }
    
    [Test]
    public static async Task Test_Serializer_read_into_store_async()
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
            AssertReadResult(result, store);
        }
    }
    
    private static void AssertReadResult(ReadResult result, EntityStore store)
    {
        AreEqual("entityCount: 2", result.ToString());
        IsNull(result.error);
        AreEqual(2, result.entityCount);
        AreEqual(2, store.EntityCount);
            
        var root        = store.GetEntityById(10);
        AreEqual(11,    root.ChildIds[0]);
        IsTrue  (new Position(1,2,3) == root.Position);
        AreEqual(1,     root.Tags.Count);
        IsTrue  (root.Tags.Has<TestTag>());
            
        var child       = store.GetEntityById(11);
        AreEqual(0,     child.ChildCount);
        AreEqual(0,     child.Components.Count);
        AreEqual(0,     child.Tags.Count);
            
        var type = store.GetArchetype(Signature.Get<Position>(), Tags.Get<TestTag>());
        AreEqual(1,     type.EntityCount);
    }
    
    [Test]
    public static void Test_Serializer_write_store_Perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();

        int count = 10;  // 1_000_000 ~ 1.227 ms
        for (int n = 0; n < count; n++) {
        var entity  = store.CreateEntity();
            entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
            entity.AddTag<TestTag>();
        }
        
        AreEqual(count, store.EntityCount);
        
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
        int entityCount = 100; // 1_000_000 ~ 2367 ms
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
            AreEqual(entityCount, store.EntityCount);
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
            AreEqual(entityCount, store.EntityCount);
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
        AssertReadResult(result, store);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadIntoStoreAsync"/></summary>
    [Test]
    public static async Task Test_Serializer_ReadAsync_MemoryStream()
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
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("xxx");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("unexpected character while reading value. Found: x path: '(root)' at position: 1", result.error);
        
        stream          = StringAsStream("{}");
        result          = serializer.ReadIntoStore(store, stream);
        AreEqual("expect array. was: ObjectStart at position: 1", result.error);
        
        stream          = StringAsStream("[}");
        result          = serializer.ReadIntoStore(store, stream);
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
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[1]");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("expect object entity. was: ValueNumber at position: 2 path: '[0]' at position: 2", result.error);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadChildren"/></summary>
    [Test]
    public static void Test_Serializer_Read_error_ReadChildren()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[ {\"children\":[true] } }");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("expect child id number. was: ValueBool at position: 19 path: '[0].children[0]' at position: 19", result.error);
    }
    
    /// <summary>Cover <see cref="EntitySerializer.ReadTags"/></summary>
    [Test]
    public static void Test_Serializer_Read_error_ReadTags()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var serializer  = new EntitySerializer();
        
        var stream      = StringAsStream("[ {\"tags\":[1] } }");
        var result      = serializer.ReadIntoStore(store, stream);
        AreEqual("expect tag string. was: ValueNumber at position: 12 path: '[0].tags[0]' at position: 12", result.error);
    }
    #endregion
}
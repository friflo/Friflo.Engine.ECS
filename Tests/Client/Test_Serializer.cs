using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
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
    public static async Task Test_Serializer_write_scene()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);

        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddBehavior(new TestBehavior1 { val1 = 10 });
        entity.AddTag<TestTag>();
        
        var child   = store.CreateEntity(11);
        entity.AddChild(child);
        AreEqual(2, store.EntityCount);
        
        // --- store game entities as scene sync
        {
            var fileName    = TestUtils.GetBasePath() + "assets/write_scene.json";
            var file        = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            serializer.WriteScene(file);
            file.Close();
        }
        // --- store game entities as scene async
        {
            var fileName    = TestUtils.GetBasePath() + "assets/write_scene_async.json";
            var file        = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            await serializer.WriteSceneAsync(file);
            file.Close();
        }
    }
    
    [Test]
    public static void Test_Serializer_write_empty_scene()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);

        var stream = new MemoryStream();
        serializer.WriteScene(stream);
        var str = MemoryStreamAsString(stream);
        stream.Close();
        AreEqual("[]", str);
        
        AreEqual(0, store.EntityCount);
    }
    
    [Test]
    public static void Test_Serializer_read_scene()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);

        // --- load game entities as scene sync
        for (int n = 0; n < 2; n++)
        {
            var fileName    = TestUtils.GetBasePath() + "assets/read_scene.json";
            var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var result      = serializer.ReadScene(file);
            file.Close();
            AssertReadSceneResult(result, store);
        }
    }
    
    [Test]
    public static async Task Test_Serializer_read_scene_async()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);

        // --- load game entities as scene sync
        for (int n = 0; n < 2; n++)
        {
            var fileName    = TestUtils.GetBasePath() + "assets/read_scene.json";
            var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var result      = await serializer.ReadSceneAsync(file);
            file.Close();
            AssertReadSceneResult(result, store);
        }
    }
    
    private static void AssertReadSceneResult(ReadSceneResult result, GameEntityStore store)
    {
        AreEqual("entityCount: 2", result.ToString());
        IsNull(result.error);
        AreEqual(2, result.entityCount);
        AreEqual(2, store.EntityCount);
            
        var root        = store.GetNodeById(10).Entity;
        AreEqual(11,    root.ChildIds[0]);
        IsTrue  (new Position(1,2,3) == root.Position);
        AreEqual(1,     root.Tags.Count);
        IsTrue  (root.Tags.Has<TestTag>());
            
        var child       = store.GetNodeById(11).Entity;
        AreEqual(0,     child.ChildCount);
        AreEqual(0,     child.Components_.Length);
        AreEqual(0,     child.Tags.Count);
            
        var type = store.GetArchetype(Signature.Get<Position>(), Tags.Get<TestTag>());
        AreEqual(1,     type.EntityCount);
    }
    
    [Test]
    public static void Test_Serializer_write_scene_Perf()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);

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
        serializer.WriteScene(stream);
        var sizeKb = stream.Length / 1024;
        stream.Close();
        Console.WriteLine($"Write scene: entities: {count}, size: {sizeKb} kb, duration: {stopwatch.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_Serializer_read_scene_Perf()
    {
        int entityCount = 100; // 1_000_000 ~ 2367 ms
        var stream      = new MemoryStream();
        // --- create JSON scene with GameDataSerializer
        {
            var store       = new GameEntityStore(PidType.UsePidAsId);
            var serializer  = new GameDataSerializer(store);

            for (int n = 0; n < entityCount; n++) {
                var entity  = store.CreateEntity();
                entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
                entity.AddTag<TestTag>();
            }
            AreEqual(entityCount, store.EntityCount);
            serializer.WriteScene(stream);
            MemoryStreamAsString(stream);
        }
        // --- read created JSON scene with GameDataSerializer
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        {
            var store       = new GameEntityStore(PidType.UsePidAsId);
            var serializer  = new GameDataSerializer(store);
            stream.Position = 0;
            var result = serializer.ReadScene(stream);
            IsNull  (result.error);
            AreEqual(entityCount, result.entityCount);
            AreEqual(entityCount, store.EntityCount);
        }
        Console.WriteLine($"ReadScene(). JSON size: {stream.Length}, entities: {entityCount}, duration: {stopwatch.ElapsedMilliseconds} ms");
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
    /// <summary>Cover <see cref="GameDataSerializer.ReadEntity"/></summary>
    [Test]
    public static void Test_Serializer_read_unknown_JSON_members()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);
        var fileName    = TestUtils.GetBasePath() + "assets/read_unknown_members.json";
        var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var result      = serializer.ReadScene(file);
        file.Close();
        AssertReadSceneResult(result, store);
    }
    
    /// <summary>Cover <see cref="GameDataSerializer.ReadSceneAsync"/></summary>
    [Test]
    public static async Task Test_Serializer_ReadSceneAsync_MemoryStream()
    {
        var stream      = new MemoryStream();
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);
        serializer.WriteScene(stream);
        stream.Position = 0;
        var result = await serializer.ReadSceneAsync(stream);
        IsNull(result.error);
    }
    
    /// <summary>Cover <see cref="GameDataSerializer.ReadSceneSync"/></summary>
    [Test]
    public static void Test_Serializer_ReadScene_error_ReadSceneSync()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);
        
        var stream      = StringAsStream("xxx");
        var result      = serializer.ReadScene(stream);
        AreEqual("unexpected character while reading value. Found: x path: '(root)' at position: 1", result.error);
        
        stream          = StringAsStream("{}");
        result          = serializer.ReadScene(stream);
        AreEqual("expect array. was: ObjectStart at position: 1", result.error);
        
        stream          = StringAsStream("[}");
        result          = serializer.ReadScene(stream);
        AreEqual("unexpected character while reading value. Found: } path: '[0]' at position: 2", result.error);
    }
    
    /// <summary>Cover <see cref="GameDataSerializer.ReadEntity"/></summary>
    [Test]
    public static void Test_Serializer_ReadScene_error_ReadEntity()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);
        
        var stream      = StringAsStream("[{ xxx");
        var result      = serializer.ReadScene(stream);
        AreEqual("unexpected character > expect key. Found: x path: '[0]' at position: 4", result.error);
    }
    
    /// <summary>Cover <see cref="GameDataSerializer.ReadEntities"/></summary>
    [Test]
    public static void Test_Serializer_ReadScene_error_ReadEntities()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);
        
        var stream      = StringAsStream("[1]");
        var result      = serializer.ReadScene(stream);
        AreEqual("expect object entity. was: ValueNumber at Position: 2 path: '[0]' at position: 2", result.error);
    }
    
    /// <summary>Cover <see cref="GameDataSerializer.ReadChildren"/></summary>
    [Test]
    public static void Test_Serializer_ReadScene_error_ReadChildren()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);
        
        var stream      = StringAsStream("[ {\"children\":[true] } }");
        var result      = serializer.ReadScene(stream);
        AreEqual("expect child id number. was: ValueBool at Position: 19 path: '[0].children[0]' at position: 19", result.error);
    }
    
    /// <summary>Cover <see cref="GameDataSerializer.ReadTags"/></summary>
    [Test]
    public static void Test_Serializer_ReadScene_error_ReadTags()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var serializer  = new GameDataSerializer(store);
        
        var stream      = StringAsStream("[ {\"tags\":[1] } }");
        var result      = serializer.ReadScene(stream);
        AreEqual("expect tag string. was: ValueNumber at Position: 12 path: '[0].tags[0]' at position: 12", result.error);
    }
    #endregion
}
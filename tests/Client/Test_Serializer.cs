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
        var fileName    = TestUtils.GetBasePath() + "assets/read_scene.json";
        var file        = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var entityCount = serializer.ReadScene(file, out string error);
        
        IsNull(error);
        AreEqual(2, entityCount);
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
        file.Close();
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
    
    private static string MemoryStreamAsString(MemoryStream stream) {
        stream.Flush();
        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }
}
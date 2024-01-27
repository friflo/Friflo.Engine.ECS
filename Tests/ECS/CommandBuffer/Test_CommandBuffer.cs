using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Buffer;

#pragma warning disable CS0618 // Type or member is obsolete TODO remove

public static class Test_CommandBuffer
{
    [Test]
    public static void Test_CommandBuffer_components()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity(1);
        var pos1 = new Position(1, 1, 1);
        var pos2 = new Position(2, 2, 2);
        {
            var ecb = new CommandBuffer(store);
            
            // --- structural change: add Position
            ecb.AddComponent(1, pos1);
            ecb.SetComponent(1, pos2);
            //
            ecb.Playback();
        }
        
        AreEqual(pos2.x,            entity.GetComponent<Position>().x);
        AreSame(entity.Archetype,   store.GetArchetype(ComponentTypes.Get<Position>()));
        
        // --- handle remove after add
        {
            var ecb = new CommandBuffer(store);
            ecb.AddComponent   <Position>(1);
            ecb.RemoveComponent<Position>(1);
            
            ecb.Playback();
            
            IsFalse(entity.HasComponent<Position>());
        }
        
        // --- no structural change
        {
            var ecb = new CommandBuffer(store);
            ecb.AddComponent   <Position>(1);
            ecb.RemoveComponent<Position>(1);
            
            ecb.Playback();
            
            IsFalse(entity.HasComponent<Position>());
        }
        
        // --- archetype changes
        {
            entity.AddComponent(new Rotation());
            var ecb = new CommandBuffer(store);
            ecb.AddComponent(1, pos1);
            ecb.Playback();
            
            AreEqual(2, entity.Components.Count);
            AreEqual(1, entity.Position.x);
        }
    }
    
    [Test]
    public static void Test_CommandBuffer_IncreaseCommands()
    {
        int count       = 10; // 1_000_000 ~ #PC: 4384 ms
        var store       = new EntityStore(PidType.UsePidAsId);

        var entities    = new Entity[count];
        store.EnsureCapacity(count);
        for (int n = 0; n < count; n++) {
            entities[n] = store.CreateEntity();
        }
        QueueCommands(store, count);
        var sw = new Stopwatch();
        sw.Start();
        var start = Mem.GetAllocatedBytes();
        for (int i = 0; i < 100; i++) {
            QueueCommands(store, count);
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"EntityCommandBuffer.AddComponent() - duration: {sw.ElapsedMilliseconds} ms");
        
        for (int n = 0; n < count; n++) {
            Mem.AreEqual(n + 1, entities[n].Position.x);
        }
    }
    
    private static void QueueCommands(EntityStore store, int count) {

        var ecb = new CommandBuffer(store);
        for (int n = 0; n < count; n++) {
            ecb.AddComponent(n + 1, new Position(n + 1, 0, 0));    
        }
        ecb.Playback();
    }
    
    [Test]
    public static void Test_CommandBuffer_tags()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = new CommandBuffer(store);
        
        // TODO not implemented
        ecb.AddTag   <TestTag>(1);
        ecb.RemoveTag<TestTag>(1);
    }
}
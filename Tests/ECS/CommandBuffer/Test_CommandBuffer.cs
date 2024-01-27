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

        // --- empty ECB
        {
            var ecb = new CommandBuffer(store);
            ecb.Playback();
            AreEqual(1, store.EntityCount);
        }
        
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
    public static void Test_CommandBuffer_grow_component_commands()
    {
        int count       = 10; // 1_000_000 ~ #PC: 4633 ms
        var store       = new EntityStore(PidType.UsePidAsId);

        var entities    = new Entity[count];
        store.EnsureCapacity(count);
        for (int n = 0; n < count; n++) {
            entities[n] = store.CreateEntity();
        }
        QueueComponentCommands(store, count);
        var sw = new Stopwatch();
        sw.Start();
        var start = Mem.GetAllocatedBytes();
        for (int i = 0; i < 100; i++) {
            QueueComponentCommands(store, count);
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"EntityCommandBuffer.AddComponent() - duration: {sw.ElapsedMilliseconds} ms");
        
        for (int n = 0; n < count; n++) {
            Mem.AreEqual(n + 1, entities[n].Position.x);
        }
    }
    
    private static void QueueComponentCommands(EntityStore store, int count) {

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
        var entity  = store.CreateEntity();
        {
            var ecb     = new CommandBuffer(store);
            ecb.AddTag   <TestTag>(1);
            ecb.Playback();
            IsTrue(entity.Tags.Has<TestTag>());
        } {
            var ecb     = new CommandBuffer(store);
            ecb.RemoveTag   <TestTag>(1);
            ecb.Playback();
            IsFalse(entity.Tags.Has<TestTag>());
        } {
            var ecb     = new CommandBuffer(store);
            ecb.AddTags   (1, Tags.Get<TestTag2>());
            ecb.Playback();
            IsTrue(entity.Tags.Has<TestTag2>());
        } {
            var ecb     = new CommandBuffer(store);
            ecb.RemoveTags(1, Tags.Get<TestTag2>());
            ecb.Playback();
            IsFalse(entity.Tags.Has<TestTag2>());
        }
    }
    
    [Test]
    public static void Test_CommandBuffer_grow_tag_commands()
    {
        int count       = 10; // 1_000_000 ~ #PC: 3383 ms
        var store       = new EntityStore(PidType.UsePidAsId);

        var entities    = new Entity[count];
        store.EnsureCapacity(count);
        for (int n = 0; n < count; n++) {
            entities[n] = store.CreateEntity();
        }
        QueueTagCommands(store, count);
        var sw = new Stopwatch();
        sw.Start();
        var start = Mem.GetAllocatedBytes();
        for (int i = 0; i < 100; i++) {
            QueueTagCommands(store, count);
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"EntityCommandBuffer.AddTag() - duration: {sw.ElapsedMilliseconds} ms");
        
        for (int n = 0; n < count; n++) {
            Mem.IsTrue(entities[n].Tags.Has<TestTag>());
        }
    }
    
    private static void QueueTagCommands(EntityStore store, int count) {

        var ecb = new CommandBuffer(store);
        for (int n = 0; n < count; n++) {
            ecb.AddTag<TestTag>(n + 1);    
        }
        ecb.Playback();
    }
    
    [Test]
    public static void Test_CommandBuffer_prevent_reuse()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = new CommandBuffer(store);
        ecb.Playback();
        
        Throws<NullReferenceException>(() => {
            ecb.AddComponent<Position>(1);
        });
        
        Throws<NullReferenceException>(() => {
            ecb.AddTag<TestTag>(1);
        });
    }
    
    [Test]
    public static void Test_CommandBuffer_command_error()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = new CommandBuffer(store);
        ecb.AddComponent<Position>(1);
        var e = Throws<InvalidOperationException>(() => {
            ecb.Playback();    
        });
        AreEqual("Playback - entity not found. command: entity: 1 - Add [Position]", e!.Message);
    }
    
    [Test]
    public static void Test_CommandBuffer_tag_error()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = new CommandBuffer(store);
        ecb.AddTag<TestTag>(1);
        var e = Throws<InvalidOperationException>(() => {
            ecb.Playback();    
        });
        AreEqual("Playback - entity not found. command: entity: 1 - Add [#TestTag]", e!.Message);
    }
}
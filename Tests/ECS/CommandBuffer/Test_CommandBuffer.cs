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
            AreEqual(0, ecb.ComponentCommandsCount);
            ecb.Playback();
            AreEqual(1, store.EntityCount);
        }
        
        var pos1 = new Position(1, 1, 1);
        var pos2 = new Position(2, 2, 2);
        {
            var ecb = new CommandBuffer(store);
            
            // --- structural change: add Position
            ecb.AddComponent(1, pos1);
            AreEqual(1, ecb.ComponentCommandsCount);
            ecb.SetComponent(1, pos2);
            AreEqual(2, ecb.ComponentCommandsCount);
            AreEqual("component commands: 2  tag commands: 0", ecb.ToString());
            //
            ecb.Playback();
            AreEqual(0, ecb.ComponentCommandsCount);
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
            AreEqual(0, ecb.TagCommandsCount);
            ecb.AddTag   <TestTag>(1);
            AreEqual(1, ecb.TagCommandsCount);
            ecb.Playback();
            AreEqual(0, ecb.TagCommandsCount);
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
        
        var start = Mem.GetAllocatedBytes();
        QueueTagCommands(store, count);
        Mem.AssertNoAlloc(start);
        
        var sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < 100; i++) {
            QueueTagCommands(store, count);
        }
        Console.WriteLine($"EntityCommandBuffer.AddTag() - duration: {sw.ElapsedMilliseconds} ms");
        
        for (int n = 0; n < count; n++) {
            Mem.IsTrue(entities[n].Tags.Has<TestTag>());
        }
    }
    
    private static void QueueTagCommands(EntityStore store, int count)
    {
        var ecb = new CommandBuffer(store);
        for (int n = 0; n < count; n++) {
            ecb.AddTag<TestTag>(n + 1);    
        }
        ecb.Playback();
    }
    
    [Test]
    public static void Test_CommandBuffer_reuse()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = new CommandBuffer(store);
        ecb.ReuseBuffer = true;
        IsTrue(ecb.ReuseBuffer);
        ecb.Playback();

        ecb.CreateEntity();
        ecb.AddComponent<Position>(1);
        ecb.AddTag<TestTag>(1);
        ecb.Playback();
        
        ecb.ReturnBuffer();
        Assert_CommandBuffer_reuse_exceptions(ecb);
    }
    
    [Test]
    public static void Test_CommandBuffer_reuse_exceptions()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = new CommandBuffer(store);
        ecb.Playback();
        Assert_CommandBuffer_reuse_exceptions(ecb);
    }
    
    private static void Assert_CommandBuffer_reuse_exceptions(CommandBuffer ecb)
    {
        var e = Throws<InvalidOperationException>(() => {
            ecb.CreateEntity();
        });
        AreEqual("Cannot reuse CommandBuffer after Playback()", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddComponent<Position>(1);
        });
        AreEqual("Cannot reuse CommandBuffer after Playback()", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddTag<TestTag>(1);
        });
        AreEqual("Cannot reuse CommandBuffer after Playback()", e!.Message);
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
    
    [Test]
    public static void Test_CommandBuffer_entity_error()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = new CommandBuffer(store);
        ecb.DeleteEntity(42);
        var e = Throws<InvalidOperationException>(() => {
            ecb.Playback();    
        });
        AreEqual("Playback - entity not found. Delete entity, entity: 42", e!.Message);
    }
    
    [Test]
    public static void Test_CommandBuffer_CreateEntity()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        int id;
        {
            var ecb     = new CommandBuffer(store);
            id = ecb.CreateEntity();
            AreEqual(1, ecb.EntityCommandsCount);
            ecb.AddComponent<Position>(id);
            ecb.Playback();
            
            var entity = store.GetEntityById(id);
            IsTrue(     entity.HasComponent<Position>());
            AreEqual(0, ecb.EntityCommandsCount);
            AreEqual(1, store.EntityCount);
        } {
            var ecb     = new CommandBuffer(store);
            ecb.DeleteEntity(id);
            ecb.Playback();
            
            AreEqual(0, ecb.EntityCommandsCount);
            AreEqual(0, store.EntityCount);
        }
    }
    
    [Test]
    public static void Test_CommandBuffer_grow_CreateEntity()
    {
        int count       = 10; // 1_000_000 ~ #PC: 5002 ms
        var store       = new EntityStore(PidType.UsePidAsId);

        store.EnsureCapacity(count);
        QueueEntityCommands(store, count);
        
        var sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < 100; i++)
        { 
            var store2   = new EntityStore(PidType.UsePidAsId);
            store2.EnsureCapacity(count);
            // var start   = Mem.GetAllocatedBytes();
            QueueEntityCommands(store2, count);
            // Mem.AssertNoAlloc(start);  TODO check allocation
        }
        Console.WriteLine($"EntityCommandBuffer.AddTag() - duration: {sw.ElapsedMilliseconds} ms");
    }
    
    private static void QueueEntityCommands(EntityStore store, int count) {

        var ecb     = new CommandBuffer(store);
        for (int n = 0; n < count; n++) {
            ecb.CreateEntity();    
        }
        for (int n = 0; n < count; n++) {
            ecb.DeleteEntity(n + 1);    
        }
        ecb.Playback();
        Mem.AreEqual(0, store.EntityCount);
    }
}
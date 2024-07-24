using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Buffer {

public static class Test_CommandBuffer
{
    [Test]
    public static void Test_CommandBuffer_components()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity(1);

        // --- empty ECB
        {
            var ecb = store.GetCommandBuffer();
            AreEqual(0, ecb.ComponentCommandsCount);
            ecb.Playback();
            AreEqual(1, store.Count);
        }
        
        var pos1 = new Position(1, 1, 1);
        var pos2 = new Position(2, 2, 2);
        {
            var ecb = store.GetCommandBuffer();
            
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
            var ecb = store.GetCommandBuffer();
            ecb.AddComponent   <Position>(1);
            ecb.RemoveComponent<Position>(1);
            
            ecb.Playback();
            
            IsFalse(entity.HasComponent<Position>());
        }
        
        // --- no structural change
        {
            var ecb = store.GetCommandBuffer();
            ecb.AddComponent   <Position>(1);
            ecb.RemoveComponent<Position>(1);
            
            ecb.Playback();
            
            IsFalse(entity.HasComponent<Position>());
        }
        
        // --- archetype changes
        {
            entity.AddComponent(new Rotation());
            var ecb = store.GetCommandBuffer();
            ecb.AddComponent(1, pos1);
            ecb.Playback();
            
            AreEqual(2, entity.Components.Count);
            AreEqual(1, entity.Position.x);
        }
    }
    
    [Test]
    public static void Test_CommandBuffer_grow_component_commands()
    {
        int count       = 10; // 1_000_000 (* 100) ~ #PC: 4665 ms
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

        var ecb = store.GetCommandBuffer();
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
            var ecb     = store.GetCommandBuffer();
            AreEqual(0, ecb.TagCommandsCount);
            ecb.AddTag   <TestTag>(1);
            AreEqual(1, ecb.TagCommandsCount);
            ecb.Playback();
            AreEqual(0, ecb.TagCommandsCount);
            IsTrue(entity.Tags.Has<TestTag>());
        } {
            var ecb     = store.GetCommandBuffer();
            ecb.RemoveTag   <TestTag>(1);
            ecb.Playback();
            IsFalse(entity.Tags.Has<TestTag>());
        } {
            var ecb     = store.GetCommandBuffer();
            ecb.AddTags   (1, Tags.Get<TestTag2>());
            ecb.Playback();
            IsTrue(entity.Tags.Has<TestTag2>());
        } {
            var ecb     = store.GetCommandBuffer();
            ecb.RemoveTags(1, Tags.Get<TestTag2>());
            ecb.Playback();
            IsFalse(entity.Tags.Has<TestTag2>());
        }
    }
    
    [Test]
    public static void Test_CommandBuffer_grow_tag_commands()
    {
        int count       = 10; // 1_000_000 (* 100) ~ #PC: 3351 ms
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
        var ecb = store.GetCommandBuffer();
        for (int n = 0; n < count; n++) {
            ecb.AddTag<TestTag>(n + 1);    
        }
        ecb.Playback();
    }
    
    [Test]
    public static void Test_CommandBuffer_reuse()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        IsTrue(ecb.ReuseBuffer);
        ecb.Playback();

        ecb.CreateEntity();
        ecb.AddComponent<Position>(1);
        ecb.AddTag<TestTag>(1);
        ecb.Playback();
        
        ecb.ReturnBuffer();
        
        var e = Throws<InvalidOperationException>(() => {
            ecb.CreateEntity();
        });
        AreEqual("CommandBuffer - buffers returned to store", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddComponent<Position>(1);
        });
        AreEqual("CommandBuffer - buffers returned to store", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddTag<TestTag>(1);
        });
        AreEqual("CommandBuffer - buffers returned to store", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddScript(1, new TestScript1());
        });
        AreEqual("CommandBuffer - buffers returned to store", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddChild(1, 2);
        });
        AreEqual("CommandBuffer - buffers returned to store", e!.Message);
    }
    
    [Test]
    public static void Test_CommandBuffer_reuse_exceptions()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = store.GetCommandBuffer();
        ecb.Playback();
        
        var e = Throws<InvalidOperationException>(() => {
            ecb.CreateEntity();
        });
        AreEqual("Reused CommandBuffer after Playback(). ReuseBuffer: false", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.DeleteEntity(42);
        });
        AreEqual("Reused CommandBuffer after Playback(). ReuseBuffer: false", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddComponent<Position>(1);
        });
        AreEqual("Reused CommandBuffer after Playback(). ReuseBuffer: false", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            ecb.AddTag<TestTag>(1);
        });
        AreEqual("Reused CommandBuffer after Playback(). ReuseBuffer: false", e!.Message);
    }
    
    [Test]
    public static void Test_CommandBuffer_command_error()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = store.GetCommandBuffer();
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
        var ecb     = store.GetCommandBuffer();
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
        var ecb     = store.GetCommandBuffer();
        ecb.DeleteEntity(42);
        var e = Throws<InvalidOperationException>(() => {
            ecb.Playback();    
        });
        AreEqual("Playback - entity not found. Delete entity: 42", e!.Message);
    }
    
    [Test]
    public static void Test_CommandBuffer_CreateEntity()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        int id;
        {
            var ecb     = store.GetCommandBuffer();
            id = ecb.CreateEntity();
            AreEqual(1, ecb.EntityCommandsCount);
            ecb.AddComponent<Position>(id);
            ecb.Playback();
            
            var entity = store.GetEntityById(id);
            IsTrue(     entity.HasComponent<Position>());
            AreEqual(0, ecb.EntityCommandsCount);
            AreEqual(1, store.Count);
        } {
            var ecb     = store.GetCommandBuffer();
            ecb.DeleteEntity(id);
            ecb.Playback();
            
            AreEqual(0, ecb.EntityCommandsCount);
            AreEqual(0, store.Count);
        }
    }
    
    [Test]
    public static void Test_CommandBuffer_grow_CreateEntity()
    {
        int count       = 10; // 1_000_000 (* 100) ~ #PC: 5002 ms
        var store       = new EntityStore(PidType.UsePidAsId);

        store.EnsureCapacity(count);
        QueueEntityCommands(store, count);
        
        var sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < 100; i++)
        { 
            var store2   = new EntityStore(PidType.UsePidAsId);
            store2.EnsureCapacity(count);
            QueueEntityCommands(store2, count);
        }
        Console.WriteLine($"EntityCommandBuffer.AddTag() - duration: {sw.ElapsedMilliseconds} ms");
    }
    
    private static void QueueEntityCommands(EntityStore store, int count) {

        var ecb     = store.GetCommandBuffer();
        for (int n = 0; n < count; n++) {
            ecb.CreateEntity();    
        }
        for (int n = 0; n < count; n++) {
            ecb.DeleteEntity(n + 1);    
        }
        ecb.Playback();
        Mem.AreEqual(0, store.Count);
    }
    
    [Test]
    public static void Test_CommandBuffer_scripts()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity(1);
        var ecb     = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        AreEqual(0, ecb.ScriptCommandsCount);
        
        var script1 = new TestScript1();
        ecb.AddScript(entity.Id, script1);
        AreEqual(1, ecb.ScriptCommandsCount);
        ecb.Playback();
        AreEqual(0, ecb.ScriptCommandsCount);
        AreSame (script1, entity.GetScript<TestScript1>());
        NotNull (script1.Store);
        
        ecb.RemoveScript<TestScript1>(entity.Id);
        ecb.Playback();
        IsNull  (entity.GetScript<TestScript1>());
        IsNull  (script1.Store);
        
        ecb.AddScript(entity.Id, new TestScript2());
        ecb.RemoveScript<TestScript2>(entity.Id);
        AreEqual(2, ecb.ScriptCommandsCount);
        ecb.Playback();
        IsNull  (entity.GetScript<TestScript2>());
    }
    
    [Test]
    public static void Test_CommandBuffer_child_entities()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var ecb     = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        AreEqual(0, ecb.ChildCommandsCount);

        ecb.AddChild(entity1.Id, entity2.Id);
        AreEqual(1, ecb.ChildCommandsCount);
        ecb.Playback();
        AreEqual(0, ecb.ChildCommandsCount);
        AreEqual(1, entity1.ChildIds.Length);
        AreEqual(2, entity1.ChildIds[0]);
        
        ecb.RemoveChild(entity1.Id, entity2.Id);
        ecb.Playback();
        AreEqual(0, entity1.ChildIds.Length);
        
        ecb.AddChild   (entity1.Id, entity2.Id);
        ecb.RemoveChild(entity1.Id, entity2.Id);
        AreEqual(2, ecb.ChildCommandsCount);
        ecb.Playback();
        AreEqual(0, entity1.ChildIds.Length);
    }
    
    [Test]
    public static void Test_CommandBuffer_Playback_early_out()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        var child   = store.CreateEntity();
        var ecb     = store.GetCommandBuffer();
        AreSame(store, ecb.EntityStore);

        ecb.ReuseBuffer = false;
        ecb.Playback();
        
        var start = Mem.GetAllocatedBytes();
        ecb = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        ecb.Playback();
        Mem.AssertNoAlloc(start);
        
        // --- Ensure single modifications are applied by Playback()  
        ecb.AddComponent(entity.Id, new Position(1,2,3));
        ecb.Playback();
        AreEqual(new Position(1,2,3), entity.Position);
        
        ecb.RemoveComponent<Position>(entity.Id);
        ecb.Playback();
        IsFalse(entity.HasPosition);
        
        ecb.AddTag<TestTag>(entity.Id);
        ecb.Playback();
        IsTrue(entity.Tags.Has<TestTag>());
        
        ecb.AddScript(entity.Id, new TestScript1());
        ecb.Playback();
        NotNull(entity.GetScript<TestScript1>());
        
        ecb.AddChild(entity.Id, child.Id);
        ecb.Playback();
        AreEqual(1, entity.ChildCount);
        
        int  newEntity = ecb.CreateEntity();
        ecb.Playback();
        AreEqual(3, store.Count);
        
        ecb.DeleteEntity(newEntity);
        ecb.Playback();
        AreEqual(2, store.Count);
    }
    
    [Test]
    public static void Test_CommandBuffer_Clear()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var ecb     = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        ecb.Clear();    // cover early out
        
        var entity  = store.CreateEntity();
        var child   = store.CreateEntity();
        
        ecb.AddComponent(entity.Id, new Position());
        ecb.AddTag<TestTag>(entity.Id);
        ecb.AddScript(entity.Id, new TestScript1());
        ecb.AddChild(entity.Id, child.Id);
        ecb.CreateEntity();
        
        AreEqual(1, ecb.ComponentCommandsCount);
        AreEqual(1, ecb.TagCommandsCount);
        AreEqual(1, ecb.ScriptCommandsCount);
        AreEqual(1, ecb.ChildCommandsCount);
        AreEqual(1, ecb.EntityCommandsCount);
        
        ecb.Clear();
        AreEqual(0, ecb.ComponentCommandsCount);
        AreEqual(0, ecb.TagCommandsCount);
        AreEqual(0, ecb.ScriptCommandsCount);
        AreEqual(0, ecb.ChildCommandsCount);
        AreEqual(0, ecb.EntityCommandsCount);
    }
}

}
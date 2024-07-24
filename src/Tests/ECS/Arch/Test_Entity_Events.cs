using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {


public static class Test_Entity_Events
{
    [Test]
    public static void Test_Entity_Events_OnTagsChanged()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        
        var entity1EventCount = 0;
        Action<TagsChanged> tagsChanged1 = (args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Tags: [#TestTag]", args.ToString()); break;
                default:    Fail("unexpected"); break;
            }
        };
        Action<TagsChanged> tagsChanged2 = (_)    => { };
        
        entity1.OnTagsChanged -= tagsChanged1;  // remove event handler not added before. 
        entity1.OnTagsChanged += tagsChanged1;
        entity1.OnTagsChanged -= tagsChanged1;
        
        entity2.OnTagsChanged -= tagsChanged1;  // cover missing element in Dictionary<int, Action<TArgs>>

        entity1.OnTagsChanged += tagsChanged1;
        entity1.OnTagsChanged += tagsChanged2;  // cover adding action to Dictionary<int, Action<TArgs>>
        
        var handlers = entity1.DebugEventHandlers;
        AreEqual("event types: 1, handlers: 2", handlers.ToString());
        AreEqual(1,                             handlers.TypeCount);
        AreEqual(1,                             handlers.Array.Length);
        AreEqual(2,                             handlers.HandlerCount);
        var handler0 = handlers[0]; 
        AreEqual("TagsChanged - Count: 2",      handler0.ToString());
        AreEqual(2,                             handler0.Count);
        AreEqual(typeof(TagsChanged),           handler0.Type);
        AreEqual(DebugEntityEventKind.Event,    handler0.Kind);

        entity1.AddTag<TestTag>();
        entity2.AddTag<TestTag>();
        
        entity1.OnTagsChanged -= tagsChanged1;
        entity1.OnTagsChanged -= tagsChanged2;
        
        AreEqual("event types: 0, handlers: 0", entity1.DebugEventHandlers.ToString());
        AreEqual("event types: 0, handlers: 0", entity2.DebugEventHandlers.ToString());
        
        entity1.AddTag<TestTag>();
        entity2.AddTag<TestTag>();
        
        AreEqual(1, entity1EventCount);
    }
    
    [Test]
    public static void Test_Entity_Events_OnComponentChanged()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);

        var entity1EventCount = 0;
        Action<ComponentChanged> onComponentChanged = (args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Component: [Position]", args.ToString()); break;
            }
        };
        entity1.OnComponentChanged += onComponentChanged; 

        entity1.AddComponent<Position>();
        entity2.AddComponent<Position>();
        
        var start = Mem.GetAllocatedBytes();
        entity1.AddComponent<Position>(); // event handler invocation executes without allocation
        Mem.AssertNoAlloc(start);
        
        entity1.OnComponentChanged -= onComponentChanged; 
        
        entity1.AddComponent<Rotation>();
        
        AreEqual(2, entity1EventCount);
    }
    
    [Test]
    public static void Test_Entity_Events_OnScriptChanged()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);

        var entity1EventCount = 0;
        Action<ScriptChanged> onScriptChanged = (args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Script: [*TestScript1]", args.ToString()); break;
                default:    Fail("unexpected"); break;
            }
        };
        entity1.OnScriptChanged += onScriptChanged; 

        entity1.AddScript(new TestScript1());
        entity2.AddScript(new TestScript2());
        
        entity1.OnScriptChanged -= onScriptChanged; 
        
        entity1.AddScript(new TestScript1());
        
        AreEqual(1, entity1EventCount);
    }
    
    [Test]
    public static void Test_Entity_Events_OnChildEntitiesChanged()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        var child10     = store.CreateEntity(10);
        var child11     = store.CreateEntity(11);
        var child12     = store.CreateEntity(12);

        var entity1EventCount = 0;
        Action<ChildEntitiesChanged> onChildEntitiesChanged = (args)    => {
            switch (entity1EventCount++) {
                case 0:     AreEqual("entity: 1 - event > Add Child[0] = 10", args.ToString()); break;
                default:    Fail("unexpected"); break;
            }
        };
        entity1.OnChildEntitiesChanged += onChildEntitiesChanged; 

        entity1.AddChild(child10);
        entity2.AddChild(child11);
        
        entity1.OnChildEntitiesChanged -= onChildEntitiesChanged; 
        
        entity1.AddChild(child12);
        
        AreEqual(1, entity1EventCount);
    }
    
    /// <summary>
    /// Note: Add signal handlers with different event types in specified order<b/>
    /// to cover null items in <see cref="EntityStore.Intern.signalHandlers"/>
    /// </summary>
    [Test]
    public static void Test_Entity_Signals()
    {
        {
            // --- See: Note
            var store2  = new EntityStore(PidType.RandomPids);
            var entity2 = store2.CreateEntity(2);
            entity2.RemoveSignalHandler<MyEvent1>(_ => { });
            
            entity2.AddSignalHandler<MyEvent1>(_ => { });
            entity2.AddSignalHandler<MyEvent2>(_ => { });
            entity2.AddSignalHandler<MyEvent3>(_ => { });
            AreEqual("event types: 3, handlers: 3", entity2.DebugEventHandlers.ToString());
            
            var entity3 = store2.CreateEntity(3);
            entity3.AddSignalHandler   <MyEvent2>(_ => { });
            entity3.RemoveSignalHandler<MyEvent1>(_ => { });
            AreEqual("event types: 1, handlers: 1", entity3.DebugEventHandlers.ToString());
        }
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity(1);

        var entity1SignalCount = 0;
        Action<Signal<MyEvent2>> onMyEvent = (signal)    => {
            switch (entity1SignalCount++) {
                case 0:
                    Mem.AreEqual(1,                                 signal.Entity.Id);
                    Mem.AreEqual(1,                                 signal.EntityId);
                    Mem.AreSame (store,                             signal.Store);
                    Mem.AreEqual(42,                                signal.Event.value);
                    Mem.AreEqual("entity: 1 - signal > MyEvent2",   signal.ToString());
                    break;
                case 1:
                    Mem.AreEqual(43,                                signal.Event.value);
                    break;
                default:
                    Fail("unexpected");
                    break;
            }
        };
        Action<Signal<MyEvent2>> onMyEvent2 = (_) => { };
        AreSame(onMyEvent,  entity.AddSignalHandler(onMyEvent));
        AreSame(onMyEvent2, entity.AddSignalHandler(onMyEvent2));
        
        entity.EmitSignal(new MyEvent2 { value = 42 });    // handler allocates memory for assertion
        var start = Mem.GetAllocatedBytes();
        entity.EmitSignal(new MyEvent2 { value = 43 });
        Mem.AssertNoAlloc(start);

        IsTrue (entity.RemoveSignalHandler(onMyEvent));
        IsTrue (entity.RemoveSignalHandler(onMyEvent2));
        IsFalse(entity.RemoveSignalHandler(onMyEvent)); // remove already removed handler
        entity.EmitSignal(new MyEvent2 { value = 13 });
        
        AreEqual(2, entity1SignalCount);
        
        entity.EmitSignal(new MyEvent1()); // no signal handler added
        entity.EmitSignal(new MyEvent3()); // no signal handler added
        
        // --- check allocation for common use case: entity has no event / signal handlers
        var start2      = Mem.GetAllocatedBytes();
        var handlers    = entity.DebugEventHandlers;
        Mem.AreEqual(0,                             handlers.TypeCount);
        Mem.AreEqual(0,                             handlers.HandlerCount);
        Mem.AreEqual("event types: 0, handlers: 0", handlers.ToString());
        Mem.AssertNoAlloc(start2);
    }
    
    [Test]
    public static void Test_Events_EntityHandlers()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        AreEqual("event types: 0, handlers: 0", entity.DebugEventHandlers.ToString());
        
        entity.OnComponentChanged     += _ => { };
        entity.OnComponentChanged     += _ => { };
        entity.OnTagsChanged          += _ => { };
        entity.OnScriptChanged        += _ => { };
        entity.OnChildEntitiesChanged += _ => { };
        entity.AddSignalHandler<MyEvent1>(_ => { });
        
        var handlers = entity.DebugEventHandlers;
        AreEqual(5, handlers.TypeCount);
        AreEqual(6, handlers.HandlerCount);
        AreEqual(5, handlers.Array.Length);
        AreEqual("event types: 5, handlers: 6", handlers.ToString());
        
        AreEqual(typeof(ComponentChanged),      handlers[0].Type);
        AreEqual(typeof(TagsChanged),           handlers[1].Type);
        AreEqual(typeof(ScriptChanged),         handlers[2].Type);
        AreEqual(typeof(ChildEntitiesChanged),  handlers[3].Type);
        AreEqual(typeof(MyEvent1),               handlers[4].Type);
        
        AreEqual("ComponentChanged - Count: 2",       handlers[0].ToString());
        AreEqual("TagsChanged - Count: 1",            handlers[1].ToString());
        AreEqual("ScriptChanged - Count: 1",          handlers[2].ToString());
        AreEqual("ChildEntitiesChanged - Count: 1",   handlers[3].ToString());
        AreEqual("Signal: MyEvent1 - Count: 1",       handlers[4].ToString());
        
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var count = 10; // 100_000_000 ~ #PC: 728 ms
        for (int n = 0; n < count; n++) {
            entity.EmitSignal(new MyEvent1());
        }
        Console.WriteLine($"EmitSignal - count: {count}, duration: {stopwatch.ElapsedMilliseconds}");
    }
}

public struct MyEvent1 { public int value; }
public struct MyEvent2 { public int value; }
public struct MyEvent3 { }

}
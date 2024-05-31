using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.ECS.Arch;
using static NUnit.Framework.Assert;

namespace Internal.ECS {

// ReSharper disable once InconsistentNaming
public static class Test_Events
{
    [Test]
    public static void Test_Events_RemoveHandlersOnDelete()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity1 = store.CreateEntity(1);
        
        entity1.OnComponentChanged     += _ => { Fail("unexpected"); };
        entity1.OnComponentChanged     += _ => { Fail("unexpected"); };
        entity1.OnTagsChanged          += _ => { Fail("unexpected"); };
        entity1.OnScriptChanged        += _ => { Fail("unexpected"); };
        entity1.OnChildEntitiesChanged += _ => { Fail("unexpected"); };
        entity1.AddSignalHandler<MyEvent>(_ => { Fail("unexpected"); });
        
        var entity2  = store.CreateEntity(2);
        var eventCount = 0;
        entity2.OnComponentChanged     += _ => { eventCount++; };
        entity2.OnTagsChanged          += _ => { eventCount++; };
        entity2.OnScriptChanged        += _ => { eventCount++; };
        entity2.OnChildEntitiesChanged += _ => { eventCount++; };
        entity2.AddSignalHandler<MyEvent>(_ => { eventCount++; });
        
        entity1.DeleteEntity();
        AreEqual("event types: 0, handlers: 0", entity1.DebugEventHandlers.ToString());
        
        entity2.AddTag<TestTag>();
        entity2.AddComponent<Position>();
        entity2.AddScript(new TestScript1());
        var entity3 = store.CreateEntity(3);
        entity2.AddChild(entity3); // fires add TreeNode component event
        entity2.EmitSignal(new MyEvent());
        entity2.DeleteEntity();
        AreEqual(6, eventCount);
        
        entity1.EmitSignal(new MyEvent());
        EntityStoreBase.AssertEventDelegatesNull(store);
        EntityStore.    AssertEventDelegatesNull(store);
    }
    
    private struct MyEvent {}
    
    
    [Test]
    public static void Test_Events_add_multiple_signal_handler()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        var countEvent1  = 0;
        var countEvent2A = 0;
        var countEvent2B = 0;
        Action<Signal<MyEvent1>> handler1 = signal => {
            switch (countEvent1++) {
                case 0: AreEqual(10, signal.Event.value); break;
                case 1: AreEqual(30, signal.Event.value); break;
            }
            AreEqual("entity: 1 - signal > MyEvent1", signal.ToString());
        };
        Action<Signal<MyEvent2>> handler2A = signal => {
            switch (countEvent2A++) {
                case 0: AreEqual(11, signal.Event.value); break;
                case 1: AreEqual(12, signal.Event.value); break;
            }
            AreEqual("entity: 1 - signal > MyEvent2", signal.ToString());
        };
        Action<Signal<MyEvent2>> handler2B = signal => {
            countEvent2B++;
            AreEqual(11, signal.Event.value);
            AreEqual("entity: 1 - signal > MyEvent2", signal.ToString());
        };
        entity.AddSignalHandler(handler1);
        entity.AddSignalHandler(handler2A);
        entity.AddSignalHandler(handler2B);
        
        var handlers = entity.DebugEventHandlers;
        AreEqual(3, handlers.HandlerCount);
        AreEqual(2, handlers.TypeCount);
        
        entity.EmitSignal(new MyEvent1 { value = 10 });
        entity.EmitSignal(new MyEvent2 { value = 11 });
        
        entity.RemoveSignalHandler(handler2B);
        handlers = entity.DebugEventHandlers;
        AreEqual(2, handlers.HandlerCount);
        AreEqual(2, handlers.TypeCount);
        entity.EmitSignal(new MyEvent2 { value = 12 });
        
        entity.RemoveSignalHandler(handler2A);
        handlers = entity.DebugEventHandlers;
        AreEqual(1, handlers.HandlerCount);
        AreEqual(1, handlers.TypeCount);
        
        entity.RemoveSignalHandler(handler1);
        handlers = entity.DebugEventHandlers;
        AreEqual(0, handlers.HandlerCount);
        AreEqual(0, handlers.TypeCount);
        
        // All signal handlers removed. EmitSignal() will not call any delegate  
        entity.EmitSignal(new MyEvent1 { value = 20 });
        entity.EmitSignal(new MyEvent2 { value = 21 });
        
        // add handler1 again
        entity.AddSignalHandler(handler1);
        entity.EmitSignal(new MyEvent1 { value = 30 });
        entity.EmitSignal(new MyEvent2 { value = 31 });
        
        AreEqual(2, countEvent1);
        AreEqual(2, countEvent2A);
        AreEqual(1, countEvent2B);
    }
    
    [Test]
    public static void Test_Events_add_multiple_ComponentChanged_handler()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        var countEvent1  = 0;
        var countEvent2A = 0;
        var countEvent2B = 0;
        Action<ComponentChanged> handler1 = changed => {
            var value = changed.Component<MyComponent1>().a;
            switch (countEvent1++) {
                case 0: AreEqual(10, value); break;
                case 1: AreEqual(20, value); break;
                case 2: AreEqual(40, value); break;
            }
        };
        Action<ComponentChanged> handler2A = changed => {
            var value = changed.Component<MyComponent1>().a;
            switch (countEvent2A++) {
                case 0: AreEqual(10, value); break;
                case 1: AreEqual(20, value); break;
            }
        };
        Action<ComponentChanged> handler2B = changed => {
            var value = changed.Component<MyComponent1>().a;
            switch (countEvent2B++) {
                case 0: AreEqual(10, value); break;
            }
        };
        entity.OnComponentChanged += handler1;
        entity.OnComponentChanged += handler2A;
        entity.OnComponentChanged += handler2B;
        
        var handlers = entity.DebugEventHandlers;
        AreEqual(3, handlers.HandlerCount);
        AreEqual(1, handlers.TypeCount);
        
        entity.AddComponent(new MyComponent1{ a = 10 });
        
        entity.OnComponentChanged -= handler2B;
        handlers = entity.DebugEventHandlers;
        AreEqual(2, handlers.HandlerCount);
        AreEqual(1, handlers.TypeCount);
        entity.AddComponent(new MyComponent1{ a = 20 });
        
        entity.OnComponentChanged -= handler2A;
        handlers = entity.DebugEventHandlers;
        AreEqual(1, handlers.HandlerCount);
        AreEqual(1, handlers.TypeCount);
        
        entity.OnComponentChanged -= handler1;
        handlers = entity.DebugEventHandlers;
        AreEqual(0, handlers.HandlerCount);
        AreEqual(0, handlers.TypeCount);
        
        // All event handlers removed. AddComponent() will not call any delegate  
        entity.AddComponent(new MyComponent1{ a = 30 });
        
        // add handler1 again
        entity.OnComponentChanged += handler1;
        entity.AddComponent(new MyComponent1{ a = 40 });
        
        AreEqual(3, countEvent1);
        AreEqual(2, countEvent2A);
        AreEqual(1, countEvent2B);
    }
}

}
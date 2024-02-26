using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
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
        entity2.AddChild(store.CreateEntity(3));
        entity2.EmitSignal(new MyEvent());
        entity2.DeleteEntity();
        AreEqual(5, eventCount);
        
        entity1.EmitSignal(new MyEvent());
        EntityStoreBase.AssertEventDelegatesNull(store);
        EntityStore.    AssertEventDelegatesNull(store);
    }
    
    private struct MyEvent {}
}

}
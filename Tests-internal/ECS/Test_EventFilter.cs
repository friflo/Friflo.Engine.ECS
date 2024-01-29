using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

namespace Internal.ECS;

// ReSharper disable once InconsistentNaming
public static class Test_EventFilter
{
    [Test]
    public static void Test_EventFilter_Internal()
    {
        var schema = EntityStore.GetEntitySchema();
        
        var componentType   = schema.ComponentTypeByType[typeof(Position)];
        var tagType         = schema.TagTypeByType[typeof(TestTag)];
        
        var componentEvents = new EntityEvents(componentType);
        var tagEvents       = new EntityEvents(tagType);
        
        AreEqual("[Position] events: 0", componentEvents.ToString());
        AreEqual("[#TestTag] events: 0", tagEvents.ToString());
    }
    
    [Test]
    public static void Test_EventFilter_Init()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var recorder    = store.EventRecorder;
        
        IsFalse(recorder.Enabled);
        AreEqual("disabled", recorder.ToString());
        
        // --- enable event recording
        recorder.Enabled = true;
        IsTrue(recorder.Enabled);
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.AddComponent<Position>();
        entity1.AddTag<TestTag>();
        
        entity2.AddComponent<Position>();
        entity2.RemoveComponent<Position>();
        entity2.AddTag<TestTag>();
        entity2.RemoveTag<TestTag>();
        
        var positionAdded = recorder.ComponentAddedEntities<Position>();
        AreEqual(2, positionAdded.Length);
        AreEqual(1, positionAdded[0]);
        AreEqual(2, positionAdded[1]);
        
        AreEqual(1, recorder.ComponentRemovedEntities<Position>().Length);
        AreEqual(2, recorder.TagAddedEntities<TestTag>().Length);
        AreEqual(1, recorder.TagRemovedEntities<TestTag>().Length);
        
        AreEqual(6, recorder.AllEventsCount);
        AreEqual("All events: 6", recorder.ToString());
        
        
        // --- disable event recording
        recorder.Enabled = false;
        IsFalse(recorder.Enabled);
        
        recorder.Reset();
        entity2.AddComponent<Position>();
        
        AreEqual(0, recorder.ComponentAddedEntities<Position>().Length);
        AreEqual(0, recorder.ComponentRemovedEntities<Position>().Length);
        AreEqual(0, recorder.TagAddedEntities<TestTag>().Length);
        AreEqual(0, recorder.TagRemovedEntities<TestTag>().Length);
        AreEqual(6, recorder.AllEventsCount);
    }
}
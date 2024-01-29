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
        recorder.Enabled = true;
        IsTrue(recorder.Enabled);
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.AddComponent<Position>();
        entity2.AddComponent<Position>();
        
        AreEqual(2, recorder.AllEventsCount);
        var positionAdded = recorder.ComponentAddedEntities<Position>();
        AreEqual(2, positionAdded.Length);
        AreEqual(1, positionAdded[0]);
        AreEqual(2, positionAdded[1]);
        
        AreEqual("All events: 2", recorder.ToString());
    }
}
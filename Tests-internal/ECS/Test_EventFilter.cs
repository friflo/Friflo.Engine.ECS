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
    public static void Test_EventFilter_filter_ToString()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var recorder        = store.EventRecorder;
        
        var positionAdded   = new EventFilter(recorder);
        var positionRemoved = new EventFilter(recorder);
        var tagAdded        = new EventFilter(recorder);
        var tagRemoved      = new EventFilter(recorder);
        
        positionAdded.  ComponentAdded  <Position>();
        positionRemoved.ComponentRemoved<Position>();
        tagAdded.  TagAdded  <TestTag>();
        tagRemoved.TagRemoved<TestTag>();
        
        AreEqual("added: [Position]",   positionAdded.ToString());
        AreEqual("removed: [Position]", positionRemoved.ToString());
        AreEqual("added: [#TestTag]",   tagAdded.ToString());
        AreEqual("removed: [#TestTag]", tagRemoved.ToString());
        
        var filter = new EventFilter(recorder);
        filter.ComponentAdded  <Position>();
        filter.ComponentRemoved<Position>();
        filter.TagAdded  <TestTag>();
        filter.TagRemoved<TestTag>();
        
        AreEqual("added: [Position, #TestTag],  removed: [Position, #TestTag]", filter.ToString());
    }
    
    [Test]
    public static void Test_EventFilter_create_recorder()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        _               = store.EventRecorder;
        var recorder    = store.EventRecorder; // cover getting already instantiated recorder
        
        IsFalse(recorder.Enabled);
        AreEqual("disabled", recorder.ToString());
        
        // --- enable event recording
        recorder.Enabled = true;
        recorder.Enabled = true;    // cover enabling if already enabled
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
    
    [Test]
    public static void Test_EventFilter_filter_events()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var recorder        = store.EventRecorder;
        
        var positionAdded   = new EventFilter(recorder);
        var positionRemoved = new EventFilter(recorder);
        var tagAdded        = new EventFilter(recorder);
        var tagRemoved      = new EventFilter(recorder);
        recorder.Enabled = true;
        
        positionAdded.  ComponentAdded  <Position>();
        positionRemoved.ComponentRemoved<Position>();
        tagAdded.  TagAdded  <TestTag>();
        tagRemoved.TagRemoved<TestTag>();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        entity2.AddComponent<Position>();
        entity2.AddTag<TestTag>();
        
        entity3.AddComponent   <Position>();
        entity3.RemoveComponent<Position>();
        entity3.AddTag   <TestTag>();
        entity3.RemoveTag<TestTag>();
        
        IsFalse(positionAdded.  Filter(entity1.Id));
        IsFalse(positionRemoved.Filter(entity1.Id));
        IsFalse(tagAdded.       Filter(entity1.Id));
        IsFalse(tagRemoved.     Filter(entity1.Id));
        
        IsTrue (positionAdded.  Filter(entity2.Id));
        IsFalse(positionRemoved.Filter(entity2.Id));
        IsTrue (tagAdded.       Filter(entity2.Id));
        IsFalse(tagRemoved.     Filter(entity2.Id));
        
        IsTrue (positionAdded.  Filter(entity3.Id));
        IsTrue (positionRemoved.Filter(entity3.Id));
        IsTrue (tagAdded.       Filter(entity3.Id));
        IsTrue (tagRemoved.     Filter(entity3.Id));
    }
}
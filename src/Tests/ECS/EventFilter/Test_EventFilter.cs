using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.ECS.Filter {

// ReSharper disable once InconsistentNaming
public static class Test_EventFilter
{
    [Test]
    public static void Test_EventFilter_Internal()
    {
        var schema = EntityStore.GetEntitySchema();
        
        var positionType = schema.ComponentTypeByType[typeof(Position)];
        var ev = new EntityEvent {
            Id          = 1,
            TypeIndex   = (byte)positionType.StructIndex,
            Action      = EntityEventAction.Added,
            Kind        = SchemaTypeKind.Component
        };
        AreEqual("id: 1 - Added [Position]", ev.ToString());
        
        var tagType = schema.TagTypeByType[typeof(TestTag)];
        ev = new EntityEvent {
            Id          = 2,
            TypeIndex   = (byte)tagType.TagIndex,
            Action      = EntityEventAction.Removed,
            Kind        = SchemaTypeKind.Tag
        };
        AreEqual("id: 2 - Removed [#TestTag]", ev.ToString());
        
        ev.Kind = (SchemaTypeKind)99;
        Throws<InvalidOperationException>(() => {
            _ = ev.ToString();
        });
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
        entity1.AddComponent    <Position>();
        entity1.AddTag          <TestTag>();
        
        entity2.AddComponent    <Position>();
        entity2.RemoveComponent <Position>();
        entity2.AddTag          <TestTag>();
        entity2.RemoveTag       <TestTag>();
        
        var componentEvents = recorder.ComponentEvents;
        AreEqual(3, componentEvents.Length);
        AreEqual("id: 1 - Added [Position]",    componentEvents[0].ToString());
        AreEqual("id: 2 - Added [Position]",    componentEvents[1].ToString());
        AreEqual("id: 2 - Removed [Position]",  componentEvents[2].ToString());
        
        var tagEvents = recorder.TagEvents;
        AreEqual(3, tagEvents.Length);
        AreEqual("id: 1 - Added [#TestTag]",    tagEvents[0].ToString());
        AreEqual("id: 2 - Added [#TestTag]",    tagEvents[1].ToString());
        AreEqual("id: 2 - Removed [#TestTag]",  tagEvents[2].ToString());
        
        AreEqual(6, recorder.AllEventsCount);
        AreEqual("All events: 6", recorder.ToString());
        
        
        // --- disable event recording
        recorder.Enabled = false;
        IsFalse(recorder.Enabled);
        
        recorder.ClearEvents();
        entity2.AddComponent<Position>();
        
        AreEqual(0, recorder.ComponentEvents.Length);
        AreEqual(0, recorder.TagEvents.      Length);
        AreEqual(6, recorder.AllEventsCount);
    }
    
    [Test]
    public static void Test_EventFilter_filter_events()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var recorder        = store.EventRecorder;
        
        var positionAdded   = new EventFilter(recorder);
        var positionRemoved = new EventFilter(recorder);
        var nameAdded       = new EventFilter(recorder);
        
        var tagAdded        = new EventFilter(recorder);
        var tagRemoved      = new EventFilter(recorder);
        var tag2Added       = new EventFilter(recorder);
        recorder.Enabled = true;
        
        positionAdded.  ComponentAdded  <Position>();
        positionRemoved.ComponentRemoved<Position>();
        nameAdded.      ComponentAdded  <EntityName>();
        
        tagAdded.       TagAdded        <TestTag>();
        tagRemoved.     TagRemoved      <TestTag>();
        tag2Added.      TagAdded        <TestTag2>();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        entity2.AddComponent    <Position>();
        entity2.AddTag          <TestTag>();
        
        entity3.AddComponent    <Position>();
        entity3.RemoveComponent <Position>();
        entity3.AddTag          <TestTag>();
        entity3.RemoveTag       <TestTag>();
        
        IsFalse(positionAdded.  HasEvent(entity1.Id));
        IsFalse(positionRemoved.HasEvent(entity1.Id));
        IsFalse(tagAdded.       HasEvent(entity1.Id));
        IsFalse(tagRemoved.     HasEvent(entity1.Id));
        IsFalse(nameAdded.      HasEvent(entity1.Id));
        IsFalse(tag2Added.      HasEvent(entity1.Id));
        
        IsTrue (positionAdded.  HasEvent(entity2.Id));
        IsFalse(positionRemoved.HasEvent(entity2.Id));
        IsTrue (tagAdded.       HasEvent(entity2.Id));
        IsFalse(tagRemoved.     HasEvent(entity2.Id));
        IsFalse(nameAdded.      HasEvent(entity2.Id));
        IsFalse(tag2Added.      HasEvent(entity2.Id));
        
        IsFalse(positionAdded.  HasEvent(entity3.Id));
        IsTrue (positionRemoved.HasEvent(entity3.Id));
        IsFalse(tagAdded.       HasEvent(entity3.Id));
        IsTrue (tagRemoved.     HasEvent(entity3.Id));
        IsFalse(nameAdded.      HasEvent(entity3.Id));
        IsFalse(tag2Added.      HasEvent(entity3.Id));
    }
    
    [Test]
    public static void Test_EventFilter_query_filter()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        var entity4 = store.CreateEntity();

        var query = store.Query();
        query.EventFilter.ComponentAdded<Position>();
        query.EventFilter.ComponentRemoved<Position>();
        query.EventFilter.TagAdded<TestTag>();
        query.EventFilter.TagRemoved<TestTag>();
        
        store.EventRecorder.Enabled = true;
        
        entity1.AddComponent    <Position>();
        entity2.AddComponent    <Position>();
        entity3.AddComponent    <Position>();
        entity3.RemoveComponent <Position>();
        
        entity1.AddTag          <TestTag>();
        entity2.AddTag          <TestTag>();
        entity3.AddTag          <TestTag>();
        entity3.RemoveTag       <TestTag>();
        entity3.AddTag          <TestTag2>();
        
        entity4.AddComponent    <Rotation>();
        entity4.AddTag          <TestTag2>();

        AreEqual(4, query.Entities.Count);
        
        foreach (var entity in query.Entities)
        {
            bool hasEvent = query.HasEvent(entity.Id);
            switch (entity.Id) {
                case 1:
                case 2:
                case 3:
                    IsTrue(hasEvent);
                    continue;
                case 4:
                    IsFalse(hasEvent);
                    continue;
                default:
                    throw new InvalidOperationException($"unexpected entity: {entity.Id}");
            }
        }
    }
    
    [Test]
    public static void Test_EventFilter_query_empty_filter()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity1 = store.CreateEntity();

        var query = store.Query();
        store.EventRecorder.Enabled = true;
        entity1.AddComponent    <Position>();
        AreEqual(1, store.EventRecorder.AllEventsCount);
        
        int count = 0;
        foreach (var entity in query.Entities)
        {
            count++;
            IsFalse(query.HasEvent(entity.Id));
        }
        AreEqual(1, count);
    }
    
    [Test]
    public static void Test_EventFilter_filter_events_perf()
    {
        int count = 10;
        // 10_000_000   EventRecorder ~ #PC: 442 ms
        //              EventFilter   ~ #PC: 240 ms
        
        var store           = new EntityStore(PidType.UsePidAsId);
        var recorder        = store.EventRecorder;
        recorder.Enabled    = true;
        for (int n = 0; n < count; n++) {
            store.CreateEntity();
        }
        
        // i: 0 is warmup
        for (int i = 0; i < 2; i++)
        {
            for (int n = 1; n <= count; n++) {
                var entity = store.GetEntityById(n);
                entity.RemoveComponent<Position>();
            }
            recorder.ClearEvents();
            
            // --- add components
            var sw = new Stopwatch();
            sw.Start();
            for (int n = 1; n <= count; n++) {
                var entity = store.GetEntityById(n);
                entity.AddComponent<Position>();
            }
            if (i == 1) Console.WriteLine($"EvenRecorder - count: {count},  duration: {sw.ElapsedMilliseconds} ms");
            AreEqual(count, recorder.ComponentEvents.Length);
        
            // --- filter component events
            var positionAdded = new EventFilter(recorder);
            positionAdded.ComponentAdded<Position>();
            var sw2 = new Stopwatch();
            sw2.Start();

            for (int n = 1; n <= count; n++) {
                Mem.IsTrue(positionAdded.HasEvent(n));
            }
            if (i == 1) Console.WriteLine($"EventFilter  - count: {count},  duration: {sw2.ElapsedMilliseconds} ms\n");
        }
    }
}

}
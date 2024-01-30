using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.ECS.Filter;

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
        
        recorder.Reset();
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
        
        IsFalse(positionAdded.  Filter(entity1.Id));
        IsFalse(positionRemoved.Filter(entity1.Id));
        IsFalse(tagAdded.       Filter(entity1.Id));
        IsFalse(tagRemoved.     Filter(entity1.Id));
        IsFalse(nameAdded.      Filter(entity1.Id));
        IsFalse(tag2Added.      Filter(entity1.Id));
        
        IsTrue (positionAdded.  Filter(entity2.Id));
        IsFalse(positionRemoved.Filter(entity2.Id));
        IsTrue (tagAdded.       Filter(entity2.Id));
        IsFalse(tagRemoved.     Filter(entity2.Id));
        IsFalse(nameAdded.      Filter(entity2.Id));
        IsFalse(tag2Added.      Filter(entity2.Id));
        
        IsFalse(positionAdded.  Filter(entity3.Id));
        IsTrue (positionRemoved.Filter(entity3.Id));
        IsFalse(tagAdded.       Filter(entity3.Id));
        IsTrue (tagRemoved.     Filter(entity3.Id));
        IsFalse(nameAdded.      Filter(entity3.Id));
        IsFalse(tag2Added.      Filter(entity3.Id));
    }
    
    [Test]
    public static void Test_EventFilter_filter_events_perf()
    {
        int count = 10;
        // 10_000_000   EventRecorder ~ #PC:  677 ms
        //              EventFilter   ~ #PC: 1123 ms
        var store           = new EntityStore(PidType.UsePidAsId);
        var recorder        = store.EventRecorder;
        recorder.Enabled    = true;
        
        for (int n = 0; n < count; n++) {
            store.CreateEntity();
        } {
            var sw = new Stopwatch();
            sw.Start();
            for (int n = 1; n <= count; n++) {
                var entity = store.GetEntityById(n);
                entity.AddComponent<Position>();
            }
            Console.WriteLine($"EvenRecorder - count: {count},  duration: {sw.ElapsedMilliseconds} ms");
            AreEqual(count, recorder.AllEventsCount);
        } {
            var positionAdded = new EventFilter(recorder);
            positionAdded.ComponentAdded<Position>();
            var sw = new Stopwatch();
            sw.Start();

            for (int n = 1; n <= count; n++) {
                Mem.IsTrue(positionAdded.Filter(n));
            }
            Console.WriteLine($"EventFilter - count: {count},  duration: {sw.ElapsedMilliseconds} ms");
        }
    }
}


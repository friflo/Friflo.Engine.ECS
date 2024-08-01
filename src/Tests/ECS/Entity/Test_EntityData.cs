using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;


// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_EntityState
{
#pragma warning disable CS0618 // Type or member is obsolete
    [Test]
    public static void Test_EntityData_access()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new Position(1,2,3), new EntityName("test"), Tags.Get<TestTag, TestTag2>());
        
        var data = entity.Data;
        IsFalse(data.IsNull);
        
        var tags = data.Tags;
        IsTrue(tags.Has<TestTag>());
        IsTrue(tags.Has<TestTag2>());
        
        AreEqual(1,                     data.Id);
        AreEqual("Id: 1",               data.ToString());
        AreSame (entity.Archetype,      data.Archetype);
        AreEqual(new Position(1,2,3),   data.Get<Position>());
        IsTrue  (                       data.Has<Position>());
        IsTrue  (                       data.TryGet<Position>(out var pos));
        AreEqual(new Position(1,2,3),   pos);
        
        AreEqual("test",                data.Get<EntityName>().value);
        
        IsFalse(                        data.Has<Scale3>());
        IsFalse(                        data.TryGet<Scale3>(out _));
        
        var components = data.Components;
        AreEqual(2, components.Count);
        int count = 0;
        foreach (var component in components)
        {
            var value = component.Value;
            switch (count++) {
                case 0: AreEqual("test",                ((EntityName)value).value); break;
                case 1: AreEqual(new Position(1,2,3),   (Position)value);           break;
            }
        }
        AreEqual(2, count);
        
        entity.DeleteEntity();
        
        data = entity.Data;
        IsTrue  (           data.IsNull);
        AreEqual(1,         data.Id);
        AreEqual("Id: 1",   data.ToString());
        
        Throws<NullReferenceException>(() => {
            GetTags(entity);
        });
        Throws<NullReferenceException>(() => {
            GetComponent(entity);
        });
    }
#pragma warning restore CS0618 // Type or member is obsolete
    
    private static void GetTags(Entity entity) {
        var state = entity.Data;
        _ = state.Tags;
    }
    
    private static void GetComponent(Entity entity) {
        var data = entity.Data;
        _ = data.Get<Position>();
    }
    
    [Test]
    public static void Test_EntityData_access_Perf()
    {
        int count   = 100; // 1_000_000_000
        // Test_EntityData_access_Perf count: 1000000000, duration: 3286 ms
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new Position(1,2,3), new EntityName("test"), new Scale3(), new MyComponent1());
        
        var data = entity.Data;
        
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            data.Get<Position>();
            data.Get<EntityName>();
            data.Get<Scale3>();
            data.Get<MyComponent1>();
        }
        Console.WriteLine($"Test_EntityData_access_Perf count: {count}, duration: {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_EntityData_access_Perf_Reference()
    {
        int count   = 100; // 1_000_000_000
        // Test_EntityData_access_Perf_Reference count: 1000000000, duration: 12405 ms
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new Position(1,2,3), new EntityName("test"), new Scale3(), new MyComponent1());
        
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            entity.GetComponent<Position>();
            entity.GetComponent<EntityName>();
            entity.GetComponent<Scale3>();
            entity.GetComponent<MyComponent1>();
        }
        Console.WriteLine($"Test_EntityData_access_Perf_Reference count: {count}, duration: {sw.ElapsedMilliseconds} ms");
    }
}

}

using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS;

public static class Test_StructHeap
{
    [Test]
    public static void Test_StructHeap_increase_entity_capacity()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var arch1       = store.GetArchetype(Signature.Get<Position>());
        int count       = 2000;
        var entities    = new Entity[count];
        for (int n = 0; n < count; n++)
        {
            var entity = store.CreateEntity(arch1);
            entities[n] = entity;
            Mem.AreSame(arch1,              entity.Archetype);
            Mem.AreEqual(n + 1,             arch1.EntityCount);
            Mem.IsTrue(new Position() == entity.Position); // Position is present & default
            entity.Position.x = n;
        }
        Mem.AreEqual(2048, arch1.Capacity);
        for (int n = 0; n < count; n++) {
            Mem.AreEqual(n, entities[n].Position.x);
        }
    }
    
    [Test]
    public static void Test_StructHeap_shrink_entity_capacity() // ENTITY_STRUCT
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var arch1       = store.GetArchetype(Signature.Get<Position>());
        int count       = 2000;
        var entities    = new Entity[count];
        for (int n = 0; n < count; n++)
        {
            var entity = store.CreateEntity(arch1);
            entities[n] = entity;
            entity.Position.x = n;
        }
        // --- delete majority of entities
        const int remaining = 500;
        for (int n = remaining; n < count; n++) {
            entities[n].DeleteEntity();
            Mem.AreEqual(count + remaining - n - 1, arch1.EntityCount);
        }
        Mem.AreEqual(1024, arch1.Capacity);
        for (int n = 0; n < remaining; n++) {
            Mem.AreEqual(n, entities[n].Position.x);
        }
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntity_Perf()
    {
        int count   = 10; // 10_000_000 (UsePidAsId) ~ 1082 ms
        var store   = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        var arch1   = store.GetArchetype(Signature.Get<MyComponent1>());
        _ = store.CreateEntity(arch1); // warmup
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int n = 0; n < count; n++) {
            _ = store.CreateEntity(arch1);
        }
        Console.WriteLine($"CreateEntity() - Entity.  count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        Mem.AreEqual(count + 1, arch1.EntityCount);
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntity_Perf_100()
    {
        int count = 10; // 100_000 (UsePidAsId) ~ 739 ms
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 100; i++) {
            var store   = new EntityStore(PidType.UsePidAsId);
            store.EnsureCapacity(count);

            var arch1   = store.GetArchetype(Signature.Get<MyComponent1>());
            for (int n = 0; n < count; n++) {
                _ = store.CreateEntity(arch1);
            }
            Mem.AreEqual(count, arch1.EntityCount);
        }
        Console.WriteLine($"CreateEntity() - Entity.  count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_StructHeap_invalid_store()
    {
        var store1      = new EntityStore();
        var store2      = new EntityStore();
        var arch1       = store1.GetArchetype(Signature.Get<Position>());
        var e = Assert.Throws<ArgumentException>(() => {
            store2.CreateEntity(arch1);
        });
        Mem.AreEqual("entity is owned by a different store (Parameter 'archetype')", e!.Message);
    }
}


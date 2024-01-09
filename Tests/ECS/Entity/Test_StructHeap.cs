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
    public static void Test_StructHeap_EntityStore_EnsureCapacity()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        Mem.AreEqual(1, store.EnsureCapacity(0)); // 1 => default capacity
        store.CreateEntity();
        Mem.AreEqual(0, store.EnsureCapacity(0));
        
        Mem.AreEqual(9, store.EnsureCapacity(9));
        for (int n = 0; n < 9; n++) {
            Mem.AreEqual(9 - n, store.EnsureCapacity(0));
            store.CreateEntity();
        }
        Mem.AreEqual(0, store.EnsureCapacity(0));
    }
    
    [Test]
    public static void Test_StructHeap_Archetype_EnsureCapacity()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var arch1   = store.GetArchetype(Signature.Get<MyComponent1>());
        Mem.AreEqual(512, arch1.EnsureCapacity(0)); // 1 => default capacity
        store.CreateEntity(arch1);
        Mem.AreEqual(511, arch1.EnsureCapacity(0));
        
        Mem.AreEqual(1023, arch1.EnsureCapacity(1000));
        for (int n = 0; n < 1023; n++) {
            Mem.AreEqual(1023 - n, arch1.EnsureCapacity(0));
            store.CreateEntity(arch1);
        }
        Mem.AreEqual(0, arch1.EnsureCapacity(0));
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntity_Perf()
    {
        int count   = 10; // 10_000_000 (UsePidAsId) ~ 488 ms
        // --- warmup
        var store   = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        var arch1   = store.GetArchetype(Signature.Get<MyComponent1>());
        arch1.EnsureCapacity(count);
        _ = store.CreateEntity(arch1); // warmup
        
        // --- perf
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
        int count = 10; // 100_000 (UsePidAsId) ~ 328 ms
        // --- warmup
        var store   = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        var arch1   = store.GetArchetype(Signature.Get<MyComponent1>());
        arch1.EnsureCapacity(count);
        store.CreateEntity(arch1);
        
        // --- perf
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 100; i++) {
            store   = new EntityStore(PidType.UsePidAsId);
            store.EnsureCapacity(count);
            arch1   = store.GetArchetype(Signature.Get<MyComponent1>());
            arch1.EnsureCapacity(count);
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


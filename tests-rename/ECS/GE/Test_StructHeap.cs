using System;
using System.Diagnostics;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;

public static class Test_StructHeap
{
    [Test]
    public static void Test_StructHeap_increase_entity_capacity()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var arch1       = store.GetArchetype(Signature.Get<Position>());
        int count       = 2000;
        var entities    = new GameEntity[count];
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
    public static void Test_StructHeap_shrink_entity_capacity()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var arch1       = store.GetArchetype(Signature.Get<Position>());
        int count       = 2000;
        var entities    = new GameEntity[count];
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
        var store   = new GameEntityStore();
        var arch1   = store.GetArchetype(Signature.Get<Position>());
        _ = store.CreateEntity(arch1); // warmup
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int count = 10; // 10_000_000 ~ 5754 ms
        for (int n = 0; n < count; n++) {
            _ = store.CreateEntity(arch1);
        }
        Console.WriteLine($"CreateEntity() - GameEntity.  count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        Mem.AreEqual(count + 1, arch1.EntityCount);
    }
    
    [Test]
    public static void Test_StructHeap_invalid_store()
    {
        var store1      = new GameEntityStore();
        var store2      = new GameEntityStore();
        var arch1       = store1.GetArchetype(Signature.Get<Position>());
        var e = Assert.Throws<ArgumentException>(() => {
            store2.CreateEntity(arch1);
        });
        Mem.AreEqual("entity is owned by a different store (Parameter 'archetype')", e!.Message);
    }
}


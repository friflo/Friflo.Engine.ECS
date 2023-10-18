using System;
using System.Diagnostics;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

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
            AreSame(arch1,              entity.Archetype);
            AreEqual(n + 1,             arch1.EntityCount);
            AreEqual(new Position(),    entity.Position); // Position is present & default
            entity.Position.x = n;
        }
        for (int n = 0; n < count; n++) {
            AreEqual(n, entities[n].Position.x);
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
        AreEqual(count + 1, arch1.EntityCount);
    }
    
    [Test]
    public static void Test_StructHeap_invalid_store()
    {
        var store1      = new GameEntityStore();
        var store2      = new GameEntityStore();
        var arch1       = store1.GetArchetype(Signature.Get<Position>());
        var e = Throws<ArgumentException>(() => {
            store2.CreateEntity(arch1);
        });
        AreEqual("entity is owned by a different store (Parameter 'archetype')", e!.Message);
    }
}


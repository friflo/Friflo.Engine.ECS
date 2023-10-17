using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;

public static class Test_StructHeapRaw
{
    [Test]
    public static void Test_StructHeapRaw_increase_entity_capacity()
    {
        var store       = new RawEntityStore();
        var arch1       = store.GetArchetype(Signature.Get<Position>());
        int count       = 2; 
        var ids         = new int[count];
        for (int n = 0; n < count; n++)
        {
            var id  = store.CreateEntity(arch1);
            ids[n]  = id;
            AreSame(arch1,              store.GetEntityArchetype(id));
            AreEqual(n + 1,             arch1.EntityCount);
            AreEqual(new Position(),    store.GetEntityComponentValue<Position>(id)); // Position is present & default
            store.GetEntityComponentValue<Position>(id).x = n;  
        }
        
        for (int n = 0; n < count; n++) {
            AreEqual(n, store.GetEntityComponentValue<Position>(ids[n]).x);
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_invalid_store()
    {
        var store1      = new RawEntityStore();
        var store2      = new RawEntityStore();
        var arch1       = store1.GetArchetype(Signature.Get<Position>());
        var e = Throws<ArgumentException>(() => {
            store2.CreateEntity(arch1);
        });
        AreEqual("entity is owned by a different store (Parameter 'archetype')", e!.Message);
    }
}


// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;



public static partial class Test_Locals_Avx
{
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void EmptyBody(ref Position1 position, in Velocity1 velocity) {
        // empty by intention
    } 
        
    [Test]
    public static void Test_EmptyBody()
    {
        var store = CreateTestStore();
        EmptyBodyQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = EmptyBodyQuery(storeVectorized);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void LocalVar(ref Position1 position, in Velocity1 velocity) {
        var vel = position.value + velocity.value;
        position.value = vel;
    } 
        
    [Test]
    public static void Test_LocalVar()
    {
        var store = CreateTestStore();
        LocalVarQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = LocalVarQuery(storeVectorized);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultipleLocals(ref Position1 position, float factor, float offset) {
        var vel1 = position.value * factor;
        var vel2 = offset * factor;
        position.value = vel1 * vel2;
    } 
        
    [Test]
    public static void Test_MultipleLocals()
    {
        var store = CreateTestStore();
        MultipleLocalsQuery(store, 2, 3, false);

        var storeVectorized = CreateTestStore();
        var query = MultipleLocalsQuery(storeVectorized, 2, 3);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }

    private const int EntityCount = 1000;

    private static EntityStore CreateTestStore()
    {
        var store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position1 { value = n }, new Velocity1 { value = 2 }, new FloatComponent { value = n });
        }
        return store;
    }
}

// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
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
            store.CreateEntity(
                new Position1 { value = n },
                new Velocity1 { value = 2 },
                new FloatComponent { value = n },
                new Position2 { value = new Vector2(n,       n + 100) },
                new Velocity2 { value = new Vector2(n + 200, n + 300) });
        }
        return store;
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MixedLocals(ref Position2 position, in FloatComponent scalarComp, Vector2 vec, float scalar) {
        Vector2 vec2 = position.value * scalar;
        float scalar2 = scalarComp.value * scalar;
        position.value = vec * vec2 * scalar2;
    } 
        
    [Test]
    public static void Test_MixedLocals()
    {
        var store = CreateTestStore();
        var vec = new Vector2(1,2);
        MixedLocalsQuery(store, vec, 3, false);

        var storeVectorized = CreateTestStore();
        var query = MixedLocalsQuery(storeVectorized, vec, 3); // VEC
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var val1 = entity.GetComponent<Position2>().value;
            var val2 = entityVectorized.GetComponent<Position2>().value;
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }
}

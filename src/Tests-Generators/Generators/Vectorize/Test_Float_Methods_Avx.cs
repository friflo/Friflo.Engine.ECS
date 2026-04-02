// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;



public static partial class Test_Float_Methods_Avx
{
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_Sin(ref Position1 position, in Velocity1 velocity) {
        position.value *= MathF.Sin(velocity.value);
    } 
        
    [Test]
    public static void Test_Float_Sin()
    {
        var store = CreateTestStore();
        Float_SinQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Float_SinQuery(storeVectorized);
        
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
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_Trigonometry(ref Position1 position, in Velocity1 velocity, float value) {
        var sin     = MathF.Sin(velocity.value);
        var cos     = MathF.Cos(velocity.value);
        var tan     = MathF.Tan(velocity.value);
        var asin    = MathF.Asin(velocity.value);
        var acos    = MathF.Acos(velocity.value);
        var atan    = MathF.Atan(velocity.value);
        var atan2   = MathF.Atan2(velocity.value, value);
        position.value += sin + cos + tan + asin + acos + atan + atan2;
    } 
        
    [Test]
    public static void Test_Float_Trigonometry()
    {
        var store = CreateTestStore();
        Float_TrigonometryQuery(store, 0.1f, false);

        var storeVectorized = CreateTestStore();
        var query = Float_TrigonometryQuery(storeVectorized, 0.1f);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }

}

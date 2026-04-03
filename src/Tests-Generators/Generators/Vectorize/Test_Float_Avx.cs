// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;



public static partial class Test_Float_Avx
{
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply(ref Position1 position, in Velocity1 velocity) {
        position.value *= velocity.value;
    } 
        
    [Test]
    public static void Test_Avx_Multiply()
    {
        var store = CreateTestStore();
        MultiplyQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyQuery(storeVectorized);
        
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

    const int RepeatCount = 1; // 1_000_000;

    [Test]
    public static void Test_Avx_Multiply_perf_Query_vectorized()
    {
        var store = CreateTestStore();
        for (int i = 0; i < RepeatCount; i++) {
            MultiplyQuery(store);
        }
    }

    [Test]
    public static void Test_Avx_Multiply_perf_Query()
    {
        var store = CreateTestStore();
        for (int i = 0; i < RepeatCount; i++) {
            MultiplyQuery(store, false);
        }
    }

    [Test]
    public static void Test_Avx_Multiply_perf_ForEachEntity()
    {
        var store = CreateTestStore();
        var query = store.Query<Position1, Velocity1>();
        for (int i = 0; i < RepeatCount; i++) {
            query.ForEachEntity(static (ref Position1 position, ref Velocity1 velocity, Entity _) => {
                position.value *= velocity.value;
            });
        }
    }

    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyDeltaTime(ref Position1 position, in Velocity1 velocity, float deltaTime) {
        position.value *= velocity.value * deltaTime;
    }

    [Test]
    public static void Test_Avx_Multiply_deltaTime()
    {
        var store = CreateTestStore();
        MultiplyDeltaTimeQuery(store, 2, false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyDeltaTimeQuery(storeVectorized, 2);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }

    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyVector(ref Position1 position, in Velocity1 velocity, float factor) {
        position.value *= velocity.value * factor;
    }

    [Test]
    public static void Test_Avx_MultiplyVector()
    {
        var store = CreateTestStore();
        MultiplyVectorQuery(store, 2, false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyVectorQuery(storeVectorized, 2);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }

    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAddAssignment(ref Position1 position, ref Velocity1 velocity, float deltaTime) {
        position.value += velocity.value * deltaTime;
    }

    [Test]
    public static void Test_Avx_MultiplyAddAssignment()
    {
        var store = CreateTestStore();
        MultiplyAddAssignmentQuery(store, 0.1f, false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyAddAssignmentQuery(storeVectorized, 0.1f);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }


    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position1 position, ref Velocity1 velocity, float deltaTime) {
        position.value = velocity.value * deltaTime + position.value;
    }

    [Test]
    public static void Test_Avx_MultiplyAdd()
    {
        var store = CreateTestStore();
        MultiplyAddQuery(store, 0.1f, false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyAddQuery(storeVectorized, 0.1f);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }

    // -----------------------------------------------------------------------------------------------------

    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Float_scalar(ref Position1 position, ref FloatComponent factor) {
        position.value = position.value * factor.value;
    }

    [Test]
    public static void Test_Avx_Multiply_FLoat_scalar()
    {
        var store = CreateTestStore();
        Multiply_Float_scalarQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Float_scalarQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Float_Max(ref Position1 position, float min)
    {
        position.value = MathF.Min(position.value, min);
    }

    [Test]
    public static void Test_Multiply_Vector4_Max()
    {
        var store = CreateTestStore();
        Multiply_Float_MaxQuery(store, 100, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Float_MaxQuery(storeVectorized, 100);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Float_Clamp(ref Position1 position, float min, float max)
    {
        position.value = Math.Clamp(position.value, min, max);
    }

    [Test]
    public static void Test_Multiply_Float_Clamp()
    {
        var store = CreateTestStore();
        Multiply_Float_ClampQuery(store, 100, 200, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Float_ClampQuery(storeVectorized, 100, 200);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Float_Const(ref Position1 position)
    {
        position.value += 1f;
    }

    [Test]
    public static void Test_Multiply_Float_Const()
    {
        var store = CreateTestStore();
        Multiply_Float_ConstQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Float_ConstQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position1>(), Is.EqualTo(entityVectorized.GetComponent<Position1>()));
        }
    }
}

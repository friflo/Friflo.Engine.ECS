// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;



public static partial class Test_Vector2_Avx
{
    [Vectorize][Query]  [OmitHash]
    private static void Multiply(ref Position2 position, in Velocity2 velocity) {
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
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }

    private const int EntityCount = 1000;

    private static EntityStore CreateTestStore()
    {
        var store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position2 { value = new Vector2(n, n+1)}, new Velocity2 { value = new Vector2(1,2)}, new FloatComponent { value = n });
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
        var query = store.Query<Position2, Velocity2>();
        for (int i = 0; i < RepeatCount; i++) {
            query.ForEachEntity(static (ref Position2 position, ref Velocity2 velocity, Entity _) => {
                position.value *= velocity.value;
            });
        }
    }

    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyDeltaTime(ref Position2 position, in Velocity2 velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }

    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyVector(ref Position2 position, in Velocity2 velocity, Vector2 vector3) {
        position.value *= velocity.value * vector3;
    }

    [Test]
    public static void Test_Avx_MultiplyVector()
    {
        var store = CreateTestStore();
        MultiplyVectorQuery(store, new Vector2(1,2), false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyVectorQuery(storeVectorized, new Vector2(1,2));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }

    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAddAssinment(ref Position2 position, ref Velocity2 velocity, float deltaTime) {
        position.value += velocity.value * deltaTime;
    }

    [Test]
    public static void Test_Avx_MultiplyAddAssinment()
    {
        var store = CreateTestStore();
        MultiplyAddAssinmentQuery(store, 0.1f, false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyAddAssinmentQuery(storeVectorized, 0.1f);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }


    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position2 position, ref Velocity2 velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }

    // -----------------------------------------------------------------------------------------------------

    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector2_scalar(ref Position2 position, ref FloatComponent factor) {
        position.value = position.value * factor.value;
    }

    [Test]
    public static void Test_Avx_Multiply_Vector2_scalar()
    {
        var store = CreateTestStore();
        Multiply_Vector2_scalarQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector2_scalarQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------

    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector2_Min(ref Position2 position, Vector2 min)
    {
        position.value = Vector2.Min(position.value, min);
    }

    [Test]
    public static void Test_Multiply_Vector2_Min()
    {
        var store = CreateTestStore();
        Multiply_Vector2_MinQuery(store, new Vector2(10,20), false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector2_MinQuery(storeVectorized, new Vector2(10,20));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector2_Clamp(ref Position2 position, Vector2 min, Vector2 max)
    {
        position.value = Vector2.Clamp(position.value, min, max);
    }

    [Test]
    public static void Test_Multiply_Float_Clamp()
    {
        var store = CreateTestStore();
        Multiply_Vector2_ClampQuery(store, new Vector2(100, 100), new Vector2(200, 200), false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector2_ClampQuery(storeVectorized, new Vector2(100, 100), new Vector2(200, 200));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }
}

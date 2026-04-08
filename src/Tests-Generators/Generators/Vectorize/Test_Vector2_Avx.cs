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
            store.CreateEntity(
                new Position2 { value = new Vector2(n, n + 100)},
                new Velocity2 { value = new Vector2(n + 200, n + 300)},
                new FloatComponent  { value = n },
                new FloatComponent2 { value = 2 * n }
                );
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
    private static void MultiplyAddAssignment(ref Position2 position, ref Velocity2 velocity, float deltaTime) {
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
            var value1 = entity.GetComponent<Position2>().value;
            var value2 = entityVectorized.GetComponent<Position2>().value;
            if (!AreEqual(value1, value2, 1e-5f)) {
                Assert.Fail("not equal");
            }
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
            var value1 = entity.GetComponent<Position2>().value;
            var value2 = entityVectorized.GetComponent<Position2>().value;
            if (!AreEqual(value1, value2, 1e-5f)) {
                Assert.Fail("not equal");
            }
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
        Multiply_Vector2_scalarQuery(store, false); // VEC

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
    private static void Set_scalar(Position2 position, ref FloatComponent flt, FloatComponent2 flt2) {
        flt.value = flt2.value;
    }

    [Test]
    public static void Test_Set_scalar()
    {
        var store = CreateTestStore();
        Set_scalarQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Set_scalarQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<FloatComponent>(), Is.EqualTo(entityVectorized.GetComponent<FloatComponent>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Vector2_Sum(Position2 position, ref Vector2 sum) {
        sum += position.value;
    }

    [Test][Ignore("Support for ref parameters required. This enables aggregate operation. E.g. + to calculate Sum")]
    public static void Test_Vector2_Sum()
    {
        var store = CreateTestStore();
        var sum1 = new Vector2();
        Vector2_SumQuery(store, ref sum1, false);

        var storeVectorized = CreateTestStore();
        var sum2 = new Vector2();
        var query = Vector2_SumQuery(storeVectorized, ref sum2);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        Assert.That(sum2, Is.EqualTo(sum1));
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
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector2_Lerp(ref Position2 src, Vector2 dst, float amount)
    {
        src.value = Vector2.Lerp(src.value, dst, amount);
    }

    [Test]
    public static void Test_Multiply_Vector2_Lerp()
    {
        var store = CreateTestStore();
        Multiply_Vector2_LerpQuery(store, new Vector2(100, 100), 0.2f, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector2_LerpQuery(storeVectorized, new Vector2(100, 100), 0.2f);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var val1 = entity.GetComponent<Position2>().value;
            var val2 = entityVectorized.GetComponent<Position2>().value;
            if (!AreEqual(val1, val2, 1e-4f)) {
                Assert.Fail($"not equal - expect: {val1}    was: {val2}");
            }
            // Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector2_Const(ref Position2 position, ref Velocity2 velocity)
    {
        position.value = velocity.value * 2;
    }

    [Test]
    public static void Test_Multiply_Vector2_Const()
    {
        var store = CreateTestStore();
        Multiply_Vector2_ConstQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector2_ConstQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector2_static(ref Position2 position, ref Velocity2 velocity)
    {
        position.value = velocity.value * Vector2.Pi;
    }

    [Test]
    public static void Test_Multiply_Vector2_static()
    {
        var store = CreateTestStore();
        Multiply_Vector2_staticQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector2_staticQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position2>(), Is.EqualTo(entityVectorized.GetComponent<Position2>()));
        }
    }
    
    private static bool AreEqual(Vector2 a, Vector2 b, float epsilon)
    {
        return Math.Abs(a.X - b.X) < epsilon &&
               Math.Abs(a.Y - b.Y) < epsilon;
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Cross_Vector2(ref Position2 position, Velocity2 velocity, ref FloatComponent scalar)
    {
        scalar.value = Vector2.Cross(position.value, velocity.value);
    }

    [Test]
    public static void Test_Cross_Vector2()
    {
        var store = CreateTestStore();
        Cross_Vector2Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Cross_Vector2Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<FloatComponent>(), Is.EqualTo(entityVectorized.GetComponent<FloatComponent>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Normalize_Vector2(ref Position2 position, Velocity2 velocity)
    {
        position.value = Vector2.Normalize(velocity.value);
    }

    [Test]
    public static void Test_Normalize_Vector2()
    {
        var store = CreateTestStore();
        Normalize_Vector2Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Normalize_Vector2Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var val1 = entity.GetComponent<Position2>().value;
            var val2 = entityVectorized.GetComponent<Position2>().value;
            if (!AreEqual(val1, val2, 1e-6f)) {
                Assert.Fail($"not equal - expect: {val1}    was: {val2}");
            }
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Length_Vector2(Position2 position, ref FloatComponent2 length)
    {
        length.value = position.value.Length();
    }

    [Test]
    public static void Test_Length_Vector2()
    {
        var store = CreateTestStore();
        Length_Vector2Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Length_Vector2Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<FloatComponent>(), Is.EqualTo(entityVectorized.GetComponent<FloatComponent>()));
        }
    }

}

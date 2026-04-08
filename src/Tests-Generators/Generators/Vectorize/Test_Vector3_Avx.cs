// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
using NUnit.Framework;
using Tests.ECS;
using Tests.Examples;


// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;



public static partial class Test_Vector3_Avx
{
    [Vectorize][Query]  [OmitHash]
    private static void Multiply(ref Position position, in Velocity velocity) {
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
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    private const int EntityCount = 1000;
    
    private static EntityStore CreateTestStore()
    {
        var store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position(n,n+1,n+2), new Velocity { value = new Vector3(1,2,3)}, new FloatComponent { value = n });
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
        var query = store.Query<Position, Velocity>();
        for (int i = 0; i < RepeatCount; i++) {
            query.ForEachEntity(static (ref Position position, ref Velocity velocity, Entity _) => {
                position.value *= velocity.value;
            });
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyDeltaTime(ref Position position, in Velocity velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyVector(ref Position position, in Velocity velocity, Vector3 vector3) {
        position.value *= velocity.value * vector3;
    }
    
    [Test]
    public static void Test_Avx_MultiplyVector()
    {
        var store = CreateTestStore();
        MultiplyVectorQuery(store, new Vector3(1,2,3), false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyVectorQuery(storeVectorized, new Vector3(1,2,3));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAddAssignment(ref Position position, ref Velocity velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position position, ref Velocity velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector3_scalar(ref Position position, ref FloatComponent factor) {
        position.value = position.value * factor.value;
    }
    
    [Test]
    public static void Test_Avx_Multiply_Vector3_scalar()
    {
        var store = CreateTestStore();
        Multiply_Vector3_scalarQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector3_scalarQuery(storeVectorized);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector3_Min(ref Position position, Vector3 min)
    {
        position.value = Vector3.Min(position.value, min);
    }

    [Test]
    public static void Test_Multiply_Vector3_Min()
    {
        var store = CreateTestStore();
        Multiply_Vector3_MinQuery(store, new Vector3(10,20,30), false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector3_MinQuery(storeVectorized, new Vector3(10,20,30));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector3_Clamp(ref Position position, Vector3 min, Vector3 max)
    {
        position.value = Vector3.Clamp(position.value, min, max);
    }

    [Test]
    public static void Test_Multiply_Float_Clamp()
    {
        var store = CreateTestStore();
        Multiply_Vector3_ClampQuery(store, new Vector3(100, 100, 100), new Vector3(200, 200, 200), false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector3_ClampQuery(storeVectorized, new Vector3(100, 100, 100), new Vector3(200, 200, 200));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector3_Lerp(ref Position src, Vector3 dst, Vector3 amount)
    {
        src.value = Vector3.Lerp(src.value, dst, amount);
    }

    [Test]
    public static void Test_Multiply_Vector2_Lerp()
    {
        var store = CreateTestStore();
        Multiply_Vector3_LerpQuery(store, new Vector3(100, 100, 100), new Vector3(0.2f, 0.3f, 0.4f), false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector3_LerpQuery(storeVectorized, new Vector3(100, 100, 100), new Vector3(0.2f, 0.3f, 0.4f));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var val1 = entity.GetComponent<Position>().value;
            var val2 = entityVectorized.GetComponent<Position>().value;
            if (!AreEqual(val1, val2)) {
                Assert.Fail($"not equal - expect: {val1}    was: {val2}");
            }
            // Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector3_Const(ref Position position, ref Velocity velocity)
    {
        position.value = velocity.value * 2;
    }

    [Test]
    public static void Test_Multiply_Vector3_Const()
    {
        var store = CreateTestStore();
        Multiply_Vector3_ConstQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector3_ConstQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector3_static(ref Position position, ref Velocity velocity)
    {
        position.value = velocity.value * Vector3.Pi;
    }

    [Test]
    public static void Test_Multiply_Vector3_static()
    {
        var store = CreateTestStore();
        Multiply_Vector3_staticQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector3_staticQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    private static bool AreEqual(Vector3 a, Vector3 b, float epsilon = 1e-4f)
    {
        return Math.Abs(a.X - b.X) < epsilon &&
               Math.Abs(a.Y - b.Y) < epsilon &&
               Math.Abs(a.Z - b.Z) < epsilon;
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Truncate_Vector3(ref Position position)
    {
        position.value = Vector3.Truncate(position.value);
    }

    [Test]
    public static void Test_Truncate_Vector3()
    {
        var store = CreateTestStore();
        Truncate_Vector3Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Truncate_Vector3Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Round_Vector3(ref Position position)
    {
        position.value = Vector3.Round(position.value);
    }

    [Test]
    public static void Test_Round_Vector3()
    {
        var store = CreateTestStore();
        Round_Vector3Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Round_Vector3Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Abs_Vector3(ref Position position)
    {
        position.value = Vector3.Abs(position.value);
    }

    [Test]
    public static void Test_Abs_Vector3()
    {
        var store = CreateTestStore();
        Abs_Vector3Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Abs_Vector3Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Cross_Vector3(ref Position position,  Velocity velocity)
    {
        position.value = Vector3.Cross(position.value, velocity.value);
    }

    [Test]
    public static void Test_Cross_Vector3()
    {
        var store = CreateTestStore();
        Cross_Vector3Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Cross_Vector3Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Normalize_Vector3(ref Position position, Velocity velocity)
    {
        position.value = Vector3.Normalize(velocity.value);
    }

    [Test]
    public static void Test_Normalize_Vector3()
    {
        var store = CreateTestStore();
        Normalize_Vector3Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Normalize_Vector3Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Length_Vector3(Position position, ref FloatComponent length)
    {
        length.value = position.value.Length();
    }

    [Test]
    public static void Test_Length_Vector3()
    {
        var store = CreateTestStore();
        Length_Vector3Query(store); // TODO , false);

        var storeVectorized = CreateTestStore();
        var query = Length_Vector3Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
}

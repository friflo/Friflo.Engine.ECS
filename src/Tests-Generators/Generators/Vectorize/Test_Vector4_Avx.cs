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



public static partial class Test_Vector4_Avx
{
    [Vectorize][Query]  [OmitHash]
    private static void Multiply(ref Position4 position, in Velocity4 velocity) {
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
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    private const int EntityCount = 1000;
    
    private static EntityStore CreateTestStore()
    {
        var store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position4 { value = new Vector4(n,n+1,n+2,n+3)}, new Velocity4 { value = new Vector4(1,2,3,4)}, new FloatComponent { value = n });
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
        var query = store.Query<Position4, Velocity4>();
        for (int i = 0; i < RepeatCount; i++) {
            query.ForEachEntity(static (ref Position4 position, ref Velocity4 velocity, Entity _) => {
                position.value *= velocity.value;
            });
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyDeltaTime(ref Position4 position, in Velocity4 velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyVector(ref Position4 position, in Velocity4 velocity, Vector4 vector4) {
        position.value *= velocity.value * vector4;
    }
    
    [Test]
    public static void Test_Avx_MultiplyVector()
    {
        var store = CreateTestStore();
        MultiplyVectorQuery(store, new Vector4(1,2,3,4), false);

        var storeVectorized = CreateTestStore();
        var query = MultiplyVectorQuery(storeVectorized, new Vector4(1,2,3,4));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAddAssignment(ref Position4 position, ref Velocity4 velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position4 position, ref Velocity4 velocity, float deltaTime) {
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
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector4_scalar(ref Position4 position, ref FloatComponent factor) {
        position.value = position.value * factor.value;
    }
    
    [Test]
    public static void Test_Avx_Multiply_Vector4_scalar()
    {
        var store = CreateTestStore();
        Multiply_Vector4_scalarQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector4_scalarQuery(storeVectorized);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector4_Max(ref Position4 position, Vector4 min)
    {
        position.value = Vector4.Min(position.value, min);
    }

    [Test]
    public static void Test_Multiply_Vector4_Max()
    {
        var store = CreateTestStore();
        Multiply_Vector4_MaxQuery(store, new Vector4(10,20,30,40), false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector4_MaxQuery(storeVectorized, new Vector4(10,20,30,40));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector4_Clamp(ref Position4 position, Vector4 min, Vector4 max)
    {
        position.value = Vector4.Clamp(position.value, min, max);
    }

    [Test]
    public static void Test_Multiply_Float_Clamp()
    {
        var store = CreateTestStore();
        Multiply_Vector4_ClampQuery(store, new Vector4(100, 100, 100, 100), new Vector4(200, 200, 200, 200), false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector4_ClampQuery(storeVectorized, new Vector4(100, 100, 100, 100), new Vector4(200, 200, 200, 200));

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector4_Const(ref Position4 position, ref Velocity4 velocity)
    {
        position.value = velocity.value * 2;
    }

    [Test]
    public static void Test_Multiply_Vector4_Const()
    {
        var store = CreateTestStore();
        Multiply_Vector4_ConstQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector4_ConstQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector4_static(ref Position4 position, ref Velocity4 velocity)
    {
        position.value = velocity.value * Vector4.Pi;
    }

    [Test]
    public static void Test_Multiply_Vector4_static()
    {
        var store = CreateTestStore();
        Multiply_Vector4_staticQuery(store, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector4_staticQuery(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Cross_Vector4(ref Position4 position,  Velocity4 velocity)
    {
        position.value = Vector4.Cross(position.value, velocity.value);
    }

    [Test]
    public static void Test_Cross_Vector4()
    {
        var store = CreateTestStore();
        Cross_Vector4Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Cross_Vector4Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Normalize_Vector4(ref Position4 position, Velocity4 velocity)
    {
        position.value = Vector4.Normalize(velocity.value);
    }

    [Test]
    public static void Test_Normalize_Vector3()
    {
        var store = CreateTestStore();
        Normalize_Vector4Query(store, false);

        var storeVectorized = CreateTestStore();
        var query = Normalize_Vector4Query(storeVectorized);

        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var val1 =  entity.GetComponent<Position4>().value;
            var val2 =  entityVectorized.GetComponent<Position4>().value;
            if (!AreEqual(val1, val2, 1e-7f )) {
                Assert.Fail("not equal");
            }
        }
    }

    private static bool AreEqual(Vector4 a, Vector4 b, float epsilon)
    {
        return MathF.Abs(a.X - b.X) < epsilon &&
               MathF.Abs(a.Y - b.Y) < epsilon &&
               MathF.Abs(a.Z - b.Z) < epsilon &&
               MathF.Abs(a.W - b.W) < epsilon;
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Multiply_Vector4_Matrix4x4(ref Position4 position, in Matrix4x4 matrix) {
        position.value = Vector4.Transform(position.value, matrix);
    }
    
    [Test]
    public static void Test_Avx_Multiply_Vector4_Matrix4x4()
    {
        Matrix4x4 rot = Matrix4x4.CreateFromYawPitchRoll(
            10f * (MathF.PI / 180.0f), // Yaw
            20f * (MathF.PI / 180.0f), // Pitch
            30f * (MathF.PI / 180.0f)  // Roll
        );
        Matrix4x4 trans = Matrix4x4.CreateTranslation(new Vector3(1f, 2f, 3f));
        var matrix = Matrix4x4.Multiply(rot, trans);
        
        var store = CreateTestStore();
        Multiply_Vector4_Matrix4x4Query(store, matrix, false);

        var storeVectorized = CreateTestStore();
        var query = Multiply_Vector4_Matrix4x4Query(storeVectorized, matrix);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position4>(), Is.EqualTo(entityVectorized.GetComponent<Position4>()));
        }
    }
}

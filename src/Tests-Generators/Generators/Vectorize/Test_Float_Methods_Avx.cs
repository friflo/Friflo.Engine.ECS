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
            store.CreateEntity(new Position1 { value = 50 - n * 0.1f }, new Velocity1 { value = -50 + n * 0.1f }, new FloatComponent { value = -50 + n * 0.1f });
        }
        return store;
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_Trigonometry(ref Position1 position, in Velocity1 velocity, float value) {
        var fraction= velocity.value - MathF.Truncate(velocity.value);
        var gtOne   = value + MathF.Abs(velocity.value);
        var sin     = MathF.Sin(velocity.value);
        var cos     = MathF.Cos(velocity.value);
        var tan     = MathF.Tan(velocity.value);
        var asin    = MathF.Asin(fraction);
        var acos    = MathF.Acos(fraction);
        var atan    = MathF.Atan(velocity.value);
        var atan2   = MathF.Atan2(velocity.value, value);
        var asinh   = MathF.Asinh(velocity.value);
        var acosh   = MathF.Acosh(gtOne);
        var atanh   = MathF.Atanh(fraction);
        position.value += sin + cos + tan + asin + acos + atan + atan2 + asinh + acosh + atanh;
    } 
        
    [Test]
    public static void Test_Float_Trigonometry()
    {
        var store = CreateTestStore();
        Float_TrigonometryQuery(store, 1.1f, false);

        var storeVectorized = CreateTestStore();
        var query = Float_TrigonometryQuery(storeVectorized, 1.1f);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var expect = entity.GetComponent<Position1>();
            if (float.IsNaN(expect.value)) {
                Assert.Fail("expected is NaN");
            }
            var value  = entityVectorized.GetComponent<Position1>();
            Assert.That(value, Is.EqualTo(expect));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_Misc(ref Position1 position, in Velocity1 velocity, float value) {
        var abs     = MathF.Abs(velocity.value);
        var floor   = MathF.Floor(velocity.value);
        var ceiling = MathF.Ceiling(velocity.value);
        var log10   = MathF.Log10(abs);
        var pow     = MathF.Pow(abs, velocity.value);
        var round   = MathF.Round(velocity.value);
        var sqrt    = MathF.Sqrt(abs);
        position.value = abs + floor + ceiling + log10 + pow + round + sqrt;
    } 
        
    [Test]
    public static void Test_Float_Misc()
    {
        var store = CreateTestStore();
        Float_MiscQuery(store, 1.1f, false);

        var storeVectorized = CreateTestStore();
        var query = Float_MiscQuery(storeVectorized, 1.1f);
        
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var expect = entity.GetComponent<Position1>();
            if (float.IsNaN(expect.value)) {
                Assert.Fail("expected is NaN");
            }
            var value  = entityVectorized.GetComponent<Position1>();
            if (Math.Abs(value.value - expect.value) > 0) {
                Assert.Fail("not equal");
            }
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_Log(ref Position1 position, in Velocity1 velocity, float value) {
        var abs     = MathF.Abs(velocity.value);
        var log     = MathF.Log(abs);
        position.value = log;
    } 
        
    [Test]
    public static void Test_Float_Log()
    {
        var store = CreateTestStore();
        Float_LogQuery(store, 1.1f, false);

        var storeVectorized = CreateTestStore();
        var query = Float_LogQuery(storeVectorized, 1.1f);
        
        AssertStoresEqual(store, query, 1e-6f);
    }
    
    // -----------------------------------------------------------------------------------------------------
    private static void AssertStoresEqual(EntityStore store, ArchetypeQuery query, float epsilon)
    {
        Assert.That(query.Count, Is.EqualTo(EntityCount));
        var storeVectorized = query.Store;
        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            var expect = entity.GetComponent<Position1>();
            if (float.IsNaN(expect.value)) {
                Assert.Fail("expected is NaN");
            }
            var value  = entityVectorized.GetComponent<Position1>();
            if (!AreEqual(value.value, expect.value, epsilon)) {
                Assert.Fail("not equal");
            }
        }
    }
    
    private static bool AreEqual(float a, float b, float epsilon) {
        if (float.IsInfinity(a) && float.IsInfinity(b)) {
            return true;
        }
        return Math.Abs(a - b) < epsilon;
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_Log2(ref Position1 position, in Velocity1 velocity, float value) {
        var abs     = MathF.Abs(velocity.value);
        var log     = MathF.Log2(abs);
        position.value = log;
    } 
        
    [Test]
    public static void Test_Float_Log2()
    {
        var store = CreateTestStore();
        Float_Log2Query(store, 1.1f, false);

        var storeVectorized = CreateTestStore();
        var query = Float_Log2Query(storeVectorized, 1.1f);
        
        AssertStoresEqual(store, query, 1e-6f);
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_Exp(ref Position1 position, in Velocity1 velocity, float value) {
        var abs     = MathF.Abs(velocity.value);
        var log     = MathF.Log2(abs);
        position.value = log;
    } 
        
    [Test]
    public static void Test_Float_Exp()
    {
        var store = CreateTestStore();
        Float_ExpQuery(store, 1.1f, false);

        var storeVectorized = CreateTestStore();
        var query = Float_ExpQuery(storeVectorized, 1.1f);
        
        AssertStoresEqual(store, query, 1e-6f);
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Float_ReciprocalSqrt(ref Position1 position, in Velocity1 velocity, float value) {
        var abs = MathF.Abs(velocity.value);
        position.value = value / MathF.Sqrt(abs);
    } 
        
    [Test]
    public static void Test_Float_ReciprocalSqrt()
    {
        var store = CreateTestStore();
        Float_ReciprocalSqrtQuery(store, 2, false);

        var storeVectorized = CreateTestStore();
        var query = Float_ReciprocalSqrtQuery(storeVectorized, 2);
        
        AssertStoresEqual(store, query, 1e-3f);
    }
    

}

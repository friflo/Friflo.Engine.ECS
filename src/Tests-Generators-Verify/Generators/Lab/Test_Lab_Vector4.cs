// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Lab;


public static class Test_Lab_Vector4
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static unsafe void Test_Lab_Vector4_Interleave()
    {
        var vectors = new Vector4[]
        {
            new(11, 12, 13, 14),
            new(21, 22, 23, 24),
            new(31, 32, 33, 34),
            new(41, 42, 43, 44),
            new(51, 52, 53, 54),
            new(61, 62, 63, 64),
            new(71, 72, 73, 74),
            new(81, 82, 83, 84),
        };
        var expectX = Vector256.Create(new float [] { 11,  21,  31,  41,  51,  61,  71,  81 });
        var expectY = Vector256.Create(new float [] { 12,  22,  32,  42,  52,  62,  72,  82 });
        var expectZ = Vector256.Create(new float [] { 13,  23,  33,  43,  53,  63,  73,  83 });
        var expectW = Vector256.Create(new float [] { 14,  24,  34,  44,  54,  64,  74,  84 });

        fixed (Vector4* vectors_ptr = vectors)
        {
            var ptr = (float*)vectors_ptr;
            // 1. Load 24 floats into three 256-bit registers
            Vector256<float> v0 = Avx.LoadVector256(ptr);
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
            Vector256<float> v2 = Avx.LoadVector256(ptr + 16);
            Vector256<float> v3 = Avx.LoadVector256(ptr + 24);

            var (x, y, z, w) = AvxVector4.Deinterleave(v0, v1, v2, v3);
            
            Assert.That(x, Is.EqualTo(expectX));
            Assert.That(y, Is.EqualTo(expectY));
            Assert.That(z, Is.EqualTo(expectZ));
            Assert.That(w, Is.EqualTo(expectW));
            
            var (res0, res1, res2, res3) = AvxVector4.Interleave(x, y, z, w);
            
            Assert.That(res0, Is.EqualTo(v0));
            Assert.That(res1, Is.EqualTo(v1));
            Assert.That(res2, Is.EqualTo(v2));
            Assert.That(res3, Is.EqualTo(v3));
        }
    }
    
    [Test]
    public static unsafe void Test_Lab_Vector4_Normalize()
    {
        var vectors    = new Vector4[8];
        var normalized = new Vector4[8];
        for (int n = 0; n < 8; n++) {
            vectors[n] = new Vector4(n, n + 100, n+ 200, n * 400);
        };
        fixed (Vector4* vectors_ptr    = vectors)
        fixed (Vector4* normalized_ptr = normalized)
        {
            var ptr      = (float*)vectors_ptr;
            var norm_ptr = (float*)normalized_ptr;
            
            Vector256<float> v0 = Avx.LoadVector256(ptr);
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
            Vector256<float> v2 = Avx.LoadVector256(ptr + 16);
            Vector256<float> v3 = Avx.LoadVector256(ptr + 24);
            (v0, v1, v2, v3) = AvxVector4.Deinterleave(v0, v1, v2, v3);

            var (n0, n1, n2, n3) = AvxVector4.Normalize(v0,v1,v2, v3);
            v0 = n0;
            v1 = n1;
            v2 = n2;
            v3 = n3;
            
            (v0, v1, v2, v3) = AvxVector4.Interleave(v0, v1, v2, v3);
            Avx.Store(norm_ptr,       v0);
            Avx.Store(norm_ptr +  8,  v1);
            Avx.Store(norm_ptr + 16,  v2);
            Avx.Store(norm_ptr + 24,  v3);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  Vector4.Normalize(vectors[n]);
            if (!AreEqual(expect, normalized[n], 1e-6f)) {
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
    
    [Test]
    public static unsafe void Test_Lab_Vector4_Length()
    {
        var vectors    = new Vector4[8];
        var lengths    = new float[8];
        for (int n = 0; n < 8; n++) {
            vectors[n] = new Vector4(n, n + 100, n+ 200, n + 400);
        };
        fixed (Vector4* vectors_ptr    = vectors)
        fixed (float*   lengths_ptr = lengths)
        {
            var ptr      = (float*)vectors_ptr;
            // 1. Load 24 floats into three 256-bit registers
            Vector256<float> v0 = Avx.LoadVector256(ptr);
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
            Vector256<float> v2 = Avx.LoadVector256(ptr + 16);
            Vector256<float> v3 = Avx.LoadVector256(ptr + 24);
            (v0, v1, v2, v3) = AvxVector4.Deinterleave(v0, v1, v2, v3);

            var length = AvxVector4.Length(v0,v1,v2,v3);
            
            Avx.Store(lengths_ptr,       length);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  vectors[n].Length();
            Assert.That(expect, Is.EqualTo(lengths[n]));
        }
    }
    
    [Test]
    public static unsafe void Test_Lab_Vector4_Distance()
    {
        var vector1  = new Vector4[8];
        var vector2  = new Vector4[8];
        var distance = new float[8];
        
        for (int n = 0; n < 8; n++) {
            vector1[n] = new Vector4(    n,     n + 100,     n + 200,     n + 300);
            vector2[n] = new Vector4(2 * n, 2 * n + 100, 2 * n + 200, 2 * n + 300);
        };
        fixed (Vector4* vector1_first = vector1)
        fixed (Vector4* vector2_first = vector2)
        fixed (float*   distance_first = distance)
        {
            var vector1_ptr      = (float*)vector1_first;
            var vector2_ptr = (float*)vector2_first;
            
            Vector256<float> v1_0 = Avx.LoadVector256(vector1_ptr);
            Vector256<float> v1_1 = Avx.LoadVector256(vector1_ptr + 8);
            Vector256<float> v1_2 = Avx.LoadVector256(vector1_ptr + 16);
            Vector256<float> v1_3 = Avx.LoadVector256(vector1_ptr + 24);
            (v1_0, v1_1, v1_2, v1_3) = AvxVector4.Deinterleave(v1_0, v1_1, v1_2,  v1_3);
            
            Vector256<float> v2_0 = Avx.LoadVector256(vector2_ptr);
            Vector256<float> v2_1 = Avx.LoadVector256(vector2_ptr + 8);
            Vector256<float> v2_2 = Avx.LoadVector256(vector2_ptr + 16);
            Vector256<float> v2_3 = Avx.LoadVector256(vector2_ptr + 24);
            (v2_0, v2_1, v2_2, v2_3) = AvxVector4.Deinterleave(v2_0, v2_1, v2_2,  v2_3);

            var dist = AvxVector4.Distance(v1_0,v1_1,v1_2,v1_3,  v2_0,v2_1,v2_2, v2_3);
            
            Avx.Store(distance_first,       dist);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  Vector4.Distance(vector1[n], vector2[n]);
            Assert.That(distance[n], Is.EqualTo(expect));
        }
    }
    
    [Test]
    public static unsafe void Test_Lab_Vector4_DistanceSquared()
    {
        var vector1  = new Vector4[8];
        var vector2  = new Vector4[8];
        var distance = new float[8];
        
        for (int n = 0; n < 8; n++) {
            vector1[n] = new Vector4(    n,     n + 100,     n + 200,     n + 300);
            vector2[n] = new Vector4(2 * n, 2 * n + 100, 2 * n + 200, 2 * n + 300);
        };
        fixed (Vector4* vector1_first = vector1)
        fixed (Vector4* vector2_first = vector2)
        fixed (float*   distance_first = distance)
        {
            var vector1_ptr      = (float*)vector1_first;
            var vector2_ptr = (float*)vector2_first;
            
            Vector256<float> v1_0 = Avx.LoadVector256(vector1_ptr);
            Vector256<float> v1_1 = Avx.LoadVector256(vector1_ptr + 8);
            Vector256<float> v1_2 = Avx.LoadVector256(vector1_ptr + 16);
            Vector256<float> v1_3 = Avx.LoadVector256(vector1_ptr + 24);
            (v1_0, v1_1, v1_2, v1_3) = AvxVector4.Deinterleave(v1_0, v1_1, v1_2,  v1_3);
            
            Vector256<float> v2_0 = Avx.LoadVector256(vector2_ptr);
            Vector256<float> v2_1 = Avx.LoadVector256(vector2_ptr + 8);
            Vector256<float> v2_2 = Avx.LoadVector256(vector2_ptr + 16);
            Vector256<float> v2_3 = Avx.LoadVector256(vector2_ptr + 24);
            (v2_0, v2_1, v2_2, v2_3) = AvxVector4.Deinterleave(v2_0, v2_1, v2_2,  v2_3);

            var dist = AvxVector4.DistanceSquared(v1_0,v1_1,v1_2,v1_3,  v2_0,v2_1,v2_2, v2_3);
            
            Avx.Store(distance_first,       dist);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  Vector4.DistanceSquared(vector1[n], vector2[n]);
            Assert.That(distance[n], Is.EqualTo(expect));
        }
    }

}




// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Lab;


public static class Test_Lab_Vector2
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static unsafe void Test_Lab_Vector2_Interleave()
    {
        var vectors = new Vector2[]
        {
            new(11, 12),
            new(21, 22),
            new(31, 32),
            new(41, 42),
            new(51, 52),
            new(61, 62),
            new(71, 72),
            new(81, 82),
            
            new( 91, 92),
            new(101,102),
            new(111,112),
            new(121,122),
            new(131,132),
            new(141,142),
            new(151,152),
            new(161,162),
        };
        var expectX1 = Vector256.Create(new float [] { 11,  21,  31,  41,  51,  61,  71,  81 });
        var expectX2 = Vector256.Create(new float [] { 91, 101, 111, 121, 131, 141, 151, 161 });
        var expectY1 = Vector256.Create(new float [] { 12,  22,  32,  42,  52,  62,  72,  82 });
        var expectY2 = Vector256.Create(new float [] { 92, 102, 112, 122, 132, 142, 152, 162 });

        fixed (Vector2* vectors_ptr = vectors)
        {
            var ptr = (float*)vectors_ptr;
            // 1. Load 24 floats into three 256-bit registers
            Vector256<float> v0 = Avx.LoadVector256(ptr);      // [X0 Y0 X1 Y1 X2 Y2 X3 Y3]
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);  // [X4 Y4 X5 Y5 X6 Y6 X7 Y7]
            Vector256<float> v2 = Avx.LoadVector256(ptr + 16);
            Vector256<float> v3 = Avx.LoadVector256(ptr + 24);

            var (x1, y1) = AvxVector2.Deinterleave(v0, v1);
            var (x2, y2) = AvxVector2.Deinterleave(v2, v3);
            
            Assert.That(x1, Is.EqualTo(expectX1));
            Assert.That(y1, Is.EqualTo(expectY1));
            Assert.That(x2, Is.EqualTo(expectX2));
            Assert.That(y2, Is.EqualTo(expectY2));
            
            var (res0, res1) = AvxVector2.Interleave(x1,y1);
            Assert.That(res0, Is.EqualTo(v0));
            Assert.That(res1, Is.EqualTo(v1));
            
            var (res2, res3) = AvxVector2.Interleave(x2,y2);
            Assert.That(res2, Is.EqualTo(v2));
            Assert.That(res3, Is.EqualTo(v3));
        }
    }
    
    [Test]
    public static unsafe void Test_Lab_Vector3_Normalize()
    {
        var vectors    = new Vector2[8];
        var normalized = new Vector2[8];
        for (int n = 0; n < 8; n++) {
            vectors[n] = new Vector2(n, n + 100);
        };
        fixed (Vector2* vectors_ptr    = vectors)
        fixed (Vector2* normalized_ptr = normalized)
        {
            var ptr      = (float*)vectors_ptr;
            var norm_ptr = (float*)normalized_ptr;

            Vector256<float> v0 = Avx.LoadVector256(ptr);
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
            (v0, v1) = AvxVector2.Deinterleave(v0, v1);

            var (n0, n1) = AvxVector2.Normalize(v0,v1);
            v0 = n0;
            v1 = n1;
            
            (v0, v1) = AvxVector2.Interleave(v0, v1);
            Avx.Store(norm_ptr,       v0);
            Avx.Store(norm_ptr +  8,  v1);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  Vector2.Normalize(vectors[n]);
            if (!AreEqual(expect, normalized[n], 1e-7f)) {
                Assert.Fail("not equal");
            }
        }
    }
    
    private static bool AreEqual(Vector2 a, Vector2 b, float epsilon)
    {
        return MathF.Abs(a.X - b.X) < epsilon &&
               MathF.Abs(a.Y - b.Y) < epsilon;
    }
    
    [Test]
    public static unsafe void Test_Lab_Vector2_Length()
    {
        var vectors    = new Vector2[8];
        var lengths    = new float[8];
        for (int n = 0; n < 8; n++) {
            vectors[n] = new Vector2(n, n + 100);
        };
        fixed (Vector2* vectors_ptr    = vectors)
        fixed (float*   lengths_ptr = lengths)
        {
            var ptr      = (float*)vectors_ptr;
            
            Vector256<float> v0 = Avx.LoadVector256(ptr);
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
            (v0, v1) = AvxVector2.Deinterleave(v0, v1);

            var length = AvxVector2.Length(v0,v1);
            
            Avx.Store(lengths_ptr,       length);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  vectors[n].Length();
            Assert.That(expect, Is.EqualTo(lengths[n]));
        }
    }
}




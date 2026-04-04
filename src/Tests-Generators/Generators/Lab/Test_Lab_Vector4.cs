// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

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
}




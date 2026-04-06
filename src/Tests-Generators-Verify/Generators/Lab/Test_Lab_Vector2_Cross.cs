// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Lab;


public static class Test_Lab_Vector2_Cross
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static void Test_Lab_Vector2_Cross_call()
    {
        var vec1 = new Position2[64];
        var vec2 = new Position2[64];
        var result1 = new float[64];
        var result2 = new float[64];
        for (int n = 0; n < vec1.Length; n++) {
            vec1[n].value = new Vector2(n,       n + 100 );
            vec2[n].value = new Vector2(n + 200, n + 300 );
            result1[n] = Vector2.Cross(vec1[n].value, vec2[n].value);
        }
        Test_Cross_Avx(vec1, vec2, result2);

        Assert.That(result1, Is.EqualTo(result2));
    }
    
    private static unsafe int Test_Cross_Avx(Span<Position2> vec1, Span<Position2> vec2, Span<float> result)
    {
        int i = 0;
        var end = vec1.Length - 16;
        if (i > end) {
            return 0;
        }
        // --- Locals

        fixed (Position2* vec1_first = vec1)
        fixed (Position2* vec2_first = vec2)
        fixed (float* result_first = result)
        {
            for (; i <= end; i += 16)
            {
                float* vec1_ptr = (float*)(vec1_first + i);
                float* vec2_ptr = (float*)(vec2_first + i);
                float* result_ptr = (float*)(result_first + i);

                // --- 1. Load
                Vector256<float> vec1_0 = Avx.LoadVector256(vec1_ptr + 0);  // 4 Position2
                Vector256<float> vec1_1 = Avx.LoadVector256(vec1_ptr + 8);  // 4 Position2
                Vector256<float> vec1_2 = Avx.LoadVector256(vec1_ptr + 16); // 4 Position2
                Vector256<float> vec1_3 = Avx.LoadVector256(vec1_ptr + 24); // 4 Position2
                (vec1_0, vec1_1) = AvxVector2.Deinterleave(vec1_0, vec1_1);
                (vec1_2, vec1_3) = AvxVector2.Deinterleave(vec1_2, vec1_3);
                
                Vector256<float> vec2_0 = Avx.LoadVector256(vec2_ptr + 0);  // 4 Position2
                Vector256<float> vec2_1 = Avx.LoadVector256(vec2_ptr + 8);  // 4 Position2
                Vector256<float> vec2_2 = Avx.LoadVector256(vec2_ptr + 16); // 4 Position2
                Vector256<float> vec2_3 = Avx.LoadVector256(vec2_ptr + 24); // 4 Position2
                (vec2_0, vec2_1) = AvxVector2.Deinterleave(vec2_0, vec2_1);
                (vec2_2, vec2_3) = AvxVector2.Deinterleave(vec2_2, vec2_3);
                
                Vector256<float> result_0 = Avx.LoadVector256(result_ptr);
                Vector256<float> result_1 = Avx.LoadVector256(result_ptr + 8);

                // --- 2. Compute
                result_0 = Fma.MultiplySubtract(vec1_0, vec2_1, Avx.Multiply(vec1_1, vec2_0));
                result_1 = Fma.MultiplySubtract(vec1_2, vec2_3, Avx.Multiply(vec1_3, vec2_2));

                // --- 3. Store
                (result_0, result_1) = AvxVector2.Deinterleave(result_0, result_1);
                Avx.Store(result_ptr + 0, result_0);
                Avx.Store(result_ptr + 8, result_1);
            }
        }
        return i;
    }
}

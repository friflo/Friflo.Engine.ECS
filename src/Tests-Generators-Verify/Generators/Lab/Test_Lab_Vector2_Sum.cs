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


public static class Test_Lab_Vector2_Sum
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static void Test_Lab_Vector2_Sum_call()
    {
        var positions = new Position2[64];
        var sum1 = new Vector2(1,2);
        for (int n = 0; n < positions.Length; n++) {
            positions[n].value = new Vector2(1, n + 100 );
            sum1 += positions[n].value;
        }
        var sum2 = new Vector2(1,2);
        Test_Sum_Avx(positions, ref sum2);
        Assert.That(sum1 == sum2);
    }
    
    [Test]
    public static void Test_Lab_Vector2_Sum_perf()
    {
        var positions = new Position2[1024];
        var sum = new Vector2(1,2);
        for (int n = 0; n < 20; n++) { // 20_000_000
            Test_Sum_Avx(positions, ref sum);
            // for (int i = 0; i < 1024; i++) { sum += positions[i].value; }
        }
    }
    
    [SkipLocalsInit]
    private static unsafe int Test_Sum_Avx(Span<Position2> position, ref Vector2 sum)
    {
        int i = 0;
        var end = position.Length - 16;
        if (i > end) {
            return 0;
        }
        // --- Locals
        Vector256<float> sum_scalar = default;
        // Vector128<float> sum_half = Vector128.Create(sum.X, sum.Y, sum.X, sum.Y);
        // var sum_scalar = Avx.InsertVector128(sum_half.ToVector256(), sum_half, 1);

        fixed (Position2* position_first = position)
        {
            for (; i <= end; i += 16)
            {
                float* position_ptr = (float*)(position_first + i);

                // --- 1. Load
                Vector256<float> position_0 = Avx.LoadVector256(position_ptr + 0);  // 4 Position2
                Vector256<float> position_1 = Avx.LoadVector256(position_ptr + 8);  // 4 Position2
                Vector256<float> position_2 = Avx.LoadVector256(position_ptr + 16); // 4 Position2
                Vector256<float> position_3 = Avx.LoadVector256(position_ptr + 24); // 4 Position2
                // (position_0, position_1) = AvxVector2.Deinterleave(position_0, position_1);
                // (position_2, position_3) = AvxVector2.Deinterleave(position_2, position_3);

                // --- 2. Compute
                sum_scalar = Avx.Add(sum_scalar, position_0);
                sum_scalar = Avx.Add(sum_scalar, position_1);
                sum_scalar = Avx.Add(sum_scalar, position_2);
                sum_scalar = Avx.Add(sum_scalar, position_3);

                // --- 3. Store
            }
            sum.X += sum_scalar[0] + sum_scalar[2] + sum_scalar[4] + sum_scalar[6];
            sum.Y += sum_scalar[1] + sum_scalar[3] + sum_scalar[5] + sum_scalar[7];
        }
        return i;
    }

}




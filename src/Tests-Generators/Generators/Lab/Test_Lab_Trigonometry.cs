// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Lab {

public static class Test_Lab_Trigonometry
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static void Test_Lab_Trigonometry_Sin()
    {
        var input = Vector256.Create(0, 0.5f, 1, 1.5f, 2, 2.5f, 3, 3.5f);
        var result_sin = Sin(input);
        var result_minimax = Sin11(input);

        for (int n = 0; n < 8; n++) {
            if (!AreEqual(result_minimax[n], result_sin[n])) {
                Assert.Fail("not equal");
            }
        }
    }

    // [Test]
    public static void Test_Lab_Trigonometry_Sin_perf()
    {
        var input = Vector256.Create(0, 0.5f, 1, 1.5f, 2, 2.5f, 3, 3.5f);
        for (long n = 0; n < 10_000_000_000; n++) {
            Sin(input);
        }
    }

    private static bool AreEqual(float a, float b, float epsilon = 1e-3f)
    {
        return Math.Abs(a - b) < epsilon;
    }

    internal static Vector256<float> Sin(Vector256<float> x)
    {
        return Vector256.Create(
            MathF.Sin(x[0]),
            MathF.Sin(x[1]),
            MathF.Sin(x[2]),
            MathF.Sin(x[3]),
            MathF.Sin(x[4]),
            MathF.Sin(x[5]),
            MathF.Sin(x[6]),
            MathF.Sin(x[7]));
    }
    
    internal static Vector256<float> FastSin(Vector256<float> x)
    {
        // Pre-calculated constants for the polynomial
        var c1 = Vector256.Create(-0.16666667f); // -1/6
        var c2 = Vector256.Create(0.008333333f);  // 1/120
        var one = Vector256.Create(1.0f);

        // 1. Calculate x^2
        var x2 = Avx.Multiply(x, x);

        // 2. Start Horner's Method: (c2 * x^2 + c1)
        // Result = (x2 * c2) + c1
        var res = Fma.MultiplyAdd(x2, c2, c1);

        // 3. Continue: res * x^2 + 1
        // Result = (res * x2) + 1
        res = Fma.MultiplyAdd(res, x2, one);

        // 4. Final step: Multiply by x
        // Result = res * x
        return Avx.Multiply(res, x);
    }
    
   
    // ---
    // Constants for Range Reduction
    private static readonly Vector256<float> _invTwoPi = Vector256.Create(0.159154943f);
    private static readonly Vector256<float> _negTwoPi = Vector256.Create(-6.28318531f);

    // 11th-Degree Minimax Coefficients for [-PI, PI]
    private static readonly Vector256<float> _c5 = Vector256.Create(-0.00000002505210838f); // x^11
    private static readonly Vector256<float> _c4 = Vector256.Create(0.00000275573192f);      // x^9
    private static readonly Vector256<float> _c3 = Vector256.Create(-0.000198412698f);      // x^7
    private static readonly Vector256<float> _c2 = Vector256.Create(0.00833333333f);       // x^5
    private static readonly Vector256<float> _c1 = Vector256.Create(-0.166666666f);        // x^3
    private static readonly Vector256<float> _one = Vector256.Create(1.0f);                // x^1

    public static Vector256<float> Sin11(Vector256<float> x)
    {
        // --- 1. RANGE REDUCTION (to [-PI, PI]) ---
        // quotient = x / 2PI
        var quotient = Avx.Multiply(x, _invTwoPi);
        // rounded = round(quotient)
        var rounded = Avx.RoundToNearestInteger(quotient);
        // xReduced = x - (rounded * 2PI)
        var xReduced = Fma.MultiplyAdd(rounded, _negTwoPi, x);

        // --- 2. POLYNOMIAL EVALUATION (Horner's Method) ---
        var x2 = Avx.Multiply(xReduced, xReduced);

        // (((((x2 * c5) + c4) * x2 + c3) * x2 + c2) * x2 + c1) * x2 + 1
        var res = Fma.MultiplyAdd(x2, _c5, _c4);
        res = Fma.MultiplyAdd(res, x2, _c3);
        res = Fma.MultiplyAdd(res, x2, _c2);
        res = Fma.MultiplyAdd(res, x2, _c1);
        res = Fma.MultiplyAdd(res, x2, _one);

        // Final result = res * xReduced
        return Avx.Multiply(res, xReduced);
    }
}

}


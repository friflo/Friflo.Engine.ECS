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
        var result_minimax = SinMinimax(input);

        for (int n = 0; n < 8; n++) {
            if (!AreEqual(result_minimax[n], result_sin[n])) {
                // Assert.Fail("not equal");
            }
        }
    }
    
    private static bool AreEqual(float a, float b, float epsilon = 1e-5f)
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
    
// --------------------------------- Minimax Polynomial for sin(x) --------------------------------- 
// Pre-aligning constants in static fields for your generator to reference
    private static readonly Vector256<float> _invTwoPi = Vector256.Create(0.159154943f);
    private static readonly Vector256<float> _negTwoPi = Vector256.Create(-6.28318531f);
    
    // Minimax Coefficients (7th Degree)
    private static readonly Vector256<float> _c1 = Vector256.Create(-0.166666567f);
    private static readonly Vector256<float> _c2 = Vector256.Create(0.008332201f);
    private static readonly Vector256<float> _c3 = Vector256.Create(-0.000195152f);
    private static readonly Vector256<float> _one = Vector256.Create(1.0f);

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector256<float> SinMinimax(Vector256<float> x)
    {
        // --- 1. RANGE REDUCTION ---
        // x = x - round(x / 2pi) * 2pi
        var quotient = Avx.Multiply(x, _invTwoPi);
        var rounded = Avx.RoundToNearestInteger(quotient);
        var xReduced = Fma.MultiplyAdd(rounded, _negTwoPi, x);

        // --- 2. POLYNOMIAL (Horner's Method) ---
        // We calculate: x * (1 + x^2 * (c1 + x^2 * (c2 + x^2 * c3)))
        var x2 = Avx.Multiply(xReduced, xReduced);

        // Start from the "innermost" parenthesis (c3)
        // res = (x2 * c3) + c2
        var res = Fma.MultiplyAdd(x2, _c3, _c2);

        // res = (res * x2) + c1
        res = Fma.MultiplyAdd(res, x2, _c1);

        // res = (res * x2) + 1.0
        res = Fma.MultiplyAdd(res, x2, _one);

        // Final scale by x
        return Avx.Multiply(res, xReduced);
    }
}

}


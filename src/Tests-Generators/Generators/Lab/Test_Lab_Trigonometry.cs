// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static System.MathF;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Lab {

public static class Test_Lab_Trigonometry
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static void Test_Lab_Trigonometry_Sin()
    {
        var input = Vector256.Create(0, 0.5f, 1, 1.5f, 2, 2.5f, 3, 3.5f);
        var result_sin = SinMathF(input);
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
            SinMathF(input);
        }
    }

    private static bool AreEqual(float a, float b, float epsilon = 1e-3f)
    {
        return Math.Abs(a - b) < epsilon;
    }

    [SkipLocalsInit]
    internal static Vector256<float> SinMathF(Vector256<float> x)
    {
        return Vector256.Create(Sin(x[0]), Sin(x[1]), Sin(x[2]), Sin(x[3]), Sin(x[4]), Sin(x[5]), Sin(x[6]), Sin(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> CosMathF(Vector256<float> x)
    {
        return Vector256.Create(Cos(x[0]), Cos(x[1]), Cos(x[2]), Cos(x[3]), Cos(x[4]), Cos(x[5]), Cos(x[6]), Cos(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> TanMathF(Vector256<float> x)
    {
        return Vector256.Create(Tan(x[0]), Tan(x[1]), Tan(x[2]), Tan(x[3]), Tan(x[4]), Tan(x[5]), Tan(x[6]), Tan(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> AsinMathF(Vector256<float> x)
    {
        return Vector256.Create(Asin(x[0]), Asin(x[1]), Asin(x[2]), Asin(x[3]), Asin(x[4]), Asin(x[5]), Asin(x[6]), Asin(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> AcosMathF(Vector256<float> x)
    {
        return Vector256.Create(Acos(x[0]), Acos(x[1]), Acos(x[2]), Acos(x[3]), Acos(x[4]), Acos(x[5]), Acos(x[6]), Acos(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> AtanMathF(Vector256<float> x)
    {
        return Vector256.Create(Atan(x[0]), Atan(x[1]), Atan(x[2]), Atan(x[3]), Atan(x[4]), Atan(x[5]), Atan(x[6]), Atan(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> Atan2MathF(Vector256<float> y, Vector256<float> x)
    {
        return Vector256.Create(Atan2(y[0],x[0]), Atan2(y[1],x[1]), Atan2(y[2],x[2]), Atan2(y[3],x[3]), Atan2(y[4],x[4]), Atan2(y[5],x[5]), Atan2(y[6],x[6]), Atan2(y[7],x[7]));
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


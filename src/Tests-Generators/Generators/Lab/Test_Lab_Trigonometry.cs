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

    // ------ Trigonometry
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
    
    [SkipLocalsInit]
    internal static Vector256<float> AsinhMathF(Vector256<float> x)
    {
        return Vector256.Create(Asinh(x[0]), Asinh(x[1]), Asinh(x[2]), Asinh(x[3]), Asinh(x[4]), Asinh(x[5]), Asinh(x[6]), Asinh(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> AcoshMathF(Vector256<float> x)
    {
        return Vector256.Create(Acosh(x[0]), Acosh(x[1]), Acosh(x[2]), Acosh(x[3]), Acosh(x[4]), Acosh(x[5]), Acosh(x[6]), Acosh(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> AtanhMathF(Vector256<float> x)
    {
        return Vector256.Create(Atanh(x[0]), Atanh(x[1]), Atanh(x[2]), Atanh(x[3]), Atanh(x[4]), Atanh(x[5]), Atanh(x[6]), Atanh(x[7]));
    }
    
    // ------ misc
    [SkipLocalsInit]
    internal static Vector256<float> TruncateMathF(Vector256<float> x)
    {
        return Vector256.Create(Truncate(x[0]), Truncate(x[1]), Truncate(x[2]), Truncate(x[3]), Truncate(x[4]), Truncate(x[5]), Truncate(x[6]), Truncate(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> FloorMathF(Vector256<float> x)
    {
        return Vector256.Create(Floor(x[0]), Floor(x[1]), Floor(x[2]), Floor(x[3]), Floor(x[4]), Floor(x[5]), Floor(x[6]), Floor(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> ExpMathF(Vector256<float> x)
    {
        return Vector256.Create(Exp(x[0]), Exp(x[1]), Exp(x[2]), Exp(x[3]), Exp(x[4]), Exp(x[5]), Exp(x[6]), Exp(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> LogMathF(Vector256<float> x)
    {
        return Vector256.Create(Log(x[0]), Log(x[1]), Log(x[2]), Log(x[3]), Log(x[4]), Log(x[5]), Log(x[6]), Log(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> Log10MathF(Vector256<float> x)
    {
        return Vector256.Create(Log10(x[0]), Log10(x[1]), Log10(x[2]), Log10(x[3]), Log10(x[4]), Log10(x[5]), Log10(x[6]), Log10(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> Log2MathF(Vector256<float> x)
    {
        return Vector256.Create(Log2(x[0]), Log2(x[1]), Log2(x[2]), Log2(x[3]), Log2(x[4]), Log2(x[5]), Log2(x[6]), Log2(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> PowMathF(Vector256<float> x, Vector256<float> y)
    {
        return Vector256.Create(Pow(x[0],y[0]), Atan2(x[1],y[1]), Atan2(x[2],y[2]), Atan2(x[3],y[3]), Atan2(x[4],y[4]), Atan2(x[5],y[5]), Atan2(x[6],y[6]), Atan2(x[7],y[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> RoundMathF(Vector256<float> x)
    {
        return Vector256.Create(Round(x[0]), Round(x[1]), Round(x[2]), Round(x[3]), Round(x[4]), Round(x[5]), Round(x[6]), Round(x[7]));
    }
    
    [SkipLocalsInit]
    internal static Vector256<float> SqrtMathF(Vector256<float> x)
    {
        return Vector256.Create(Sqrt(x[0]), Sqrt(x[1]), Sqrt(x[2]), Sqrt(x[3]), Sqrt(x[4]), Sqrt(x[5]), Sqrt(x[6]), Sqrt(x[7]));
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


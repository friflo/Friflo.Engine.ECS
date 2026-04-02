// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
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
        var result_naive = Sin(input);
        var result_simd = FastSin(input);
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
}

}


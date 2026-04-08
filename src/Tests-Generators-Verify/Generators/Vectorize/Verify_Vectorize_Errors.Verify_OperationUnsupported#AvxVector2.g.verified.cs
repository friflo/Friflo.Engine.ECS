//HintName: Friflo.Vectorization.Intrinsics/AvxVector2.g.cs

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
// ReSharper disable InconsistentNaming

namespace Friflo.Vectorization.Intrinsics;

public static class AvxVector2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static (Vector256<float> X, Vector256<float> Y) Deinterleave(
        Vector256<float> lower, 
        Vector256<float> upper)
    {
        // STEP 1: Shuffle to separate X and Y within 128-bit lanes.
        // Index 0xA8 is 0b10_10_10_10 (Selects Y components: indices 1, 1, 3, 3)
        // Index 0x50 is 0b01_01_00_00 (Selects X components: indices 0, 0, 2, 2)
        // However, Avx.Shuffle(a, b, mask) is better here:
        
        // x_mixed: [X0, X1, X2, X3 | X4, X5, X6, X7] (but lanes are swapped)
        // y_mixed: [Y0, Y1, Y2, Y3 | Y4, Y5, Y6, Y7] 
        
        // A cleaner way is using Shuffle on the same register:
        var x_parts = Avx.Shuffle(lower, upper, 0b10_00_10_00); 
        // Result: [X0, X1, X4, X5 | X2, X3, X6, X7]
        
        var y_parts = Avx.Shuffle(lower, upper, 0b11_01_11_01);
        // Result: [Y0, Y1, Y4, Y5 | Y2, Y3, Y6, Y7]

        // STEP 2: Fix the Lane Problem
        // Because AVX shuffles work within 128-bit lanes, the indices are jumbled.
        // We use Permute2x128 to put 0,1,2,3 in the low half and 4,5,6,7 in the high.
        
        // Final X: [X0, X1, X2, X3, X4, X5, X6, X7]
        var x = Avx.Permute2x128(x_parts, x_parts, 0x02); 
        x = Avx2.PermuteVar8x32(x_parts, Vector256.Create(0, 1, 4, 5, 2, 3, 6, 7).AsInt32());

        // Final Y: [Y0, Y1, Y2, Y3, Y4, Y5, Y6, Y7]
        var y = Avx2.PermuteVar8x32(y_parts, Vector256.Create(0, 1, 4, 5, 2, 3, 6, 7).AsInt32());

        return (x, y);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static (Vector256<float> V0, Vector256<float> V1) Interleave(Vector256<float> x, Vector256<float> y)
    {
        // 1. Interleave 32-bit floats
        // xyLo: [X0, Y0, X1, Y1 | X4, Y4, X5, Y5]
        // xyHi: [X2, Y2, X3, Y3 | X6, Y6, X7, Y7]
        var xyLo = Avx.UnpackLow(x, y);
        var xyHi = Avx.UnpackHigh(x, y);

        // 2. Fix the 128-bit Lane Gap
        // We want V0 to be [X0, Y0, X1, Y1, X2, Y2, X3, Y3]
        // We want V1 to be [X4, Y4, X5, Y5, X6, Y6, X7, Y7]
        
        // Permute2x128 takes the low 128-bits of xyLo and the low 128-bits of xyHi
        // 0x20 = 0010 0000 (Low of Source 2, Low of Source 1)
        var v0 = Avx.Permute2x128(xyLo, xyHi, 0x20);

        // 0x31 = 0011 0001 (High of Source 2, High of Source 1)
        var v1 = Avx.Permute2x128(xyLo, xyHi, 0x31);

        return (v0, v1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static (Vector256<float> x, Vector256<float> y) 
        Normalize(Vector256<float> vx, Vector256<float> vy)
    {
        // 1. Calculate squared magnitude: (x^2 + y^2)
        // We use FMA (vx * vx + vy * vy) for speed and precision
        Vector256<float> x2 = Avx.Multiply(vx, vx);
        Vector256<float> lengthSq = Fma.MultiplyAdd(vy, vy, x2);

        // 2. Initial approximation of 1 / sqrt(lengthSq)
        Vector256<float> rsqrt = Avx.ReciprocalSqrt(lengthSq);

        // 3. Newton-Raphson refinement (Standard for 24-bit precision)
        // r = r * 0.5 * (3.0 - lengthSq * r * r)
        Vector256<float> three = Vector256.Create(3.0f);
        Vector256<float> half = Vector256.Create(0.5f);
        
        Vector256<float> r2 = Avx.Multiply(rsqrt, rsqrt);
        // Compute (3.0 - lengthSq * r2) using FMA: (-lengthSq * r2 + 3.0)
        Vector256<float> negLengthSq = Avx.Subtract(Vector256<float>.Zero, lengthSq);
        Vector256<float> convergence = Fma.MultiplyAdd(negLengthSq, r2, three);
        
        // Final scaling factor
        rsqrt = Avx.Multiply(rsqrt, Avx.Multiply(half, convergence));

        // 4. Multiply original components by the reciprocal
        return (
            Avx.Multiply(vx, rsqrt),
            Avx.Multiply(vy, rsqrt)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static Vector256<float> Length(Vector256<float> vx, Vector256<float> vy)
    {
        // 1. Calculate lengthSq = x^2 + y^2
        // Start with x * x
        Vector256<float> lengthSq = Avx.Multiply(vx, vx);
        
        // Use FMA for the final component: lengthSq = (vy * vy) + lengthSq
        lengthSq = Fma.MultiplyAdd(vy, vy, lengthSq);

        // 2. Return the square root
        return Avx.Sqrt(lengthSq);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static Vector256<float> Distance(
        Vector256<float> ax, Vector256<float> ay,
        Vector256<float> bx, Vector256<float> by)
    {
        // 1. Calculate Deltas (Difference between A and B)
        Vector256<float> dx = Avx.Subtract(ax, bx);
        Vector256<float> dy = Avx.Subtract(ay, by);

        // 2. Calculate squared magnitude: (dx^2 + dy^2)
        // Start with dx * dx
        Vector256<float> distSq = Avx.Multiply(dx, dx);
        
        // Use FMA to add (dy * dy) into the result
        // distSq = (dy * dy) + distSq
        distSq = Fma.MultiplyAdd(dy, dy, distSq);

        // 3. Final distance is the square root
        return Avx.Sqrt(distSq);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static Vector256<float> DistanceSquared(
        Vector256<float> ax, Vector256<float> ay,
        Vector256<float> bx, Vector256<float> by)
    {
        // 1. Calculate Deltas (8 subtractions per instruction)
        Vector256<float> dx = Avx.Subtract(ax, bx);
        Vector256<float> dy = Avx.Subtract(ay, by);

        // 2. Accumulate squared differences
        // Start with dx * dx
        Vector256<float> distSq = Avx.Multiply(dx, dx);
        
        // Final step: distSq = (dy * dy) + distSq
        // FMA combines the multiply and add into one high-speed instruction
        return Fma.MultiplyAdd(dy, dy, distSq);
    }

}

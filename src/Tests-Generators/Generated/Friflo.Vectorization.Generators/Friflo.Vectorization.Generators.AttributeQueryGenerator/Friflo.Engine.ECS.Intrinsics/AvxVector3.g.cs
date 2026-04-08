
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
// ReSharper disable InconsistentNaming

namespace Friflo.Engine.ECS.Intrinsics;

public static class AvxVector3
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static   (Vector256<float>  X, Vector256<float>  Y, Vector256<float>  Z)
        Deinterleave(Vector256<float> v0, Vector256<float> v1, Vector256<float> v2)
    {
        // --- STEP 1: Lane-Level Alignment ---
        // We use Shuffle (VSHUFPS) to move Zs out of the way. 
        // This is a 1-cycle latency instruction.
        // Result: [X0 Y0 X1 Y1 | X2 Y2 ?? ??] 
        // var x0y0x1y1 = Avx.Shuffle(v0, v0, 0b01_00_01_00);

        // --- STEP 2: The "Magic" Permutes ---
        // Instead of many blends, we use exactly 3 PermuteVar8x32 calls.
        // On modern CPUs, the 3-cycle latency of Permute is hidden 
        // by the sheer throughput of the rest of the pipeline.

        // Masks must be static readonly for maximum performance
        // X indices: 0, 3, 6 (v0), 9, 12, 15 (v1), 18, 21 (v2) -> relative to base
        // Because we process in 256-bit chunks, we use these specific patterns:
        var x       = Avx2.PermuteVar8x32(v0, Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0));
        var x_mid   = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0));
        var x_end   = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5));

        // Combine using the fastest Blend instruction (VBLENDPS)
        x = Avx.Blend(x, x_mid, 0b00111000); // 0x38
        x = Avx.Blend(x, x_end, 0b11000000); // 0xC0

        // Y indices: 1, 4, 7 (v0), 10, 13, 16 (v1), 19, 22 (v2)
        var y           = Avx2.PermuteVar8x32(v0, Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0));
        var y_mid       = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0));
        var y_end_raw   = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6));

        y = Avx.Blend(y, y_mid, 0b00011000); // 0x18
        // Y5 is index 0 of v2, so we blend it manually
        var y5 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        y = Avx.Blend(y,        y5, 0b00100000); // 0x20
        y = Avx.Blend(y, y_end_raw, 0b11000000);

        // Z indices: 2, 5, 8 (v1-index0), 11, 14, 17 (v2), 20, 23
        var z       = Avx2.PermuteVar8x32(v0, Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0));
        var z2      = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0)); // Z2
        var z_mid   = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0));
        var z_end   = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7));

        z = Avx.Blend(z,    z2, 0b00000100); // 0x04
        z = Avx.Blend(z, z_mid, 0b00011000); // 0x18
        z = Avx.Blend(z, z_end, 0b11100000); // 0xE0

        return (x, y, z);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<float> V0, Vector256<float> V1, Vector256<float> V2) Interleave(
        Vector256<float> vx, Vector256<float> vy, Vector256<float> vz)
    {
        // 1. Initial Unpacks (Very fast, Port 5)
        var lowXY  = Avx.UnpackLow(vx, vy);   
        var highXY = Avx.UnpackHigh(vx, vy); 
        
        // 2. Fix Lanes (Required for 3-way stride)
        var xy_0123 = Avx.Permute2x128(lowXY, highXY, 0x20);
        var xy_4567 = Avx.Permute2x128(lowXY, highXY, 0x31);

        // 3. V0: Use immediate Blend instead of BlendVariable
        // Mask 0b00100100 (hex 0x24) targets indices 2 and 5
        var res0 = Avx2.PermuteVar8x32(xy_0123, Vector256.Create(0, 1, 0, 2, 3, 0, 4, 5));
        var z_v0 = Avx2.PermuteVar8x32(vz,      Vector256.Create(0, 0, 0, 0, 0, 1, 0, 0).AsInt32()); // Z0, Z1
        var v0 = Avx.Blend(res0, z_v0, 0x24); 

        // 4. V1: Optimized Bridge
        var v1_xyL = Avx2.PermuteVar8x32(xy_0123, Vector256.Create(0, 6, 7, 0, 0, 0, 0, 0));
        var v1_xyH = Avx2.PermuteVar8x32(xy_4567, Vector256.Create(0, 0, 0, 0, 0, 1, 0, 2));
        var v1_z   = Avx2.PermuteVar8x32(vz,      Vector256.Create(2, 0, 0, 3, 0, 0, 4, 0));
        
        // Combine XY parts with an immediate blend (Port 0/5)
        var v1_tmp = Avx.Blend(v1_xyL, v1_xyH, 0xB0); // 0b10110000
        // Final Z merge
        var v1 = Avx.Blend(v1_tmp, v1_z, 0x49); // 0b01001001 (Indices 0, 3, 6)

        // 5. V2: Optimized Tail
        var res2 = Avx2.PermuteVar8x32(xy_4567, Vector256.Create(3, 0, 4, 5, 0, 6, 7, 0));
        var z_v2 = Avx2.PermuteVar8x32(vz,      Vector256.Create(0, 5, 0, 0, 6, 0, 0, 7));
        var v2 = Avx.Blend(res2, z_v2, 0x92); // 0b10010010 (Indices 1, 4, 7)

        return (v0, v1, v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<float>, Vector256<float>, Vector256<float>) Normalize(
        Vector256<float> vX,
        Vector256<float> vY, 
        Vector256<float> vZ)
    {

       // 1. Calculate dot product (x^2 + y^2 + z^2)
        Vector256<float> x2 = Avx.Multiply(vX, vX);
        Vector256<float> y2 = Avx.Multiply(vY, vY);
        Vector256<float> z2 = Avx.Multiply(vZ, vZ);
        
        Vector256<float> dot = Avx.Add(x2, Avx.Add(y2, z2));

        // 2. Calculate 1 / sqrt(dot)
        // Reciprocal Square Root is much faster than Div(Sqrt())
        Vector256<float> rsqrt = Avx.ReciprocalSqrt(dot);

        // 3. Optional: Newton-Raphson Iteration for higher precision
        // nr = rsqrt * (1.5 - 0.5 * dot * rsqrt * rsqrt)
        Vector256<float> half = Vector256.Create(0.5f);
        Vector256<float> onePointFive = Vector256.Create(1.5f);
        Vector256<float> rsqrtSquared = Avx.Multiply(rsqrt, rsqrt);
        Vector256<float> step1 = Avx.Multiply(dot, rsqrtSquared);
        Vector256<float> step2 = Avx.Multiply(half, step1);
        Vector256<float> step3 = Avx.Subtract(onePointFive, step2);
        rsqrt = Avx.Multiply(rsqrt, step3);

        // 4. Multiply original components by the reciprocal
        var normX = Avx.Multiply(vX, rsqrt);
        var normY = Avx.Multiply(vY, rsqrt);
        var normZ = Avx.Multiply(vZ, rsqrt);
        
        return (normX, normY,  normZ);
    }
}

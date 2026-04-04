// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Lab {

public static class Test_Lab_Vector3
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static unsafe void Test_Lab_Vector3_Transpose()
    {
        var vectors = new Vector3[]
        {
            new Vector3(11, 12, 13),
            new Vector3(21, 22, 23),
            new Vector3(31, 32, 33),
            new Vector3(41, 42, 43),
            new Vector3(51, 52, 53),
            new Vector3(61, 62, 63),
            new Vector3(71, 72, 73),
            new Vector3(81, 82, 83),
        };
        fixed (Vector3* vectors_ptr = vectors)
        {
            var (x,y,z) = Transpose8Vector3s((float*)vectors_ptr);
            var (x2,y2,z2) = Transpose8Vector3sOptimized((float*)vectors_ptr);
            
        }
    }
    
    private static unsafe (Vector256<float> X, Vector256<float> Y, Vector256<float> Z)
        Transpose8Vector3s(float* ptr)
    {
        // Load the 24 floats into three 256-bit registers
        // v0: [X0, Y0, Z0, X1, Y1, Z1, X2, Y2]
        // v1: [Z2, X3, Y3, Z3, X4, Y4, Z4, X5]
        // v2: [Y5, Z5, X6, Y6, Z6, X7, Y7, Z7]
        Vector256<float> v0 = Avx.LoadVector256(ptr);
        Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
        Vector256<float> v2 = Avx.LoadVector256(ptr + 16);

        // --- EXTRACT X ---
        // Indices for X: 0, 3, 6 (from v0), 1, 4, 7 (from v1), 2, 5 (from v2)
        var maskX0 = Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0); // v0 contributions
        var maskX1 = Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0); // v1 contributions
        var maskX2 = Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5); // v2 contributions

        var x = Avx2.PermuteVar8x32(v0, maskX0);
        x = Avx2.BlendVariable(x, Avx2.PermuteVar8x32(v1, maskX1), Vector256.Create(0, 0, 0, -1, -1, -1, 0, 0).AsInt32().AsSingle());
        x = Avx2.BlendVariable(x, Avx2.PermuteVar8x32(v2, maskX2), Vector256.Create(0, 0, 0, 0, 0, 0, -1, -1).AsInt32().AsSingle());

        // --- EXTRACT Y ---
        // Indices for Y: 1, 4, 7 (from v0), 2, 5 (from v1), 0, 3, 6 (from v2)
        var maskY0 = Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0);
        var maskY1 = Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0);
        var maskY2 = Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6); // Note: Y5 is index 0 of v2
        
        var y = Avx2.PermuteVar8x32(v0, maskY0);
        y = Avx2.BlendVariable(y, Avx2.PermuteVar8x32(v1, maskY1), Vector256.Create(0, 0, 0, -1, -1, 0, 0, 0).AsInt32().AsSingle());
        // Special case for Y5 which is at the start of v2
        var y5 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0)); 
        y = Avx2.BlendVariable(y, y5, Vector256.Create(0, 0, 0, 0, 0, -1, 0, 0).AsInt32().AsSingle());
        y = Avx2.BlendVariable(y, Avx2.PermuteVar8x32(v2, maskY2), Vector256.Create(0, 0, 0, 0, 0, 0, -1, -1).AsInt32().AsSingle());

        // --- EXTRACT Z ---
        // Indices for Z: 2, 5 (from v0), 0, 3, 6 (from v1), 1, 4, 7 (from v2)
        var maskZ0 = Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0);
        var maskZ1 = Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0); // Z2 is index 0 of v1
        var maskZ2 = Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7);

        var z = Avx2.PermuteVar8x32(v0, maskZ0);
        var z2 = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        z = Avx2.BlendVariable(z, z2, Vector256.Create(0, 0, -1, 0, 0, 0, 0, 0).AsInt32().AsSingle());
        z = Avx2.BlendVariable(z, Avx2.PermuteVar8x32(v1, maskZ1), Vector256.Create(0, 0, 0, -1, -1, 0, 0, 0).AsInt32().AsSingle());
        z = Avx2.BlendVariable(z, Avx2.PermuteVar8x32(v2, maskZ2), Vector256.Create(0, 0, 0, 0, 0, -1, -1, -1).AsInt32().AsSingle());

        return (x, y, z);
    }
    
    private static unsafe (Vector256<float> X, Vector256<float> Y, Vector256<float> Z) 
        Transpose8Vector3sOptimized(float* ptr)
    {
        // 1. Load 24 floats into three 256-bit registers
        Vector256<float> v0 = Avx.LoadVector256(ptr);      // [X0 Y0 Z0 X1 Y1 Z1 X2 Y2]
        Vector256<float> v1 = Avx.LoadVector256(ptr + 8);  // [Z2 X3 Y3 Z3 X4 Y4 Z4 X5]
        Vector256<float> v2 = Avx.LoadVector256(ptr + 16); // [Y5 Z5 X6 Y6 Z6 X7 Y7 Z7]

        // --- STEP 1: Lane-Level Alignment ---
        // We use Shuffle (VSHUFPS) to move Zs out of the way. 
        // This is a 1-cycle latency instruction.
        // Result: [X0 Y0 X1 Y1 | X2 Y2 ?? ??] 
        var x0y0x1y1 = Avx.Shuffle(v0, v0, 0b01_00_01_00); 
        
        // --- STEP 2: The "Magic" Permutes ---
        // Instead of many blends, we use exactly 3 PermuteVar8x32 calls.
        // On modern CPUs, the 3-cycle latency of Permute is hidden 
        // by the sheer throughput of the rest of the pipeline.
        
        // Masks must be static readonly for maximum performance
        // X indices: 0, 3, 6 (v0), 9, 12, 15 (v1), 18, 21 (v2) -> relative to base
        // Because we process in 256-bit chunks, we use these specific patterns:
        
        var x = Avx2.PermuteVar8x32(v0, Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0));
        var x_mid = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0));
        var x_end = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5));
        
        // Combine using the fastest Blend instruction (VBLENDPS)
        x = Avx.Blend(x, x_mid, 0b00111000); // 0x38
        x = Avx.Blend(x, x_end, 0b11000000); // 0xC0

        // Y indices: 1, 4, 7 (v0), 10, 13, 16 (v1), 19, 22 (v2)
        var y = Avx2.PermuteVar8x32(v0, Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0));
        var y_mid = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0));
        var y_end_raw = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6)); 
        
        y = Avx.Blend(y, y_mid, 0b00011000); // 0x18
        // Y5 is index 0 of v2, so we blend it manually
        var y5 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        y = Avx.Blend(y, y5, 0b00100000);    // 0x20
        y = Avx.Blend(y, y_end_raw, 0b11000000);

        // Z indices: 2, 5, 8 (v1-index0), 11, 14, 17 (v2), 20, 23
        var z = Avx2.PermuteVar8x32(v0, Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0));
        var z2 = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0)); // Z2
        var z_mid = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0));
        var z_end = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7));

        z = Avx.Blend(z, z2, 0b00000100);    // 0x04
        z = Avx.Blend(z, z_mid, 0b00011000); // 0x18
        z = Avx.Blend(z, z_end, 0b11100000); // 0xE0

        return (x, y, z);
    }
}

}


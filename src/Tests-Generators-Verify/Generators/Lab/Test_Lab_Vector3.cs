// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Lab;


public static class Test_Lab_Vector3
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static unsafe void Test_Lab_Vector3_Interleave()
    {
        var vectors = new Vector3[]
        {
            new(11, 12, 13),
            new(21, 22, 23),
            new(31, 32, 33),
            new(41, 42, 43),
            new(51, 52, 53),
            new(61, 62, 63),
            new(71, 72, 73),
            new(81, 82, 83),
        };
        var expect1 = Vector256.Create(new float [] {11, 21, 31, 41, 51, 61, 71, 81});
        var expect2 = Vector256.Create(new float [] {12, 22, 32, 42, 52, 62, 72, 82});
        var expect3 = Vector256.Create(new float [] {13, 23, 33, 43, 53, 63, 73, 83});

        fixed (Vector3* vectors_ptr = vectors)
        {
            var ptr = (float*)vectors_ptr;
            // 1. Load 24 floats into three 256-bit registers
            Vector256<float> v0 = Avx.LoadVector256(ptr); // [X0 Y0 Z0 X1 Y1 Z1 X2 Y2]
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8); // [Z2 X3 Y3 Z3 X4 Y4 Z4 X5]
            Vector256<float> v2 = Avx.LoadVector256(ptr + 16); // [Y5 Z5 X6 Y6 Z6 X7 Y7 Z7]

            var (x1, y1, z1) = AvxVector3.Deinterleave(v0, v1, v2);
            var (x2, y2, z2) = Approach2.Deinterleave(v0, v1, v2);
            var (x3, y3, z3) = Approach3.DeinterleaveExtreme(v0, v1, v2);
            var (x4, y4, z4) = Approach4.Deinterleave(v0,v1,v2);
            
            Assert.That(x1, Is.EqualTo(expect1));
            Assert.That(y1, Is.EqualTo(expect2));
            Assert.That(z1, Is.EqualTo(expect3));
            
            var (res1, res2, res3) = AvxVector3.Interleave(x1,y1,z1);
            Assert.That(res1, Is.EqualTo(v0));
            Assert.That(res2, Is.EqualTo(v1));
            Assert.That(res3, Is.EqualTo(v2));
        }
    }
    
    [Test]
    public static unsafe void Test_Lab_Vector3_Normalize()
    {
        var vectors    = new Vector3[8];
        var normalized = new Vector3[8];
        for (int n = 0; n < 8; n++) {
            vectors[n] = new Vector3(n, n + 100, n+ 200);
        };
        fixed (Vector3* vectors_ptr    = vectors)
        fixed (Vector3* normalized_ptr = normalized)
        {
            var ptr      = (float*)vectors_ptr;
            var norm_ptr = (float*)normalized_ptr;
            // 1. Load 24 floats into three 256-bit registers
            Vector256<float> v0 = Avx.LoadVector256(ptr);
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
            Vector256<float> v2 = Avx.LoadVector256(ptr + 16);
            (v0, v1, v2) = AvxVector3.Deinterleave(v0, v1, v2);

            var (n0, n1, n2) = AvxVector3.Normalize(v0,v1,v2);
            v0 = n0;
            v1 = n1;
            v2 = n2;
            
            (v0, v1, v2) = AvxVector3.Interleave(v0, v1, v2);
            Avx.Store(norm_ptr,       v0);
            Avx.Store(norm_ptr +  8,  v1);
            Avx.Store(norm_ptr + 16,  v2);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  Vector3.Normalize(vectors[n]);
            if (!AreEqual(expect, normalized[n], 1e-7f)) {
                Assert.Fail("not equal");
            }
        }
    }
    
    [Test]
    public static unsafe void Test_Lab_Vector3_Length()
    {
        var vectors    = new Vector3[8];
        var lengths    = new float[8];
        for (int n = 0; n < 8; n++) {
            vectors[n] = new Vector3(n, n + 100, n+ 200);
        };
        fixed (Vector3* vectors_ptr    = vectors)
        fixed (float*   lengths_ptr = lengths)
        {
            var ptr      = (float*)vectors_ptr;
            // 1. Load 24 floats into three 256-bit registers
            Vector256<float> v0 = Avx.LoadVector256(ptr);
            Vector256<float> v1 = Avx.LoadVector256(ptr + 8);
            Vector256<float> v2 = Avx.LoadVector256(ptr + 16);
            (v0, v1, v2) = AvxVector3.Deinterleave(v0, v1, v2);

            var length = AvxVector3.Length(v0,v1,v2);
            
            Avx.Store(lengths_ptr,       length);
        }
        for (int n = 0; n < 8; n++) {
            var expect =  vectors[n].Length();
            Assert.That(expect, Is.EqualTo(lengths[n]));
        }
    }
    
    private static bool AreEqual(Vector3 a, Vector3 b, float epsilon)
    {
        return MathF.Abs(a.X - b.X) < epsilon &&
               MathF.Abs(a.Y - b.Y) < epsilon &&
               MathF.Abs(a.Z - b.Z) < epsilon;
    }
}

public static class Approach2
{
    // Indices for X: 0, 3, 6 (from v0), 1, 4, 7 (from v1), 2, 5 (from v2)
    private static readonly Vector256<int> maskX0 = Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0); // v0 contributions
    private static readonly Vector256<int> maskX1 = Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0); // v1 contributions
    private static readonly Vector256<int> maskX2 = Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5); // v2 contributions

    // Indices for Y: 1, 4, 7 (from v0), 2, 5 (from v1), 0, 3, 6 (from v2)
    private static readonly Vector256<int> maskY0 = Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0);
    private static readonly Vector256<int> maskY1 = Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0);

    private static readonly Vector256<int>
        maskY2 = Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6); // Note: Y5 is index 0 of v2

    // Indices for Z: 2, 5 (from v0), 0, 3, 6 (from v1), 1, 4, 7 (from v2)
    private static readonly Vector256<int> maskZ0 = Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0);
    private static readonly Vector256<int> maskZ1 = Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0); // Z2 is index 0 of v1
    private static readonly Vector256<int> maskZ2 = Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7);

    public static (Vector256<float> X, Vector256<float> Y, Vector256<float> Z)
        Deinterleave(Vector256<float> v0, Vector256<float> v1, Vector256<float> v2)
    {
        // --- EXTRACT X ---
        var x = Avx2.PermuteVar8x32(v0, maskX0);
        x = Avx2.BlendVariable(x, Avx2.PermuteVar8x32(v1, maskX1), Vector256.Create(0, 0, 0, -1, -1, -1, 0, 0).AsInt32().AsSingle());
        x = Avx2.BlendVariable(x, Avx2.PermuteVar8x32(v2, maskX2), Vector256.Create(0, 0, 0, 0, 0, 0, -1, -1).AsInt32().AsSingle());

        // --- EXTRACT Y ---
        var y = Avx2.PermuteVar8x32(v0, maskY0);
        y = Avx2.BlendVariable(y, Avx2.PermuteVar8x32(v1, maskY1), Vector256.Create(0, 0, 0, -1, -1, 0, 0, 0).AsInt32().AsSingle());
        // Special case for Y5 which is at the start of v2
        var y5 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        y = Avx2.BlendVariable(y, y5, Vector256.Create(0, 0, 0, 0, 0, -1, 0, 0).AsInt32().AsSingle());
        y = Avx2.BlendVariable(y, Avx2.PermuteVar8x32(v2, maskY2), Vector256.Create(0, 0, 0, 0, 0, 0, -1, -1).AsInt32().AsSingle());

        // --- EXTRACT Z ---
        var z = Avx2.PermuteVar8x32(v0, maskZ0);
        var z2 = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        z = Avx2.BlendVariable(z, z2, Vector256.Create(0, 0, -1, 0, 0, 0, 0, 0).AsInt32().AsSingle());
        z = Avx2.BlendVariable(z, Avx2.PermuteVar8x32(v1, maskZ1), Vector256.Create(0, 0, 0, -1, -1, 0, 0, 0).AsInt32().AsSingle());
        z = Avx2.BlendVariable(z, Avx2.PermuteVar8x32(v2, maskZ2), Vector256.Create(0, 0, 0, 0, 0, -1, -1, -1).AsInt32().AsSingle());
        return (x, y, z);
    }
}



public static class Approach3
{
    public static (Vector256<float> X, Vector256<float> Y, Vector256<float> Z)
        DeinterleaveExtreme(Vector256<float> v0, Vector256<float> v1, Vector256<float> v2)
    {
        // --- STEP 1: Align components within 128-bit lanes ---
        // Shift right by 4 bytes (1 float) and 8 bytes (2 floats)
        // vpalignr works on 128-bit lanes separately in AVX2
        var v0_S1 = Avx2.AlignRight(v0.AsByte(), v0.AsByte(), 4).AsSingle(); // [Y0 Z0 X1 ?? | Z1 X2 Y2 ??]
        var v0_S2 = Avx2.AlignRight(v0.AsByte(), v0.AsByte(), 8).AsSingle(); // [Z0 X1 ?? ?? | X2 Y2 ?? ??]

        var v1_S1 = Avx2.AlignRight(v1.AsByte(), v1.AsByte(), 4).AsSingle();
        var v1_S2 = Avx2.AlignRight(v1.AsByte(), v1.AsByte(), 8).AsSingle();

        var v2_S1 = Avx2.AlignRight(v2.AsByte(), v2.AsByte(), 4).AsSingle();
        var v2_S2 = Avx2.AlignRight(v2.AsByte(), v2.AsByte(), 8).AsSingle();

        // --- STEP 2: Extract X ---
        // X0 is v0[0], X1 is v0_S2[1] (relative to lane), X2 is v0_S2[4]... 
        // We use Shuffle (VSHUFPS) to pick 2 from A and 2 from B in 1-cycle.
        // Result: [X0, X1, X2, X3 | X4, X5, X6, X7] (Needs cross-lane fix)

        // This part is the "Shuffle Forest" - we use masks to pull the aligned values
        var x_l = Avx.Shuffle(v0, v1_S1, 0b01_00_11_00); // Picks X0, X1 (from v0) and X3, X4 (from v1_S1)

        // --- FINAL OPTIMIZED MAPPING ---
        // To avoid 20+ shuffles, the "Extreme" version actually uses 
        // 3 Permutes but hides them behind the arithmetic.
        // If you strictly want no PermuteVar8x32:

        var x = Avx.Blend(
            Avx.Shuffle(v0, v1, 0b10_01_00_01), // Mixed Xs
            Avx.Shuffle(v1, v2, 0b11_10_01_00),
            0b11110000);

        // --- THE TRUTH ---
        // Even in "Extreme" C++, once you have Vector3 AoS, 
        // the VPERMPS (PermuteVar8x32) instruction is the fastest way 
        // to bridge the 128-bit lane gap in AVX2. 

        // The "Extreme" optimization is actually this:
        var X = Avx2.PermuteVar8x32(v0, Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0));
        X = Avx2.Blend(X, Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0)), 0x38);
        X = Avx2.Blend(X, Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5)), 0xC0);

        var Y = Avx2.PermuteVar8x32(v0, Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0));
        Y = Avx2.Blend(Y, Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0)), 0x18);
        var y5 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        Y = Avx2.Blend(Y, y5, 0x20);
        Y = Avx2.Blend(Y, Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6)), 0xC0);

        var Z = Avx2.PermuteVar8x32(v0, Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0));
        var z2 = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        Z = Avx2.Blend(Z, z2, 0x04);
        Z = Avx2.Blend(Z, Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0)), 0x18);
        Z = Avx2.Blend(Z, Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7)), 0xE0);

        return (X, Y, Z);
    }
}

public static class Approach4
{
    // These masks pick specific indices from v0, v1, and v2 to align them for blending
    private static readonly Vector256<int> MaskV0X = Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0); // X0, X1, X2
    private static readonly Vector256<int> MaskV1X = Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0); // X3, X4, X5
    private static readonly Vector256<int> MaskV2X = Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5); // X6, X7

    private static readonly Vector256<int> MaskV0Y = Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0); // Y0, Y1, Y2
    private static readonly Vector256<int> MaskV1Y = Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0); // Y3, Y4 (Y5 is in V2)
    private static readonly Vector256<int> MaskV2Y = Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6); // Y6, Y7 (Y5 is at index 0)

    private static readonly Vector256<int> MaskV0Z = Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0); // Z0, Z1
    private static readonly Vector256<int> MaskV1Z = Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0); // Z2, Z3, Z4
    private static readonly Vector256<int> MaskV2Z = Vector256.Create(0, 0, 1, 4, 7, 0, 0, 0); // Z5, Z6, Z7

    public static (Vector256<float> X, Vector256<float> Y, Vector256<float> Z)
    Deinterleave(Vector256<float> v0, Vector256<float> v1, Vector256<float> v2)
    {
        // --- X EXTRACTION ---
        // Grab X from each, move to their final target slots, then combine
        var x0 = Avx2.PermuteVar8x32(v0, MaskV0X); 
        var x1 = Avx2.PermuteVar8x32(v1, MaskV1X); 
        var x2 = Avx2.PermuteVar8x32(v2, MaskV2X);
        var x = Avx.Blend(x0, x1, 0b00111000); // Combine X0-2 and X3-5
        x = Avx.Blend(x, x2, 0b11000000);       // Combine with X6-7

        // --- Y EXTRACTION ---
        var y0 = Avx2.PermuteVar8x32(v0, MaskV0Y);
        var y1 = Avx2.PermuteVar8x32(v1, MaskV1Y);
        // Y5 is special: it's at index 0 of v2
        var y2 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6).AsInt32());
        var y5 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 1, 0, 0).AsInt32()); // Pick Y5
        
        var y = Avx.Blend(y0, y1, 0b00011000); // Combine Y0-2 and Y3-4
        y = Avx.Blend(y, y5, 0b00100000);       // Add Y5
        y = Avx.Blend(y, y2, 0b11000000);       // Add Y6-7

        // --- Z EXTRACTION ---
        var z0 = Avx2.PermuteVar8x32(v0, MaskV0Z); // Z0, Z1
        var z1 = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0).AsInt32()); // Z3, Z4
        var z2 = Avx2.PermuteVar8x32(v1, Vector256.Create(0, 0, 1, 0, 0, 0, 0, 0).AsInt32()); // Z2 (from v1 index 0)
        var z3 = Avx2.PermuteVar8x32(v2, Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7).AsInt32()); // Z5, Z6, Z7

        var z = Avx.Blend(z0, z2, 0b00000100); // Combine Z0-1 and Z2
        z = Avx.Blend(z, z1, 0b00011000);       // Add Z3-4
        z = Avx.Blend(z, z3, 0b11100000);       // Add Z5-7

        return (x, y, z);
    }
    
    public static (Vector256<float> V0, Vector256<float> V1, Vector256<float> V2) Interleave_2(
            Vector256<float> vx, 
            Vector256<float> vy, 
            Vector256<float> vz)
        {
// 1. Combine X and Y into two registers that are "lane-fixed"
        // xy_0123: [X0, Y0, X1, Y1, X2, Y2, X3, Y3]
        // xy_4567: [X4, Y4, X5, Y5, X6, Y6, X7, Y7]
        var lowXY  = Avx.UnpackLow(vx, vy);   // [X0, Y0, X1, Y1, X4, Y4, X5, Y5]
        var highXY = Avx.UnpackHigh(vx, vy);  // [X2, Y2, X3, Y3, X6, Y6, X7, Y7]
        
        var xy_0123 = Avx.Permute2x128(lowXY, highXY, 0x20);
        var xy_4567 = Avx.Permute2x128(lowXY, highXY, 0x31);

        // ---------------------------------------------------------
        // OUTPUT 0: [X0, Y0, Z0, X1, Y1, Z1, X2, Y2]
        // ---------------------------------------------------------
        // Source indices from xy_0123: 0, 1, (z), 2, 3, (z), 4, 5
        var res0 = Avx2.PermuteVar8x32(xy_0123,   Vector256.Create(0, 1, 0, 2, 3, 0, 4, 5).AsInt32());
        // Blend in Z0 at index 2, Z1 at index 5
        var z_for_v0 = Avx2.PermuteVar8x32(vz,    Vector256.Create(0, 0, 0, 0, 0, 1, 0, 0).AsInt32());
        res0 = Avx2.BlendVariable(res0, z_for_v0, Vector256.Create(0, 0,-1, 0, 0,-1, 0, 0).AsInt32().AsSingle());

        // ---------------------------------------------------------
        // OUTPUT 1: [Z2, X3, Y3, Z3, X4, Y4, Z4, X5]
        // ---------------------------------------------------------
        // This is the bridge register.
        // Needs: X3, Y3 (xy_0123[6,7]) 
        //        X4, Y4, X5 (xy_4567[0,1,2])
        //        Z2, Z3, Z4 (vz[2,3,4])
        
        // Grab X and Y components first
        var v1_xy0123 = Avx2.PermuteVar8x32(xy_0123, Vector256.Create(0, 6, 7, 0, 0, 0, 0, 0).AsInt32());
        var v1_xy4567 = Avx2.PermuteVar8x32(xy_4567, Vector256.Create(0, 0, 0, 0, 0, 1, 0, 2).AsInt32());
        var res1 = Avx.Blend(v1_xy0123, v1_xy4567, 0b10110000); // Merge XY parts
        
        // Blend in Z2 at index 0, Z3 at index 3, Z4 at index 6
        var z_for_v1 = Avx2.PermuteVar8x32(vz,    Vector256.Create( 2, 0, 0, 3, 0, 0, 4, 0).AsInt32());
        res1 = Avx2.BlendVariable(res1, z_for_v1, Vector256.Create(-1, 0, 0,-1, 0, 0,-1, 0).AsInt32().AsSingle());

        // ---------------------------------------------------------
        // OUTPUT 2: [Y5, Z5, X6, Y6, Z6, X7, Y7, Z7]
        // ---------------------------------------------------------
        // Needs: Y5 (xy_4567[3])
        //        X6, Y6, X7, Y7 (xy_4567[4,5,6,7])
        //        Z5, Z6, Z7 (vz[5,6,7])
        
        var res2 = Avx2.PermuteVar8x32(xy_4567,   Vector256.Create(3, 0, 4, 5, 0, 6, 7, 0).AsInt32());
        // Blend in Z5 at index 1, Z6 at index 4, Z7 at index 7
        var z_for_v2 = Avx2.PermuteVar8x32(vz,    Vector256.Create(0, 5, 0, 0, 6, 0, 0, 7).AsInt32());
        res2 = Avx2.BlendVariable(res2, z_for_v2, Vector256.Create(0,-1, 0, 0,-1, 0, 0,-1).AsInt32().AsSingle());

        return (res0, res1, res2);
    }
    

}




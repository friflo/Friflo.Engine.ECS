using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
// ReSharper disable InconsistentNaming

namespace Tests.Generators.Lab;

public static class AvxVector4
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<float> X, Vector256<float> Y, Vector256<float> Z, Vector256<float> W) Deinterleave(
        Vector256<float> v0, // [X0 Y0 Z0 W0 | X1 Y1 Z1 W1]
        Vector256<float> v1, // [X2 Y2 Z2 W2 | X3 Y3 Z3 W3]
        Vector256<float> v2, // [X4 Y4 Z4 W4 | X5 Y5 Z5 W5]
        Vector256<float> v3) // [X6 Y6 Z6 W6 | X7 Y7 Z7 W7]
    {
        // --- STAGE 1: 32-bit Unpacks (Interleave within lanes) ---
        // Pairs elements: [X0, X2], [Y0, Y2], etc.
        var xya = Avx.UnpackLow(v0, v1);  // [X0, X2, Y0, Y2 | X1, X3, Y1, Y3]
        var zwa = Avx.UnpackHigh(v0, v1); // [Z0, Z2, W0, W2 | Z1, Z3, W1, W3]
        var xyb = Avx.UnpackLow(v2, v3);  // [X4, X6, Y4, Y6 | X5, X7, Y5, Y7]
        var zwb = Avx.UnpackHigh(v2, v3); // [Z4, Z6, W4, W6 | Z5, Z7, W5, W7]

        // --- STAGE 2: 64-bit Unpacks (The Double Trick) ---
        // Groups components: [X0, X2, X4, X6]
        var x_half = Avx.UnpackLow (Vector256.AsDouble(xya), Vector256.AsDouble(xyb)); 
        var y_half = Avx.UnpackHigh(Vector256.AsDouble(xya), Vector256.AsDouble(xyb));
        var z_half = Avx.UnpackLow (Vector256.AsDouble(zwa), Vector256.AsDouble(zwb));
        var w_half = Avx.UnpackHigh(Vector256.AsDouble(zwa), Vector256.AsDouble(zwb));

        // --- STAGE 3: 128-bit Lane Fix (The Final Merge) ---
        // At this point, x_half (as floats) is: [X0, X2, X4, X6 | X1, X3, X5, X7]
        // We use Permute2x128 to interleave the 128-bit halves to get [X0..X7]
        
        // Mask 0x20: Low 128 of Source1, Low 128 of Source2
        // Mask 0x31: High 128 of Source1, High 128 of Source2
        
        // To get [X0, X1, X2, X3, X4, X5, X6, X7], we need a specific blend/permute.
        // The most efficient way to linearize this in AVX2 is actually:
        var resX = Avx2.PermuteVar8x32(Vector256.AsSingle(x_half), Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7).AsInt32());
        var resY = Avx2.PermuteVar8x32(Vector256.AsSingle(y_half), Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7).AsInt32());
        var resZ = Avx2.PermuteVar8x32(Vector256.AsSingle(z_half), Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7).AsInt32());
        var resW = Avx2.PermuteVar8x32(Vector256.AsSingle(w_half), Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7).AsInt32());

        return (resX, resY, resZ, resW);
    }
}
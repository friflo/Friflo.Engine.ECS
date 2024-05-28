// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Internal.SIMD {

[ExcludeFromCodeCoverage]
public class VectorOps
{
    public static readonly VectorOps Instance = GetInstance();

    private static VectorOps GetInstance () {
#if   UNITY_BURST
        return Unity.Burst.Intrinsics.X86.Sse2.IsSse2Supported  ? new VectorOpsUnity()  : new VectorOps();
#elif NETCOREAPP3_0 || NETCOREAPP3_0_OR_GREATER
        return System.Runtime.Intrinsics.X86.Sse2.IsSupported   ? new VectorOpsCLR()    : new VectorOps();
#else
        return new VectorOps();
#endif
    }
        
    /// <summary>
    /// Using a specific SIMD implementation like VectorOpsCLR gain performance boost by factor 3   
    /// </summary>
    public virtual void Xor(
        byte[] dest,    int destPos,
        byte[] src,     int srcPos,
        byte[] mask,    int maskPos,
        int length)
    {
        // --- SISD
        for (int n = 0; n < length; n++) {
            var b = src[srcPos + n];
            dest[destPos + n] = (byte)(b ^ mask[(maskPos + n) % 4]);
        }
    }
        
    public virtual void Populate(byte[] arr) { }
        
    protected void PopulateVector(byte[] arr) {
        arr[4] = arr [8] = arr[12] = arr[16] = arr[0];
        arr[5] = arr [9] = arr[13] = arr[17] = arr[1];
        arr[6] = arr[10] = arr[14] = arr[18] = arr[2];
        arr[7] = arr[11] = arr[15] = arr[19] = arr[3];
    }
}

}
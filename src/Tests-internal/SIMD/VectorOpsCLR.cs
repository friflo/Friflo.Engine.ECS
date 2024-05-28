// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics.X86;

// ReSharper disable InconsistentNaming
namespace Internal.SIMD {

[ExcludeFromCodeCoverage]
public sealed class VectorOpsCLR : VectorOps
{
    public override unsafe void Xor(
        byte[] dest,    int destPos,
        byte[] src,     int srcPos,
        byte[] mask,    int maskPos,
        int length)
    {
        int     n = 0;
        const   int vectorSize = 16; // 128 bit
        fixed (byte* destPointer  = dest)
        fixed (byte* srcPointer   = src)
        fixed (byte* maskPointer  = mask)
        {
            var end = length - vectorSize;
            for (; n <= end; n += vectorSize) {
                var bufferVector        = Sse2.LoadVector128(srcPointer   + srcPos + n);
                var maskingKeyVector    = Sse2.LoadVector128(maskPointer  + (maskPos + n) % 4);
                var xor                 = Sse2.Xor(bufferVector, maskingKeyVector);
                Sse2.Store(destPointer + destPos + n, xor);
            }
        }
        // remaining bytes
        base.Xor(dest, destPos + n, src, srcPos + n, mask, maskPos + n, length - n);
    }

    public override void Populate(byte[] arr) {
        PopulateVector(arr);
    }
}

}
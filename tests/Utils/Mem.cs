using System;
using NUnit.Framework;

namespace Tests.Utils;

public static class Mem
{
    public static long GetAllocatedBytes() {
        return GC.GetAllocatedBytesForCurrentThread();
    }
        
    /// <summary>Assert no allocation were performed</summary>
    public static void AssertNoAlloc(long start) {
        var diff =  GC.GetAllocatedBytesForCurrentThread() - start;
        if (diff == 0) {
            return;
        }
        var msg = $"expected no allocation.\n but was: {diff}";
        throw new AssertionException(msg);
    }
}
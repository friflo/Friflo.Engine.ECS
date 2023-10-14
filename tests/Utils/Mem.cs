using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Tests.Utils;

[ExcludeFromCodeCoverage]
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
    
    /// <summary>Assert allocation of expected bytes</summary>
    public static void AssertAlloc(long start, long expected) {
        var diff =  GC.GetAllocatedBytesForCurrentThread() - start;
        if (diff == expected) {
            return;
        }
        var msg = $"expected allocation of {expected} bytes.\n but was: {diff}";
        throw new AssertionException(msg);
    }
}
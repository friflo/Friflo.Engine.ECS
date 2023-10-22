using System;
using System.Collections.Generic;
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
    public static long AssertNoAlloc(long start) {
        long current    = GC.GetAllocatedBytesForCurrentThread();
        var diff        = current - start;
        if (diff == 0) {
            return current;
        }
        var msg = $"expected no allocation.\n but was: {diff}";
        throw new AssertionException(msg);
    }
    
    /// <summary>Assert allocation of expected bytes</summary>
    public static long AssertAlloc(long start, long expected) {
        long current    = GC.GetAllocatedBytesForCurrentThread();
        var diff        = current - start;
        if (diff == expected) {
            return current;
        }
        var msg = $"expected allocation of {expected} bytes.\n but was: {diff}";
        throw new AssertionException(msg);
    }
    
    /// <summary>
    /// Similar behavior as <see cref="Assert.AreEqual(object, object)"/> but without memory allocation.<br/>
    /// It also requires both parameters are of the same type.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="Mem()"/> on initialization of <see cref="Mem"/> utility class to force one time allocations
    /// of default types like: int, float, double, ... .
    /// </remarks>
    public static void AreEqual<T>(T expect, T actual) {
        if (EqualityComparer<T>.Default.Equals(expect, actual)) {
            return;
        }
        var msg = $"Expect: {expect}\n  But was:  {actual}";
        Assert.Fail(msg);
    }
    
    /// <summary>used for <see cref="AreEqual{T}"/></summary>
    static Mem() {
        // force one time allocations for common types
        AreEqual        (1, 1);
        AreEqual<uint>  (1, 1);
        AreEqual<byte>  (1, 1);
        AreEqual<sbyte> (1, 1);
        AreEqual<short> (1, 1);
        AreEqual<ushort>(1, 1);
        AreEqual<long>  (1, 1);
        AreEqual<ulong> (1, 1);
        AreEqual<float> (1, 1);
        AreEqual<double>(1, 1);
    }
    
    public static bool IsDebug => IsDebugInternal();
    
    private static bool IsDebugInternal() {
#if DEBUG
        return true;
#else
        return false;
#endif        
    }

}
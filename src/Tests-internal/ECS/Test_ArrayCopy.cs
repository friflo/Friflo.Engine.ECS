using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Internal.ECS {

// ReSharper disable InconsistentNaming
public static class Test_ArrayCopy
{
    [Test]
    public static void Perf_ArrayCopy() {
        var count = 10; // 100_000_000
        var bytesSource = new byte[100];
        var bytesTarget = new byte[100];
        Array_Copy  (bytesSource, bytesTarget, count);
        Span_CopyTo (bytesSource, bytesTarget, count);
    }
    
    private static void Array_Copy<T>(T[] source, T[] target, int count) where T : struct
    {
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (int n = 0; n < count; n++) {
            Array.Copy(source, target, source.Length);
            // Buffer.BlockCopy(source, 0, target, 0, source.Length);  // even slower for byte[] than Array.Copy()
        }
        Console.WriteLine($"Array.Copy() Type: {typeof(T).Name} {stopwatch.ElapsedMilliseconds} ms");
    }
    
    private static void Span_CopyTo<T>(T[] source, T[] target, int count) where T : struct
    {
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (int n = 0; n < count; n++) {
            // execute ~ 40% faster if Length = 10.
            // execute ~ 10% faster if Length > 10
            ReadOnlySpan<T> sourceSpan = source;
            Span<T>         targetSpan = target;
            sourceSpan.CopyTo(targetSpan);
        }
        Console.WriteLine($"Span.CopyTo() Type: {typeof(T).Name} {stopwatch.ElapsedMilliseconds} ms");
    }
}

}
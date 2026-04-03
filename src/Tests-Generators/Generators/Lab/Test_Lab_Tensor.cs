using System;
using System.Runtime.Intrinsics.X86;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
using NUnit.Framework;

namespace Tests.Generators.Lab;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SpanAttribute : Attribute { }

public static class Test_Lab_Tensor
{
    // Friflo.Vectorization.Generators
    [Vectorize]
    private static void MultiplyAdd([Span] ref float a, [Span] float b, float c) {
        a = (a * b) + c;
    }
    
    public static void MultiplyAddVector(Span<float> a, ReadOnlySpan<float> b, float c, bool vectorized = true)
    {
        int n = 0;
        if (vectorized) {
            if (Avx.IsSupported) {
                // n = MultiplyAddVector_Avx(a, b, c);
            }
        }
        int len = a.Length;
        for (int i = 0; i < len; i++) {
            MultiplyAdd(ref a[i], b[i], c);      
        }
    }
    
    [Test]
    public static void Test_MultiplyAdd()
    {
        var a = new float[1000];
        var b = new float[1000];
        for (int n = 0; n < 1000; n++) {
            a[n] = n;
            b[n] = 2 * n;
        }
        MultiplyAddVector(a, b, 5);
    }
    
}
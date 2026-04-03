using System;
using NUnit.Framework;

namespace Tests.Generators.Lab;


[AttributeUsage(AttributeTargets.Method)]
public sealed class TensorizeAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SpanAttribute : Attribute { }

public static class Test_Lab_Tensorize
{
    [Tensorize]
    private static void MultiplyAdd([Span] ref float a, [Span] float b, float c) {
        a = (a * b) + c;
    }
    
    public static void MultiplyAddVector(Span<float> a, ReadOnlySpan<float> b, float c)
    {
        
    }
    
    [Test]
    public static void Test_MultiplyAdd()
    {
        var a = new float[1000];
        var b = new float[1000];
        MultiplyAddVector(a, b, 5);
    }
    
}
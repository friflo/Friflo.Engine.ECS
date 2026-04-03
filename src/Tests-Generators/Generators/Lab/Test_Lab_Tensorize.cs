using System;

namespace Tests.Generators.Lab;


[AttributeUsage(AttributeTargets.Method)]
public sealed class TensorizeAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SpanAttribute : Attribute { }

public static class Test_Lab_Tensorize
{
    [Tensorize]
    public static void MultiplyAdd([Span] ref float a, [Span] float b, float c)
    {
        
    }
    
    public static void MultiplyAddExecute(Span<float> a, ReadOnlySpan<float> b, float c)
    {
        
    }
}
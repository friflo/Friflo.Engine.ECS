using System.Numerics;
using NUnit.Framework;

namespace Tests.SIMD {

public static class VectorTest
{
    [Test]
    public static void Test_Vector() {
        var v1 = new Vector2(0.1f, 0.2f);
        var v2 = new Vector2(1.1f, 2.2f);
        var vResult = v1 + v2;
        Assert.AreEqual(new Vector2(1.2f, 2.4f), vResult);
    }
}
/*
[ExcludeFromCodeCoverage]
public static class MyExtensions
{
    public static T Sum<T>(this IEnumerable<T> source)
        where T : struct, IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        if (source.GetType() == typeof(T[])) {
            return Sum(Unsafe.As<T[]>(source));
        }
        var sum = T.AdditiveIdentity;
        foreach (var value in source)
        {
            sum += value;
        }
        return sum;
    }

    private static T Sum<T>(this T[] source)
        where T : struct, IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
        => Sum<T>(source.AsSpan());

    private static T Sum<T>(this ReadOnlySpan<T> source)
        where T : struct, IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        var sum = T.AdditiveIdentity;
        // check if SIMD is available and can be used
        if(Vector.IsHardwareAccelerated && Vector<T>.IsSupported && source.Length > Vector<T>.Count)
        {
            var sumVector = Vector<T>.Zero; // initialize to zeros

            // cast the span to a span of vectors  
            var vectors = MemoryMarshal.Cast<T, Vector<T>>(source);   
 
            // add each vector to the sum vector
            foreach (ref readonly var vector in vectors) {
                sumVector += vector;
            }
            // get the sum of all elements of the vector
            sum = Vector.Sum(sumVector);
            // find what elements of the source were left out
            var remainder = source.Length % Vector<T>.Count;
            source = source[^remainder..];
        }
        // sum all elements not handled by SIMD
        foreach (ref readonly var value in source)
        {
            sum += value;
        }
        return sum;
    }
}
*/
}
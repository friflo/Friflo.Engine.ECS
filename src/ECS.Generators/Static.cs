namespace Friflo.Engine.ECS.Generators;

public class Static
{
    internal static string Code = @"
using static System.MathF;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Friflo.Engine.ECS.Intrinsics
{
    internal static class AvxUtils
    {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector256<float> TransformVector4PairAVX(
            Vector256<float> v, Vector256<float> c0, Vector256<float> c1, Vector256<float> c2, Vector256<float> c3)
        {
            // Shuffle/Broadcast components
            // We use Avx.Shuffle to pick x,y,z,w and broadcast them across the 4 lanes for each vector
            Vector256<float> xxxx = Avx.Shuffle(v, v, 0b00_00_00_00); 
            Vector256<float> yyyy = Avx.Shuffle(v, v, 0b01_01_01_01);
            Vector256<float> zzzz = Avx.Shuffle(v, v, 0b10_10_10_10);
            Vector256<float> wwww = Avx.Shuffle(v, v, 0b11_11_11_11);
            return Fma.MultiplyAdd(wwww, c3, Fma.MultiplyAdd(zzzz, c2, Fma.MultiplyAdd(yyyy, c1, Avx.Multiply(xxxx, c0))));
        }

        [SkipLocalsInit]
        internal static Vector256<float> SinMathF(Vector256<float> x)
        {
            return Vector256.Create(Sin(x[0]), Sin(x[1]), Sin(x[2]), Sin(x[3]), Sin(x[4]), Sin(x[5]), Sin(x[6]), Sin(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> CosMathF(Vector256<float> x)
        {
            return Vector256.Create(Cos(x[0]), Cos(x[1]), Cos(x[2]), Cos(x[3]), Cos(x[4]), Cos(x[5]), Cos(x[6]), Cos(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> TanMathF(Vector256<float> x)
        {
            return Vector256.Create(Tan(x[0]), Tan(x[1]), Tan(x[2]), Tan(x[3]), Tan(x[4]), Tan(x[5]), Tan(x[6]), Tan(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> AsinMathF(Vector256<float> x)
        {
            return Vector256.Create(Asin(x[0]), Asin(x[1]), Asin(x[2]), Asin(x[3]), Asin(x[4]), Asin(x[5]), Asin(x[6]), Asin(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> AcosMathF(Vector256<float> x)
        {
            return Vector256.Create(Acos(x[0]), Acos(x[1]), Acos(x[2]), Acos(x[3]), Acos(x[4]), Acos(x[5]), Acos(x[6]), Acos(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> AtanMathF(Vector256<float> x)
        {
            return Vector256.Create(Atan(x[0]), Atan(x[1]), Atan(x[2]), Atan(x[3]), Atan(x[4]), Atan(x[5]), Atan(x[6]), Atan(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> Atan2MathF(Vector256<float> y, Vector256<float> x)
        {
            return Vector256.Create(Atan2(y[0],x[0]), Atan2(y[1],x[1]), Atan2(y[2],x[2]), Atan2(y[3],x[3]), Atan2(y[4],x[4]), Atan2(y[5],x[5]), Atan2(y[6],x[6]), Atan2(y[7],x[7]));
        }

        [SkipLocalsInit]
        internal static Vector256<float> TruncateMathF(Vector256<float> x)
        {
            return Vector256.Create(Truncate(x[0]), Truncate(x[1]), Truncate(x[2]), Truncate(x[3]), Truncate(x[4]), Truncate(x[5]), Truncate(x[6]), Truncate(x[7]));
        }
    }
}
";
}
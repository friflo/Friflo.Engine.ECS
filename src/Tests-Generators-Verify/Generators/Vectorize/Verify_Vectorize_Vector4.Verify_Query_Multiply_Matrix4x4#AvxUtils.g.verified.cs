//HintName: Friflo.Vectorization.Intrinsics/AvxUtils.g.cs

using static System.MathF;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Friflo.Vectorization.Intrinsics
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
    }

    internal static class MathUtils
    {
        // ------ Trigonometry
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
        internal static Vector256<float> AsinhMathF(Vector256<float> x)
        {
            return Vector256.Create(Asinh(x[0]), Asinh(x[1]), Asinh(x[2]), Asinh(x[3]), Asinh(x[4]), Asinh(x[5]), Asinh(x[6]), Asinh(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> AcoshMathF(Vector256<float> x)
        {
            return Vector256.Create(Acosh(x[0]), Acosh(x[1]), Acosh(x[2]), Acosh(x[3]), Acosh(x[4]), Acosh(x[5]), Acosh(x[6]), Acosh(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float> AtanhMathF(Vector256<float> x)
        {
            return Vector256.Create(Atanh(x[0]), Atanh(x[1]), Atanh(x[2]), Atanh(x[3]), Atanh(x[4]), Atanh(x[5]), Atanh(x[6]), Atanh(x[7]));
        }
        
        // ------ misc
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    AbsMathF(Vector256<float> x)
        {
            return Vector256.Create(Abs(x[0]), Abs(x[1]), Abs(x[2]), Abs(x[3]), Abs(x[4]), Abs(x[5]), Abs(x[6]), Abs(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    TruncateMathF(Vector256<float> x)
        {
            return Vector256.Create(Truncate(x[0]), Truncate(x[1]), Truncate(x[2]), Truncate(x[3]), Truncate(x[4]), Truncate(x[5]), Truncate(x[6]), Truncate(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    FloorMathF(Vector256<float> x)
        {
            return Vector256.Create(Floor(x[0]), Floor(x[1]), Floor(x[2]), Floor(x[3]), Floor(x[4]), Floor(x[5]), Floor(x[6]), Floor(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    CeilingMathF(Vector256<float> x)
        {
            return Vector256.Create(Ceiling(x[0]), Ceiling(x[1]), Ceiling(x[2]), Ceiling(x[3]), Ceiling(x[4]), Ceiling(x[5]), Ceiling(x[6]), Ceiling(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    ExpMathF(Vector256<float> x)
        {
            return Vector256.Create(Exp(x[0]), Exp(x[1]), Exp(x[2]), Exp(x[3]), Exp(x[4]), Exp(x[5]), Exp(x[6]), Exp(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    LogMathF(Vector256<float> x)
        {
            return Vector256.Create(Log(x[0]), Log(x[1]), Log(x[2]), Log(x[3]), Log(x[4]), Log(x[5]), Log(x[6]), Log(x[7]));
        }
        
        [SkipLocalsInit]
        internal static Vector256<float>    Log10MathF(Vector256<float> x)
        {
            return Vector256.Create(Log10(x[0]), Log10(x[1]), Log10(x[2]), Log10(x[3]), Log10(x[4]), Log10(x[5]), Log10(x[6]), Log10(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    Log2MathF(Vector256<float> x)
        {
            return Vector256.Create(Log2(x[0]), Log2(x[1]), Log2(x[2]), Log2(x[3]), Log2(x[4]), Log2(x[5]), Log2(x[6]), Log2(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    PowMathF(Vector256<float> x, Vector256<float> y)
        {
            return Vector256.Create(Pow(x[0],y[0]), Pow(x[1],y[1]), Pow(x[2],y[2]), Pow(x[3],y[3]), Pow(x[4],y[4]), Pow(x[5],y[5]), Pow(x[6],y[6]), Pow(x[7],y[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float>    RoundMathF(Vector256<float> x)
        {
            return Vector256.Create(Round(x[0]), Round(x[1]), Round(x[2]), Round(x[3]), Round(x[4]), Round(x[5]), Round(x[6]), Round(x[7]));
        }
        
        [SkipLocalsInit]                    // TODO Vectorize in AvxUtils
        internal static Vector256<float> SqrtMathF(Vector256<float> x)
        {
            return Vector256.Create(Sqrt(x[0]), Sqrt(x[1]), Sqrt(x[2]), Sqrt(x[3]), Sqrt(x[4]), Sqrt(x[5]), Sqrt(x[6]), Sqrt(x[7]));
        }
    }
}

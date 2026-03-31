
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Friflo.Engine.ECS.Intrinsics
{
    internal static class AvxUtils
    {
        private static Vector256<float> TransformVector4PairAVX2(
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
}

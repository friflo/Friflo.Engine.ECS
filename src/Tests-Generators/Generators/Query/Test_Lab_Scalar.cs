using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;

// ReSharper disable InconsistentNaming


// ReSharper disable InconsistentNaming
namespace Tests.Generators.Query {

public static class Test_Lab_Scalar
{
    [Test]
    public static void Test_Lab_Scalar_Call() {
        var position = new Position[8];
        var value   = new float[8];
        for (int n = 0; n < 8; n++) {
            position[n] = new Position(1, 1, 1);
            value[n] = n + 1;
        }
        MultiplayScalarArray_Avx(position, value);
    }
    
    private static unsafe int MultiplayScalarArray_Avx(Span<Position> position, Span<float> value)
    {
        Vector256<int> value_mask_0 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);
        Vector256<int> value_mask_1 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);
        Vector256<int> value_mask_2 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);
        
        int i = 0;
        var end = position.Length - 8;
        fixed (Position* position_first = position)
        fixed (float* scalar_first = value)
        {
            for (; i <= end; i += 8)
            {
                float* position_ptr = (float*)(position_first + i);
                float* scalar_ptr   = (float*)(scalar_first + i);

                // 1. LOAD: 3 registers filled with interleaved XYZ data
                Vector256<float> position_0 = Avx.LoadVector256(position_ptr);      // [X0 Y0 Z0 X1 Y1 Z1 X2 Y2]
                Vector256<float> position_1 = Avx.LoadVector256(position_ptr + 8);  // [Z2 X3 Y3 Z3 X4 Y4 Z4 X5]
                Vector256<float> position_2 = Avx.LoadVector256(position_ptr + 16); // [Y5 Z5 X6 Y6 Z6 X7 Y7 Z7]
                
                Vector256<float> value_scalar = Avx.LoadVector256(scalar_ptr);
                
                Vector256<float> value_spread_0 = Avx2.PermuteVar8x32(value_scalar, value_mask_0);
                Vector256<float> value_spread_1 = Avx2.PermuteVar8x32(value_scalar, value_mask_1);
                Vector256<float> value_spread_2 = Avx2.PermuteVar8x32(value_scalar, value_mask_2);
                
                // 2. COMPUTE: Directly in the interleaved state!
                position_0 = Avx.Add(position_0, value_spread_0);
                position_1 = Avx.Add(position_1, value_spread_1);
                position_2 = Avx.Add(position_2, value_spread_2);

                // 3. STORE: 3 fast block writes
                Avx.Store(position_ptr, position_0);
                Avx.Store(position_ptr + 8, position_1);
                Avx.Store(position_ptr + 16, position_2);
            }
        }
        return i;
    }
}

}


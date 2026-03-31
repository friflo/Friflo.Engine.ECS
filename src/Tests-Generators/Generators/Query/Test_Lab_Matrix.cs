// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Query;

public static class Test_Lab_Matrix
{
    // ------------------------------- scalar component -------------------------------
    [Test]
    public static void Test_Lab_Matrix_Call()
    {
        Matrix4x4 rot = Matrix4x4.CreateFromYawPitchRoll(
            10f * (MathF.PI / 180.0f), // Yaw
            20f * (MathF.PI / 180.0f), // Pitch
            30f * (MathF.PI / 180.0f)  // Roll
        );
        Matrix4x4 trans = Matrix4x4.CreateTranslation(new Vector3(1f, 2f, 3f));
        var matrix = Matrix4x4.Multiply(rot, trans);
        
        var position = new Vector4[1024];
        for (int n = 0; n < 1024; n++) {
            position[n] = new Vector4(n, 2 * n, n + 10, 0);
        }
        var result2 = new Vector4[1024];
        TransformVector4Array_Unroll2_Avx(position, matrix, result2);
        
        var result4 = new Vector4[1024];
        MultiplyUnrolled4(position, matrix, result4);
        
        var result_naive = new Vector4[1024];
        TransformVector4Array_naive(position, matrix, result_naive);
        
        for (int n =  0; n < position.Length; n++) {
            Assert.That(result2[n], Is.EqualTo(result_naive[n]));
            Assert.That(result4[n], Is.EqualTo(result_naive[n]));
            // Avx implementation has precision errors
            var areEqual = AreEqual(result4[n], result_naive[n]);
            if (!areEqual) {
                Assert.Fail($"expect: {result_naive[n]}\nwas:    {result4[n]}");
            }
        }
    }
    
    private static bool AreEqual(Vector4 a, Vector4 b, float epsilon = 1e-5f)
    {
        return Math.Abs(a.X - b.X) < epsilon &&
               Math.Abs(a.Y - b.Y) < epsilon &&
               Math.Abs(a.Z - b.Z) < epsilon &&
               Math.Abs(a.W - b.W) < epsilon;
    }
    
    [Test]
    public static void Test_Lab_Matrix_perf()
    {
        Matrix4x4 rot = Matrix4x4.CreateFromYawPitchRoll(
            10f * (MathF.PI / 180.0f), // Yaw
            20f * (MathF.PI / 180.0f), // Pitch
            30f * (MathF.PI / 180.0f)  // Roll
        );
        Matrix4x4 trans = Matrix4x4.CreateTranslation(new Vector3(1f, 2f, 3f));
        var matrix = Matrix4x4.Multiply(rot, trans);
        
        var position = new Vector4[1024];
        for (int n = 0; n < 1024; n++) {
            position[n] = new Vector4(n, 2 * n, n + 10, 0);
        }
        var result = new Vector4[1024];
        var repeat = 10_000_000; // 10_000_000;
        for (int n = 0; n < repeat; n++) {
            // TransformVector4Array_Unroll2_Avx(position, matrix, result);
            // TransformVector4Array_Unroll4_Avx(position, matrix, result);
            // TransformVector4Array_naive(position, matrix, result);
            MultiplyUnrolled4(position, matrix, result);
        }
    }
    
    private static void TransformVector4Array_naive(Vector4[] src, Matrix4x4 matrix, Vector4[] dst)
    {
        int len = src.Length;
        for (int i = 0; i < len; i++) {
            dst[i] = Vector4.Transform(src[i], matrix);
        }
    }
    
    // Note!
    // Apply loop unrolling to improve performance by processing 4 or 8 vectors per iteration (instead of 2)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void TransformVector4Array_Unroll2_Avx(Vector4[] src, Matrix4x4 matrix, Vector4[] dst)
    {
        int i = 0;
        var end = src.Length - 2;
        
        fixed (Vector4* srcPtr = src)
        fixed (Vector4* dstPtr = dst)
        {
            // Load Matrix columns into 256-bit registers.
            // We use Broadcast to mirror the 128-bit column across the whole 256-bit register.
            // Result: [Col.x, Col.y, Col.z, Col.w, Col.x, Col.y, Col.z, Col.w]
            var col1 = Avx.BroadcastVector128ToVector256(&matrix.M11);
            var col2 = Avx.BroadcastVector128ToVector256(&matrix.M21);
            var col3 = Avx.BroadcastVector128ToVector256(&matrix.M31);
            var col4 = Avx.BroadcastVector128ToVector256(&matrix.M41);

            // Main Loop: Process 2 Vector4s per iteration (32 bytes total)
            for (; i <= end; i += 2)
            {
                // 1. Load two Vector4s: [v1.x, v1.y, v1.z, v1.w, v2.x, v2.y, v2.z, v2.w]
                Vector256<float> v = Avx.LoadVector256((float*)(srcPtr + i));

                // 2. Extract and broadcast individual components (x, y, z, w)
                // Shuffle creates a vector where all 4 slots in each 128-bit lane are the same component.
                // e.g., xxxx = [v1.x, v1.x, v1.x, v1.x, v2.x, v2.x, v2.x, v2.x]
                var xxxx = Avx.Shuffle(v, v, 0b00_00_00_00);
                var yyyy = Avx.Shuffle(v, v, 0b01_01_01_01);
                var zzzz = Avx.Shuffle(v, v, 0b10_10_10_10);
                var wwww = Avx.Shuffle(v, v, 0b11_11_11_11);

                // 3. The Math: result = (x * col1) + (y * col2) + (z * col3) + (w * col4)
                // We start with (w * col4) to handle translation efficiently if w=1.
                Vector256<float> result = Avx.Multiply(wwww, col4);
                
                // Use FMA (Fused Multiply-Add) to accumulate the remaining columns.
                // This is faster and more precise than separate Multiply/Add calls.
                result = Fma.MultiplyAdd(xxxx, col1, result);
                result = Fma.MultiplyAdd(yyyy, col2, result);
                result = Fma.MultiplyAdd(zzzz, col3, result);

                // 4. Store the two transformed results back to memory
                Avx.Store((float*)(dstPtr + i), result);
            }
        }
    }
    
    // prompt: Show avx implementation for Vector4.Transform() operating on Vector4 array and a Matrix4x4 with Loop Unroll 4
    private static unsafe void TransformVector4Array_Unroll4_Avx(Vector4[] src, Matrix4x4 matrix, Vector4[] dst)
    {
        int count = src.Length;
        fixed (Vector4* pSrc = src, pDst = dst)
        {
            float* s = (float*)pSrc;
            float* d = (float*)pDst;

            // Load Matrix Columns into 256-bit registers (4 floats each)
            // We only use the lower 128 bits of each Vector256 or use Vector128
            // For AVX2, we often load them into Vector128 to save register space
            Vector128<float> col1 = Vector128.Create(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
            Vector128<float> col2 = Vector128.Create(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
            Vector128<float> col3 = Vector128.Create(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
            Vector128<float> col4 = Vector128.Create(matrix.M41, matrix.M42, matrix.M43, matrix.M44);

            int i = 0;
            // Loop Unroll 4: Process 4 Vector4s per iteration
            for (; i <= count - 4; i += 4)
            {
                // Pointers for the 4 vectors in this batch
                float* s0 = s + (i * 4);
                float* s1 = s + ((i + 1) * 4);
                float* s2 = s + ((i + 2) * 4);
                float* s3 = s + ((i + 3) * 4);

                // Transform each of the 4 vectors using FMA
                TransformSingle(s0, d + (i * 4), col1, col2, col3, col4);
                TransformSingle(s1, d + ((i + 1) * 4), col1, col2, col3, col4);
                TransformSingle(s2, d + ((i + 2) * 4), col1, col2, col3, col4);
                TransformSingle(s3, d + ((i + 3) * 4), col1, col2, col3, col4);
            }

            // Cleanup loop for remaining vectors
            for (; i < count; i++)
            {
                TransformSingle(s + (i * 4), d + (i * 4), col1, col2, col3, col4);
            }
        }
    }
    
    private static unsafe void TransformSingle(float* src, float* dst, 
        Vector128<float> c1, Vector128<float> c2, Vector128<float> c3, Vector128<float> c4)
    {
        // Broadcast X, Y, Z, W of the source vector
        Vector128<float> x = Vector128.Create(src[0]);
        Vector128<float> y = Vector128.Create(src[1]);
        Vector128<float> z = Vector128.Create(src[2]);
        Vector128<float> w = Vector128.Create(src[3]);

        // Result = x*col1 + y*col2 + z*col3 + w*col4
        // Using Fused Multiply-Add (Fma.MultiplyAdd)
        Vector128<float> res = Fma.MultiplyAdd(x, c1, 
            Fma.MultiplyAdd(y, c2, 
                Fma.MultiplyAdd(z, c3, 
                    Vector128.Multiply(w, c4))));

        Sse.Store(dst, res);
    }
    
    
    // -------------------------------------- alternative
    // prompt:   how to implement a multiplication vector4[] arrays and a Matrix4x4 using loop unroll: 4 in C# using avx
    public static unsafe void MultiplyUnrolled4(Vector4[] inputs, Matrix4x4 matrix, Vector4[] results)
    {
        if (inputs.Length != results.Length) throw new ArgumentException("Array lengths must match.");
        
        fixed (Vector4* pInput = inputs)
        fixed (Vector4* pResult = results)
        {
            float* fIn = (float*)pInput;
            float* fOut = (float*)pResult;

            // Load Matrix columns into 256-bit registers (each column duplicated)
            // [Col0.x, Col0.y, Col0.z, Col0.w, Col0.x, Col0.y, Col0.z, Col0.w]
            Vector256<float> col0 = Vector256.Create(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M11, matrix.M12, matrix.M13, matrix.M14);
            Vector256<float> col1 = Vector256.Create(matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M21, matrix.M22, matrix.M23, matrix.M24);
            Vector256<float> col2 = Vector256.Create(matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M31, matrix.M32, matrix.M33, matrix.M34);
            Vector256<float> col3 = Vector256.Create(matrix.M41, matrix.M42, matrix.M43, matrix.M44, matrix.M41, matrix.M42, matrix.M43, matrix.M44);

            int i = 0;
            int count = inputs.Length;

            // Loop Unroll: 4 (Processing 2 vectors per YMM, so 4 * 2 = 8 vectors per loop)
            for (; i <= count - 8; i += 8)
            {
                float* input_ptr = (float*)(pInput + i);
                float* result_ptr = (float*)(pResult + i);
                
                Vector256<float> input_0 = Avx.LoadVector256(input_ptr + 0);
                Vector256<float> input_1 = Avx.LoadVector256(input_ptr + 8);
                Vector256<float> input_2 = Avx.LoadVector256(input_ptr + 16);
                Vector256<float> input_3 = Avx.LoadVector256(input_ptr + 24);
                
                // Process 4 pairs of vectors
                var res_0 = ProcessPair(input_0, col0, col1, col2, col3);    // fOut + (i + 0) * 4, 
                var res_1 = ProcessPair(input_1, col0, col1, col2, col3);    // fOut + (i + 2) * 4, 
                var res_2 = ProcessPair(input_2, col0, col1, col2, col3);    // fOut + (i + 4) * 4, 
                var res_3 = ProcessPair(input_3, col0, col1, col2, col3);    // fOut + (i + 6) * 4, 
                
                Avx.Store(result_ptr + 0, res_0);
                Avx.Store(result_ptr + 8, res_1);
                Avx.Store(result_ptr + 16, res_2);
                Avx.Store(result_ptr + 24, res_3);
            }

            // Remainder loop for vectors not covered by unroll
            for (; i < count; i++)
            {
                results[i] = Vector4.Transform(inputs[i], matrix);
            }
        }
    }

    private static unsafe Vector256<float> ProcessPair(Vector256<float> v, // float* inputPtr, float* outputPtr, 
        Vector256<float> c0, Vector256<float> c1, Vector256<float> c2, Vector256<float> c3)
    {
        // Load two Vector4s into one Vector256
        // Vector256<float> v = Avx.LoadVector256(inputPtr);

        // Shuffle/Broadcast components
        // We use Avx.Shuffle to pick x,y,z,w and broadcast them across the 4 lanes for each vector
        Vector256<float> xxxx = Avx.Shuffle(v, v, 0b00_00_00_00); 
        Vector256<float> yyyy = Avx.Shuffle(v, v, 0b01_01_01_01);
        Vector256<float> zzzz = Avx.Shuffle(v, v, 0b10_10_10_10);
        Vector256<float> wwww = Avx.Shuffle(v, v, 0b11_11_11_11);

        // Standard Dot Product math: (x * col0) + (y * col1) + (z * col2) + (w * col3)
        // Vector256<float> res = Avx.Multiply(xxxx, c0);
        // res = Avx.Add(res, Avx.Multiply(yyyy, c1));
        // res = Avx.Add(res, Avx.Multiply(zzzz, c2));
        // res = Avx.Add(res, Avx.Multiply(wwww, c3));
        
        // Vector256<float> res = Avx.Multiply(xxxx, c0);
        // res = Fma.MultiplyAdd(yyyy, c1, res);
        // res = Fma.MultiplyAdd(zzzz, c2, res);
        // res = Fma.MultiplyAdd(wwww, c3, res);
        
        Vector256<float> res = Fma.MultiplyAdd(wwww, c3, Fma.MultiplyAdd(zzzz, c2, Fma.MultiplyAdd(yyyy, c1, Avx.Multiply(xxxx, c0))));

        return res;
        // Store the two resulting Vector4s
        // Avx.Store(outputPtr, res);
    }
}


using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Examples;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Vectorize;


public static class Test_Vectorize_Lab
{
    // [Query]
    [Vectorize]
    public static void MoveVectorized(ref Position position, in Velocity velocity, float deltaTime) {
        position.value += velocity.value * deltaTime;
    }
    
#region template for code generation
    public static ArchetypeQuery MoveVectorizedQuery(EntityStore store, float deltaTime)
    {
        var query = MoveVectorized_GetQuery(store);
        foreach (var chunk in query.Chunks)
        {
            var entities = chunk.Entities;
            var positionSpan = chunk.Chunk1.Span;
            var velocitySpan = chunk.Chunk2.Span;
            for (int n = 0; n < entities.Length; n++) {
                MoveVectorized(ref positionSpan[n], velocitySpan[n], deltaTime);
            }
        }
        return query;
    }
    
    private static readonly int MoveVectorized_Slot = EntityStore.UserDataNewSlot();
    
    private static ArchetypeQuery<Position, Velocity> MoveVectorized_GetQuery(EntityStore store)
    {
        var query = (ArchetypeQuery<Position, Velocity>)EntityStore.UserDataGet(store, MoveVectorized_Slot);
        if (query != null) {
            return query;
        }
        query = store.Query<Position, Velocity>();
        
        EntityStore.UserDataSet(store, MoveVectorized_Slot, query);
        return query;
    }
    #endregion  
    
    [Test]
    public static void Test_Vectorize_Lab_Run()
    {
        var store = new EntityStore();
        for (int n = 0; n < 1000; n++) {
            store.CreateEntity(new Position { x = n, y = 2, z = 3 }, new Velocity() { value = new Vector3(1,0,0 )});
        }
        MoveVectorizedQuery(store, 0.1f);
    }
    
    [Test]
    public static unsafe void Test_Vectorize_Unzip_Shuffle()
    {
        var input = new Vector3[] {
            new ( 1, 2, 3), new (11,12,13), new (21,22,23), new (31,32,33),
            new (41,42,43), new (51,52,53), new (61,62,63), new (71,72,73)
        };
        var result = new Vector3[8];
        fixed (Vector3* p =    input) 
        fixed (Vector3* pOut = result) {
            var (vx, vy, vz) = Transpose8Vector3((float*)p);
            StoreSoAtoAoS(vx, vy, vz, (float*)pOut);
        }
        Assert.AreEqual(input, result);
    }
    
    // Prompt:  Transpose: Vector3[8]  ->  tuple of three Vector256<float> using shuffle in C# with maximum performance
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe (Vector256<float> X, Vector256<float> Y, Vector256<float> Z) Transpose8Vector3(float* inputPtr)
    {
        // 1. Load 3 contiguous 256-bit blocks
        Vector256<float> r0 = Avx.LoadVector256(inputPtr);      // [X0 Y0 Z0 X1 Y1 Z1 X2 Y2]
        Vector256<float> r1 = Avx.LoadVector256(inputPtr + 8);  // [Z2 X3 Y3 Z3 X4 Y4 Z4 X5]
        Vector256<float> r2 = Avx.LoadVector256(inputPtr + 16); // [Y5 Z5 X6 Y6 Z6 X7 Y7 Z7]

        // 2. Extract X components: [X0 X1 X2 X3 X4 X5 X6 X7]
        // Indices: r0[0,3,6], r1[1,4,7], r2[2,5]
        Vector256<float> x012   = Avx2.PermuteVar8x32(r0, Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0));
        Vector256<float> x345   = Avx2.PermuteVar8x32(r1, Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0));
        Vector256<float> x67    = Avx2.PermuteVar8x32(r2, Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5));
        
        // Combine using Blends (Blends are faster than Ors)
        Vector256<float> vx = Avx.Blend(x012, x345, 0b00111000); // 0, 3, 6 from r0; 1, 4, 7 from r1
        vx = Avx.Blend(vx, x67, 0b11000000);                    // 2, 5 from r2

        // 3. Extract Y components: [Y0 Y1 Y2 Y3 Y4 Y5 Y6 Y7]
        // Indices: r0[1,4,7], r1[2,5], r2[0,3,6]
        Vector256<float> y012   = Avx2.PermuteVar8x32(r0, Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0));
        Vector256<float> y345   = Avx2.PermuteVar8x32(r1, Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0));
        Vector256<float> y567   = Avx2.PermuteVar8x32(r2, Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6));
        // Y5 is actually at r2[0]
        Vector256<float> y5_fix = Avx2.PermuteVar8x32(r2, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));

        Vector256<float> vy = Avx.Blend(y012, y345, 0b00011000); 
        vy = Avx.Blend(vy, y5_fix, 0b00100000);
        vy = Avx.Blend(vy, y567, 0b11000000);

        // 4. Extract Z components: [Z0 Z1 Z2 Z3 Z4 Z5 Z6 Z7]
        // Indices: r0[2,5], r1[0,3,6], r2[1,4,7]
        Vector256<float> z01    = Avx2.PermuteVar8x32(r0, Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0));
        Vector256<float> z234   = Avx2.PermuteVar8x32(r1, Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0));
        // Z2 is at r1[0]
        Vector256<float> z2_fix = Avx2.PermuteVar8x32(r1, Vector256.Create(0, 0, 0, 0, 0, 0, 0, 0));
        Vector256<float> z567   = Avx2.PermuteVar8x32(r2, Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7));

        Vector256<float> vz = Avx.Blend(z01, z2_fix, 0b00000100);
        vz = Avx.Blend(vz, z234, 0b00011000);
        vz = Avx.Blend(vz, z567, 0b11100000);
        return (vx, vy, vz);
    }
    
    // Prompt:   Complete method to transpose 3 Vector256<float> V to a Vector3[8] array with a float* parameter using Unpacks and Blends in C# with maximum performance
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreSoAtoAoS (Vector256<float> x, Vector256<float> y, Vector256<float> z, float* ptr)
    {
        float* f = ptr;
    
        // For storing, the JIT-optimized scalar scatter is currently 
        // the fastest on .NET 8/9 for AVX2.
        f[0] = x.GetElement(0); f[1] = y.GetElement(0); f[2] = z.GetElement(0);
        f[3] = x.GetElement(1); f[4] = y.GetElement(1); f[5] = z.GetElement(1);
        f[6] = x.GetElement(2); f[7] = y.GetElement(2); f[8] = z.GetElement(2);
        f[9] = x.GetElement(3); f[10] = y.GetElement(3); f[11] = z.GetElement(3);
        f[12] = x.GetElement(4); f[13] = y.GetElement(4); f[14] = z.GetElement(4);
        f[15] = x.GetElement(5); f[16] = y.GetElement(5); f[17] = z.GetElement(5);
        f[18] = x.GetElement(6); f[19] = y.GetElement(6); f[20] = z.GetElement(6);
        f[21] = x.GetElement(7); f[22] = y.GetElement(7); f[23] = z.GetElement(7);
    }
    
    private static (Vector3[] position, Vector3[] velocity) CreateTestData()
    {
        var position = new Vector3[1024];
        var velocity = new Vector3[1024];
        for (int n = 0; n < position.Length; n++) {
            position[n] = new Vector3(n, n, n);
            velocity[n] = new Vector3(2, 2, 2);
        }
        return (position, velocity);
    }

    private const int repeatCount = 10; // 100_000;
    
    [Test]
    public static void Test_Vectorize_Multiply_perf()
    {
        var (position, velocity) = CreateTestData();
        for (int n = 0; n < repeatCount; n++) {
            MultiplyVectorized(position, velocity);
        }
    }
    
    [Test]
    public static void Test_Vectorize_Multiply_perf_idiomatic()
    {
        var (position, velocity) = CreateTestData();
        for (int n = 0; n < repeatCount; n++) {
            MultiplyIdiomatic(position, velocity);
        }
    }
    
    [Test]
    public static void Test_Vectorize_Multiply_validate()
    {
        var (position1, velocity1) = CreateTestData();
        var (position2, velocity2) = CreateTestData();
        
        MultiplyIdiomatic (position1, velocity1);
        MultiplyVectorized(position2, velocity2);
        
        Assert.AreEqual(position1, position2);
    }
    
    private static unsafe void MultiplyVectorized(Vector3[] position, Vector3[] velocity)
    {
        fixed (Vector3* positionPtr =    position) 
        fixed (Vector3* velocityPtr =    velocity)
        {
            for (int i = 0; i < 1024; i += 8) {
                var (positionX, positionY, positionZ) = Transpose8Vector3((float*)(positionPtr + i));
                var (velocityX, velocityY, velocityZ) = Transpose8Vector3((float*)(velocityPtr + i));
                positionX *= velocityX;
                positionY *= velocityY;
                positionZ *= velocityZ;
                StoreSoAtoAoS(positionX, positionY, positionZ, (float*)(positionPtr + i));
            }
        }
    }
    
    private static void MultiplyIdiomatic(Vector3[] position, Vector3[] velocity)
    {
        for (int i = 0; i < 1024; i++) {
            position[i] *= velocity[i];
        }
    }
}


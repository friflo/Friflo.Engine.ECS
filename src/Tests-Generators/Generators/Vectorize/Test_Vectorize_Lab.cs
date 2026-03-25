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
    }
    
    /// Prompt:  Transpose: Vector3[8]  ->  tuple of three Vector256&lt;float> using shuffle
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe (Vector256<float> X, Vector256<float> Y, Vector256<float> Z) Transpose8Vector3(float* inputPtr)
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreSoAtoAoS (
        Vector256<float> vx, 
        Vector256<float> vy, 
        Vector256<float> vz, 
        float* destination)
    {
        // 1. Interleave X and Y 
        // lowXY:  [X0 Y0 X1 Y1 | X4 Y4 X5 Y5]
        // highXY: [X2 Y2 X3 Y3 | X6 Y6 X7 Y7]
        Vector256<float> lowXY = Avx.UnpackLow(vx, vy);
        Vector256<float> highXY = Avx.UnpackHigh(vx, vy);

        // 2. Generate the three final registers via Shuffles and Blends
        // This requires specific mapping to handle the 12-byte stride of Vector3.
        
        // Target Register 0: [X0 Y0 Z0 X1 Y1 Z1 X2 Y2]
        // We take X0, Y0 from lowXY, Z0 from vz, X1, Y1 from lowXY, Z1 from vz...
        Vector256<float> r0 = Avx2.PermuteVar8x32(
            Avx2.Blend(lowXY, vz, 0b00100100), // Mixes X0Y0, X1Y1 with Z0, Z1
            Vector256.Create(0, 1, 8, 2, 3, 9, 4, 5) // Indices to align for R0
        );

        // Target Register 1: [Z2 X3 Y3 Z3 X4 Y4 Z4 X5]
        Vector256<float> r1 = Avx2.PermuteVar8x32(
            Avx2.Blend(highXY, vz, 0b10010010), 
            Vector256.Create(2, 0, 1, 3, 4, 5, 6, 7) // Indices to align for R1
        );

        // Target Register 2: [Y5 Z5 X6 Y6 Z6 X7 Y7 Z7]
        Vector256<float> r2 = Avx2.PermuteVar8x32(
            Avx2.Blend(highXY, vz, 0b01001001),
            Vector256.Create(1, 2, 3, 4, 5, 6, 7, 0) // Indices to align for R2
        );

        // 3. Store to memory
        // Total 24 floats = 96 bytes.
        Avx.Store(destination, r0);
        Avx.Store(destination + 8, r1);
        Avx.Store(destination + 16, r2);
    }
}


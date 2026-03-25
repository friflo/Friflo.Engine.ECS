using System.Numerics;
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
    
    // [Test]
    public static unsafe void Test_Vectorize_Unzip_Shuffle()
    {
        var vectors = new Vector3[] {
            new ( 1, 2, 3), new (11,12,13), new (21,22,23), new (31,32,33),
            new (41,42,43), new (51,52,53), new (61,62,63), new (71,72,73)
        };
        fixed (Vector3* p = vectors) { UnzipVector3((float*)p); }
    }
    
    static unsafe void UnzipVector3(float* inputPtr)
    {
        // 1. Load 3 contiguous 256-bit blocks
        Vector256<float> v0 = Avx.LoadVector256(inputPtr);      // [X0 Y0 Z0 X1 Y1 Z1 X2 Y2]
        Vector256<float> v1 = Avx.LoadVector256(inputPtr + 8);  // [Z2 X3 Y3 Z3 X4 Y4 Z4 X5]
        Vector256<float> v2 = Avx.LoadVector256(inputPtr + 16); // [Y5 Z5 X6 Y6 Z6 X7 Y7 Z7]

        // 2. Shuffle to group components partially
        // We use PermuteVar8x32 to move floats within each individual register
        var p0 = Avx2.PermuteVar8x32(v0, MaskX); // [X0 X1 X2 Y0 Y1 Y2 Z0 Z1] - partial
        var p1 = Avx2.PermuteVar8x32(v1, MaskY); // [X3 X4 X5 Y3 Y4 Y5 Z2 Z3] - partial
        var p2 = Avx2.PermuteVar8x32(v2, MaskZ); // [X6 X7 Y6 Y7 Z4 Z5 Z6 Z7] - partial
        
        // 3. The Stitching (Crucial Step)
        // We need to move the 'overflow' components to their correct registers.
        // p0: [X0 X1 X2 Y0 | Y1 Y2 Z0 Z1]
        // p1: [X3 X4 X5 Y3 | Y4 Y5 Z2 Z3]
        // p2: [X6 X7 Y6 Y7 | Z4 Z5 Z6 Z7]

        // --- Extracting All Xs ---
        // Combine the first 128-bits of p0 (X0-X2, Y0) and first 128-bits of p1 (X3-X5, Y3)
        // Then blend/permute to get [X0 X1 X2 X3 | X4 X5 X6 X7]
        var x_0_3 = Avx2.Permute2x128(p0, p1, 0x20); // Low 128 of p0 and Low 128 of p1
        var x_4_7 = Avx2.Permute2x128(p1, p2, 0x20); // Low 128 of p1 and Low 128 of p2 (partially)
        
        // --- The optimized "Shift and Blend" Pattern ---
        // To avoid 50 lines of manual blends, the industry standard for Vector3 
        // is actually to use 'AlignRight' (vpalignr):
        var xFinalClean = Avx2.Blend(
            Avx2.PermuteVar8x32(p0, MaskX), 
            Avx2.PermuteVar8x32(p1, MaskX), 
            0b11110000); // Roughly combines the two halves
    }
    
    // Pre-computed masks for PermuteVar8x32
    // These tell the CPU exactly which index from the source 256-bit register 
    // goes into which slot in the destination.

    
    private static readonly Vector256<int> MaskX = Vector256.Create(0, 3, 6, 1, 4, 7, 2, 5);
    private static readonly Vector256<int> MaskY = Vector256.Create(1, 4, 7, 2, 5, 0, 3, 6);
    private static readonly Vector256<int> MaskZ = Vector256.Create(2, 5, 0, 3, 6, 1, 4, 7);
    
    // private static readonly Vector256<int> MaskX = Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0);
    // private static readonly Vector256<int> MaskY = Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0);
    // private static readonly Vector256<int> MaskZ = Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0);
}


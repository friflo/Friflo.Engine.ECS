// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

#region --- T1 T2

public readonly struct QueryChunks<T1, T2>  // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    readonly ArchetypeQuery<T1, T2> query;

    public  override string         ToString() => query.structIndexes.GetString("Chunks: ");

    internal QueryChunks(ArchetypeQuery<T1, T2> query) {
        this.query = query;
    }
    
    public ChunkEnumerator<T1,T2> GetEnumerator() => new (query);
}

public ref struct ChunkEnumerator<T1, T2>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    private readonly    ReadOnlySpan<Archetype> archetypes;
    private             int                     archetypePos;
    private readonly    int                     structIndex1;
    private readonly    int                     structIndex2;
    private             StructChunk<T1>[]       chunks1;
    private             StructChunk<T2>[]       chunks2;
    private             Chunk<T1>               chunk1;
    private             Chunk<T2>               chunk2;
    private             int                     chunkPos;
    private             int                     chunkCount;
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1, T2> query)
    {
        archetypes      = query.Archetypes;
        archetypePos    = 0;
        structIndex1    = query.structIndexes.T1;
        structIndex2    = query.structIndexes.T2;
        var archetype   = archetypes[0];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
        chunkPos        = -1;
        chunkCount      = archetype.EntityCount / ChunkSize;
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public (Chunk<T1>, Chunk<T2>) Current   => (chunk1, chunk2);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (chunkPos < chunkCount) {
            chunkPos++;
            chunk1 = new Chunk<T1>(chunks1[chunkPos].components, 1);
            chunk2 = new Chunk<T2>(chunks2[chunkPos].components, 1);
            return true;
        }
        if (archetypePos < archetypes.Length -1) {
            var heapMap = archetypes[archetypePos++].heapMap;
            chunks1     = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
            chunks2     = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
            chunkPos    = 0;
            return true;
        }
        return false;  
    }
}
#endregion


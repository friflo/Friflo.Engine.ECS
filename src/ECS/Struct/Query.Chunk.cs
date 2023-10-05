// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public ref struct ChunkEnumerator<T1, T2>
    where T1 : struct
    where T2 : struct
{
    private             ReadOnlySpan<Archetype> archetypes;
    private             StructHeap<T1>          heap1;
    private             StructHeap<T2>          heap2;
    private             Chunk<T1>               chunk1;
    private             Chunk<T2>               chunk2;
    private readonly    int                     structIndex1;
    private readonly    int                     structIndex2;
    private             int                     chunkPos;
    private             int                     archPos;
    private             int                     chunkCount;
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1, T2> query)
    {
        chunkPos        = -1;
        archPos         = 0;
        var indices     = query.structIndices;
        structIndex1    = indices[0];
        structIndex2    = indices[1];
        archetypes      = query.Archetypes;
        var archetype   = archetypes[0];
        chunkCount      = archetype.EntityCount / ChunkSize;
        var heapMap     = archetype.heapMap;
        heap1           = (StructHeap<T1>)heapMap[structIndex1];
        heap2           = (StructHeap<T2>)heapMap[structIndex2];
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public (Chunk<T1>, Chunk<T2>) Current   => (chunk1, chunk2);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (chunkPos < chunkCount) {
            chunkPos++;
            chunk1 = new Chunk<T1>(heap1.chunks[chunkPos].components, 1);
            chunk2 = new Chunk<T2>(heap2.chunks[chunkPos].components, 1);
            return true;
        }
        if (archPos < archetypes.Length -1) {
            archPos++;
            var archetype   = archetypes[archPos];
            var heapMap     = archetype.heapMap;
            heap1           = (StructHeap<T1>)heapMap[structIndex1];
            heap2           = (StructHeap<T2>)heapMap[structIndex2];
            chunkPos = 0;
            return true;
        }
        return false;  
    }
}

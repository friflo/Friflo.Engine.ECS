// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public ref struct QueryEnumerator<T1, T2>
    where T1 : struct
    where T2 : struct
{
    private StructChunk<T1>[]       chunks1;
    private StructChunk<T2>[]       chunks2;
    private int                     structIndex1;
    private int                     structIndex2;
    private int                     pos;
    private int                     count;
    
    private ReadOnlySpan<Archetype> archetypes;
    
    internal  QueryEnumerator(ArchetypeQuery<T1, T2> query)
    {
        pos             = -1;
        var indices     = query.structIndices;
        structIndex1    = indices[0];
        structIndex2    = indices[1];
        var archetype   = query.Archetypes[0];
        count           = archetype.EntityCount - 1;
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public (T1, T2) Current   => (chunks1[pos / ChunkSize].components[pos % ChunkSize], 
                                  chunks2[pos / ChunkSize].components[pos % ChunkSize]);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (pos < count) {
            pos++;
            return true;
        }
        return false;  
    }
}

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

    private readonly    int         structIndex1;
    private readonly    int         structIndex2;
    
    private     StructChunk<T1>[]   chunks1;
    private     StructChunk<T2>[]   chunks2;
    private             int         chunkPos;
    private             int         chunkLen;
    
    private             int         componentLen;
    private             T1[]        components1;
    private             T2[]        components2;
    private             int         componentPos;
    
    private int                     archetypePos;
    private ReadOnlySpan<Archetype> archetypes;
    
    internal  QueryEnumerator(ArchetypeQuery<T1, T2> query)
    {
        var indices     = query.structIndices;
        structIndex1    = indices[0];
        structIndex2    = indices[1];
        archetypePos    = 0;
        archetypes      = query.Archetypes;
        var archetype   = archetypes[0];
        var heapMap     = archetype.heapMap;
        chunkLen        = 1;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
        components1     = chunks1[0].components;
        components2     = chunks2[0].components;
        componentPos    = -1;
        componentLen    = archetype.EntityCount - 1;
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public (T1, T2) Current   => (components1[componentPos], components2[componentPos]);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (componentPos < componentLen) {
            componentPos++;
            return true;
        }
        if (chunkPos < chunks1.Length - 1) {
            components1     = chunks1[chunkPos].components;
            components2     = chunks2[chunkPos].components;
            chunkPos++;
            return true;
        }
        if (archetypePos < archetypes.Length - 1) {
            var archetype   = archetypes[++archetypePos];
            var heapMap     = archetype.heapMap;
            chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
            chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
            chunkPos        = 0;
            components1     = chunks1[0].components;
            components2     = chunks2[0].components;
            componentPos    = 0;
            return true;
        }
        return false;  
    }
}

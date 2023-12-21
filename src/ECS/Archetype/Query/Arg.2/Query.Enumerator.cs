// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

#if COMP_ITER

using System;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public ref struct QueryEnumerator<T1, T2>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    private readonly    T1[]                    copyT1;
    private readonly    T2[]                    copyT2;
    
    private  readonly   int                     structIndex1;
    private  readonly   int                     structIndex2;

    private  readonly   ReadOnlySpan<Archetype> archetypes;
    private             int                     archetypePos;

    private             StructChunk<T1>[]       chunks1;
    private             StructChunk<T2>[]       chunks2;
    private             int                     chunkPos;
    private  readonly   int                     chunkEnd;
        
    private             Ref<T1>                 ref1;   // its .pos is used as loop condition in MoveNext()
    private             Ref<T2>                 ref2;
    private             int                     componentLen;
    
    internal QueryEnumerator(ArchetypeQuery<T1, T2> query)
    {
        copyT1          = query.copyT1;
        copyT2          = query.copyT2;
        structIndex1    = query.signatureIndexes.T1;
        structIndex2    = query.signatureIndexes.T2;
        archetypes      = query.GetArchetypes();
        archetypePos    = 0;
        var archetype   = archetypes[0];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
        chunkEnd        = archetype.ChunkCount() - 1;
        
        componentLen    = Math.Min(archetype.EntityCount, ChunkSize) - 1;
        ref1.Set(chunks1[0].components, copyT1, componentLen);
        ref1.pos        = -1;
        ref2.Set(chunks2[0].components, copyT2, componentLen);
        ref2.pos        = -1;
    }
    
    /// <summary>
    /// return each component using a <see cref="Ref{T}"/> to avoid struct copy and enable mutation in library
    /// </summary>
    public readonly (Ref<T1>, Ref<T2>) Current   => (ref1, ref2);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (ref1.pos < componentLen) {
            ref1.pos++;
            ref2.pos++;
            return true;
        }
        if (chunkPos < chunkEnd) {
            goto Next;
        }
        if (chunkPos == chunkEnd && chunkPos >= 0) {
            componentLen = archetypes[archetypePos].ChunkRest() - 1;
            if (componentLen >= 0) {
                goto Next;
            }
        }
        if (archetypePos >= archetypes.Length - 1) {
            return false;
        }
        var archetype   = archetypes[++archetypePos];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
        chunkPos        = -1;
        componentLen    = Math.Min(archetype.EntityCount, ChunkSize) - 1;
    Next:
        chunkPos++;
        ref1.Set(chunks1[chunkPos].components, copyT1, componentLen);
        ref1.pos = 0;
        ref2.Set(chunks2[chunkPos].components, copyT2, componentLen);
        ref2.pos = 0;
        return true;
    }
}

#endif
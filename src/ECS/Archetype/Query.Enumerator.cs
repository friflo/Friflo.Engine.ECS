// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

#region --- none

public ref struct QueryEnumerator
{
    private             int                     entityPos;
    private             int                     componentLen;
    private             int[]                   entityIds;
    
    private  readonly   ReadOnlySpan<Archetype> archetypes;
    private             int                     archetypePos;
    
    internal QueryEnumerator(ArchetypeQuery query)
    {
        archetypes      = query.Archetypes;
        archetypePos    = 0;
        var archetype   = archetypes[0];
        entityIds       = archetype.entityIds;
        entityPos       = -1;
        componentLen    = archetype.EntityCount - 1;
    }
    
    /// <summary>
    /// return each component using a <see cref="Ref{T}"/> to avoid struct copy and enable mutation in library
    /// </summary>
    public int Current   => entityIds[entityPos];
    
    // --- IEnumerator
    public bool MoveNext() {
        if (entityPos < componentLen) {
            entityPos++;
            return true;
        }
        if (archetypePos < archetypes.Length - 1) {
            var archetype   = archetypes[++archetypePos];
            entityPos       = 0;
            entityIds       = archetype.entityIds;
            componentLen    = archetype.EntityCount - 1;
            return true;
        }
        return false;  
    }
}
#endregion

#region --- T1 T2

public ref struct QueryEnumerator<T1, T2>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    private  readonly   int                     structIndex1;
    private  readonly   int                     structIndex2;
    
    private             StructChunk<T1>[]       chunks1;
    private             StructChunk<T2>[]       chunks2;
    private             int                     chunkPos;
    private  readonly   int                     chunkEnd;
        
    private             Ref<T1>                 ref1;   // its .pos is used as loop condition in MoveNext()
    private             Ref<T2>                 ref2;
    private             int                     componentLen;
    
    private  readonly   ReadOnlySpan<Archetype> archetypes;
    private             int                     archetypePos;
    
    internal QueryEnumerator(ArchetypeQuery<T1, T2> query)
    {
        structIndex1    = query.signatureIndexes.T1;
        structIndex2    = query.signatureIndexes.T2;
        archetypes      = query.Archetypes;
        archetypePos    = 0;
        var archetype   = archetypes[0];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
        chunkEnd        = archetype.ChunkEnd;
        
        ref1.Set(chunks1[0].components);
        ref1.pos        = -1;
        ref2.Set(chunks2[0].components);
        ref2.pos        = -1;
        componentLen    = Math.Min(archetype.EntityCount, StructUtils.ChunkSize) - 1;
    }
    
    /// <summary>
    /// return each component using a <see cref="Ref{T}"/> to avoid struct copy and enable mutation in library
    /// </summary>
    public (Ref<T1>, Ref<T2>) Current   => (ref1, ref2);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (ref1.pos < componentLen) {
            ref1.pos++;
            ref2.pos++;
            return true;
        }
        if (chunkPos < chunkEnd) {
            if (++chunkPos == chunkEnd) {
                var archetype   = archetypes[archetypePos];
                componentLen    = (archetype.EntityCount % StructUtils.ChunkSize) - 1;
            }
            ref1.Set(chunks1[chunkPos].components);
            ref1.pos = 0;
            ref2.Set(chunks2[chunkPos].components);
            ref2.pos = 0;
            return true;
        }
        if (archetypePos < archetypes.Length - 1) {
            var archetype   = archetypes[++archetypePos];
            var heapMap     = archetype.heapMap;
            chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
            chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
            chunkPos        = 0;
            ref1.Set(chunks1[0].components);
            ref1.pos = 0;
            ref2.Set(chunks2[0].components);
            ref2.pos = 0;
            componentLen    = Math.Min(archetype.EntityCount, StructUtils.ChunkSize) - 1;
            return true;
        }
        return false;  
    }
}
#endregion
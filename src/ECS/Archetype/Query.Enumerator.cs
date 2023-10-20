// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Fliox.Engine.ECS.StructUtils;

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
    public readonly int Current   => entityIds[entityPos];
    
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
        chunkEnd        = archetype.ChunkEnd - 1;
        
        componentLen    = Math.Min(archetype.EntityCount, ChunkSize) - 1;
        ref1.Set(chunks1[0].components);
        ref1.pos        = -1;
        ref2.Set(chunks2[0].components);
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
        if (chunkPos == chunkEnd && chunkPos > -1) {
            componentLen = archetypes[archetypePos].ChunkRest - 1;
            EnumeratorUtils.AssertComponentLenGreater0(componentLen);
            goto Next;
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
        ref1.Set(chunks1[chunkPos].components);
        ref1.pos = 0;
        ref2.Set(chunks2[chunkPos].components);
        ref2.pos = 0;
        return true;
    }
}
#endregion

internal static class EnumeratorUtils
{
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    internal static void AssertComponentLenGreater0 (int componentLen) {
        if (componentLen <= 0) throw new InvalidOperationException("expect componentLen > 0");
    }
}
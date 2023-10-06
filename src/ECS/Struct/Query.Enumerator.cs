// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Enables access to a struct component by reference using its property <see cref="Value"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Ref<T> where T : struct
{
    /// <summary>
    /// Returns a mutable struct component value by reference.<br/>
    /// <see cref="Value"/> modifications are instantaneously available via <see cref="GameEntity.GetComponentValue{T}"/>  
    /// </summary>
    public          ref T       Value => ref components[pos];
    
    internal            T[]     components;
    internal            int     pos;

    public  override    string  ToString() => Value.ToString();
}

public ref struct QueryEnumerator<T1, T2>
    where T1 : struct
    where T2 : struct
{

    private readonly    int                 structIndex1;
    private readonly    int                 structIndex2;
    
    private             StructChunk<T1>[]   chunks1;
    private             StructChunk<T2>[]   chunks2;
    private             int                 chunkPos;
    private             int                 chunkLen;
    
    private             int                 componentLen;
    private             Ref<T1>             ref1;   // its .pos is used as loop condition in MoveNext()
    private             Ref<T2>             ref2;
    
    private             int                 archetypePos;
    private ReadOnlySpan<Archetype>         archetypes;
    
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
        ref1.components = chunks1[0].components;
        ref2.components = chunks2[0].components;
        ref1.pos        = -1;
        ref2.pos        = -1;
        componentLen    = archetype.EntityCount - 1;
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
        if (chunkPos < chunks1.Length - 1) {
            ref1.components = chunks1[chunkPos].components;
            ref1.pos = 0;
            ref2.components = chunks2[chunkPos].components;
            ref2.pos = 0;
            chunkPos++;
            return true;
        }
        if (archetypePos < archetypes.Length - 1) {
            var archetype   = archetypes[++archetypePos];
            var heapMap     = archetype.heapMap;
            chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
            chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
            chunkPos        = 0;
            ref1.components = chunks1[0].components;
            ref1.pos        = 0;
            ref2.components = chunks2[0].components;
            ref2.pos        = 0;
            return true;
        }
        return false;  
    }
}

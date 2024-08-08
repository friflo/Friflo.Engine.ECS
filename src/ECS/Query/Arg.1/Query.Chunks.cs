// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Contains the components returned by a component query.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#enumerate-query-chunks">Example.</a>
/// </summary>
public readonly struct Chunks<T1>
    where T1 : struct, IComponent
{
    public              int             Length => Chunk1.Length;
    public readonly     Chunk<T1>       Chunk1;     //  16
    public readonly     ChunkEntities   Entities;   //  32

    public override     string          ToString() => Entities.GetChunksString();

    internal Chunks(Chunk<T1> chunk1, in ChunkEntities entities) {
        Chunk1     = chunk1;
        Entities   = entities;
    }
    
    internal Chunks(in Chunks<T1> chunks, int start, int length, int taskIndex) {
        Chunk1      = new Chunk<T1>    (chunks.Chunk1,   start, length);
        Entities    = new ChunkEntities(chunks.Entities, start, length, taskIndex);
    }
    
    internal Chunks(in ChunkEntities entities, int taskIndex) {
        Entities   = new ChunkEntities(entities, taskIndex);
    }
    
    public void Deconstruct(out Chunk<T1> chunk1, out ChunkEntities entities) {
        chunk1      = Chunk1;
        entities    = Entities;
    }
}

/// <summary>
/// Contains the component chunks returned by a component query.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#enumerate-query-chunks">Example.</a>
/// </summary>
public readonly struct QueryChunks<T1>  : IEnumerable <Chunks<T1>>
    where T1 : struct, IComponent
{
    private readonly    ArchetypeQuery<T1>  query;

    public              int     Count       => query.Count;
    
    /// <summary> Obsolete. Renamed to <see cref="Count"/>. </summary>
    [Obsolete($"Renamed to {nameof(Count)}")] [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public              int     EntityCount => query.Count;
    
    public  override    string  ToString()  => query.GetQueryChunksString();

    internal QueryChunks(ArchetypeQuery<T1> query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<Chunks<T1>>
    IEnumerable<Chunks<T1>>.GetEnumerator() => new ChunkEnumerator<T1> (query);
    
    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator     IEnumerable.GetEnumerator() => new ChunkEnumerator<T1> (query);
    
    // --- IEnumerable
    public ChunkEnumerator<T1> GetEnumerator() => new (query);
}

public struct ChunkEnumerator<T1> : IEnumerator<Chunks<T1>>
    where T1 : struct, IComponent
{
    private readonly    int                     structIndex1;   //  4
    //
    private readonly    Archetypes              archetypes;     // 16
    //
    private             int                     archetypePos;   //  4
    private             Chunks<T1>              chunks;         // 40
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1> query)
    {
        structIndex1    = query.signatureIndexes.T1;
        archetypes      = query.GetArchetypes();
        archetypePos    = -1;
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly Chunks<T1> Current   => chunks;
    
    // --- IEnumerator
    [ExcludeFromCodeCoverage]
    public void Reset() {
        archetypePos    = -1;
        chunks          = default;
    }

    [ExcludeFromCodeCoverage]
    object IEnumerator.Current  => chunks;
    
    // --- IEnumerator
    public bool MoveNext()
    {
        Archetype archetype;
        int count;
        int start = 0;
        var types = archetypes;
        var pos   = archetypePos;
        if (types.chunkPositions != null) {
            goto SingleEntity;
        }
        do {
            if (pos >= types.last) {  // last = length - 1
                archetypePos = pos;
                return false;
            }
            archetype   = types.array[++pos];
            count       = archetype.entityCount;
        }
        while (count == 0); // skip archetypes without entities
    SetChunks:
        archetypePos    = pos;
        var heapMap     = archetype.heapMap;
        var chunks1     = (StructHeap<T1>)heapMap[structIndex1];
            
        var chunk1      = new Chunk<T1>(chunks1.components, count, start);
        var entities    = new ChunkEntities(archetype,      count, start);
        chunks          = new Chunks<T1>(chunk1, entities);
        return true;
    SingleEntity:
        if (pos >= types.last) {
            return false;
        }
        pos++;
        start       = types.chunkPositions[pos];
        archetype   = types.array         [pos];
        count       = 1;
        goto SetChunks;
    }
    
    // --- IDisposable
    public void Dispose() { }
}

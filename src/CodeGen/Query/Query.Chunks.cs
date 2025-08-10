using static CodeGen.Gen;

namespace CodeGen.Query;

static partial class QueryGen {
    
    public static string Query_Chunks_generator(int count)
    {
        var args = Join(count, n => $"T{n}", ",");
        
    return $$"""
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
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#enumerate-query-chunks">Example.</a>
/// </summary>
public readonly struct Chunks<{{args}}>
{{Where(count)}}
{
    public              int             Length => Chunk1.Length;
{{Join(count, n => $"    public readonly     Chunk<T{n}>       Chunk{n};     //  16", "\r\n ")}}
    public readonly     ChunkEntities   Entities;   //  32

    public override     string          ToString() => Entities.GetChunksString();

    internal Chunks({{Join(count, n => $"Chunk<T{n}> chunk{n}", ", ")}}, in ChunkEntities entities) {
        Chunk1     = chunk1;
        Chunk2     = chunk2;
        Entities   = entities;
    }
    
    internal Chunks(in Chunks<{{args}}> chunks, int start, int length, int taskIndex) {
{{Join(count, n => $"        Chunk{n}      = new Chunk<T{n}>    (chunks.Chunk{n},   start, length);", "\r\n ")}}
        Entities    = new ChunkEntities(chunks.Entities, start, length, taskIndex);
    }
    
    internal Chunks(in ChunkEntities entities, int taskIndex) {
        Entities   = new ChunkEntities(entities, taskIndex);
    }
    
    public void Deconstruct({{Join(count, n => $"out Chunk<T{n}> chunk{n}", ", ")}}, out ChunkEntities entities) {
{{Join(count, n => $"        chunk{n}      = Chunk{n};", "\r\n ")}}
        entities    = Entities;
    }
}

/// <summary>
/// Contains the component chunks returned by a component query.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#enumerate-query-chunks">Example.</a>
/// </summary>
public readonly struct QueryChunks<{{args}}> : IEnumerable <Chunks<{{args}}>>
{{Where(count)}}
{
    private readonly ArchetypeQuery<{{args}}> query;

    public              int     Count       => query.Count;
    
    /// <summary> Obsolete. Renamed to <see cref="Count"/>. </summary>
    [Obsolete($"Renamed to {nameof(Count)}")] [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public              int     EntityCount => query.Count;
    
    public  override    string  ToString()  => query.GetQueryChunksString();

    internal QueryChunks(ArchetypeQuery<{{args}}> query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<Chunks<{{args}}>>
    IEnumerable<Chunks<{{args}}>>.GetEnumerator() => new ChunkEnumerator<{{args}}> (query);
    
    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator     IEnumerable.GetEnumerator() => new ChunkEnumerator<{{args}}> (query);
    
    // --- IEnumerable
    public ChunkEnumerator<{{args}}> GetEnumerator() => new (query);
}

public struct ChunkEnumerator<{{args}}> : IEnumerator<Chunks<{{args}}>>
{{Where(count)}}
{
{{Join(count, n => $"    private readonly    int                     structIndex{n};   //  4", "\r\n")}}
    //
    private readonly    EntityStoreBase         store;          //  8
    private readonly    Archetypes              archetypes;     // 16
    //
    private             int                     archetypePos;   //  4
    private             Chunks<{{args}}>          chunks;         // 46
    
    
    internal  ChunkEnumerator(ArchetypeQuery<{{args}}> query)
    {
{{Join(count, n => $"        structIndex{n}    = query.signatureIndexes.T{n};", "\r\n")}}
        archetypes      = query.GetArchetypes();
        archetypePos    = -1;
        if (query.checkChange) {
            store = query.store;
            store.internBase.activeQueryLoops++;
        }
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly Chunks<{{args}}> Current   => chunks;
    
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
{{Join(count, n => $"        var chunks{n}     = (StructHeap<T{n}>)heapMap[structIndex{n}];", "\r\n")}}

{{Join(count, n => $"        var chunk{n}      = new Chunk<T{n}>(chunks{n}.components, count, start);", "\r\n")}}
        var entities    = new ChunkEntities(archetype,      count, start);
        chunks          = new Chunks<{{args}}>({{Join(count, n => $"chunk{n}", ", ")}}, entities);
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
    public void Dispose() {
        if (store != null) {
            store.internBase.activeQueryLoops--;
        }
    }
}

""";
    }
}

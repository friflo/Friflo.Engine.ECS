// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunks<T1>
    where T1 : struct, IComponent
{
    public              int             Length => chunk1.Length;
    public readonly     Chunk<T1>       chunk1;
    public readonly     ChunkEntities   entities;

    public override     string          ToString() => entities.GetChunksString();

    internal Chunks(Chunk<T1> chunk1, ChunkEntities entities) {
        this.chunk1     = chunk1;
        this.entities   = entities;
    }
    
    public void Deconstruct(out Chunk<T1> chunk1, out ChunkEntities entities) {
        chunk1      = this.chunk1;
        entities    = this.entities;
    }
}


public readonly struct QueryChunks<T1>  : IEnumerable <Chunks<T1>>
    where T1 : struct, IComponent
{
    private readonly ArchetypeQuery<T1> query;

    public  override string         ToString() => query.signatureIndexes.GetString("Chunks: ");

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
    private readonly    T1[]                    copyT1;         //  8
    private readonly    int                     structIndex1;   //  4
    //
    private readonly    Archetypes              archetypes;     // 16
    private             int                     archetypePos;   //  4
    private             Archetype               archetype;      //  8
    //
    private             StructChunk<T1>[]       chunks1;        //  8
    private             Chunk<T1>               chunk1;         // 16
    private             ChunkEntities           entities;       // 24
    private             int                     chunkPos;       //  4
    private             int                     chunkEnd;       //  4
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1> query)
    {
        copyT1          = query.copyT1;
        structIndex1    = query.signatureIndexes.T1;
        archetypes      = query.GetArchetypes();
        archetypePos    = 0;
        archetype       = archetypes.array[0];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunkEnd        = archetype.ChunkCount();
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly Chunks<T1> Current   => new (chunk1, entities);
    
    // --- IEnumerator
    [ExcludeFromCodeCoverage]
    public void Reset()         => throw new NotImplementedException();

    [ExcludeFromCodeCoverage]
    object IEnumerator.Current  => (chunk1, entities);
    
    // --- IEnumerator
    public bool MoveNext()
    {
        int componentLen;
        if (chunkPos < chunkEnd) {
            componentLen = ChunkSize;
            goto Next;
        }
        if (chunkPos == chunkEnd)  {
            componentLen = archetype.ChunkRest();
            if (componentLen > 0) {
                goto Next;
            }
        }
        // --- skip archetypes without entities
        do {
            if (archetypePos >= archetypes.last) {  // last = length - 1
                return false;
            }
            archetype   = archetypes.array[++archetypePos];
            chunkEnd    = archetype.ChunkEnd();
        }
        while (chunkEnd == -1);
        
        // --- set chunks of new archetype
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunkPos        = 0;
        componentLen    = chunkEnd == 0 ? archetype.ChunkRest() : ChunkSize;
    Next:
        chunk1      = new Chunk<T1>(chunks1[chunkPos].components, copyT1, componentLen, true);
        entities    = new ChunkEntities(archetype, chunkPos, componentLen);
        chunkPos++;
        return true;  
    }
    
    // --- IDisposable
    public void Dispose() { }
}

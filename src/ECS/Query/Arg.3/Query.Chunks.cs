// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunks<T1, T2, T3>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    public              int             Length => chunk1.Length;
    public readonly     Chunk<T1>       chunk1;
    public readonly     Chunk<T2>       chunk2;
    public readonly     Chunk<T3>       chunk3;
    public readonly     ChunkEntities   entities;

    public override     string          ToString() => entities.GetChunksString();

    internal Chunks(Chunk<T1> chunk1, Chunk<T2> chunk2, Chunk<T3> chunk3, ChunkEntities entities) {
        this.chunk1     = chunk1;
        this.chunk2     = chunk2;
        this.chunk3     = chunk3;
        this.entities   = entities;
    }
    
    public void Deconstruct(out Chunk<T1> chunk1, out Chunk<T2> chunk2, out Chunk<T3> chunk3, out ChunkEntities entities) {
        chunk1      = this.chunk1;
        chunk2      = this.chunk2;
        chunk3      = this.chunk3;
        entities    = this.entities;
    }
}


public readonly struct QueryChunks<T1, T2, T3>  : IEnumerable <Chunks<T1, T2, T3>>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    private readonly ArchetypeQuery<T1, T2, T3> query;

    public  override string         ToString() => query.signatureIndexes.GetString("Chunks: ");

    internal QueryChunks(ArchetypeQuery<T1, T2, T3> query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<Chunks<T1, T2, T3>>
    IEnumerable<Chunks<T1, T2, T3>>.GetEnumerator() => new ChunkEnumerator<T1, T2, T3> (query);
    
    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator     IEnumerable.GetEnumerator() => new ChunkEnumerator<T1, T2, T3> (query);
    
    // --- IEnumerable
    public ChunkEnumerator<T1, T2, T3> GetEnumerator() => new (query);
}

public struct ChunkEnumerator<T1, T2, T3> : IEnumerator<Chunks<T1, T2, T3>>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    private readonly    T1[]                    copyT1;         //  8
    private readonly    T2[]                    copyT2;         //  8
    private readonly    T3[]                    copyT3;         //  8
    private readonly    int                     structIndex1;   //  4
    private readonly    int                     structIndex2;   //  4
    private readonly    int                     structIndex3;   //  4
    //
    private readonly    Archetypes              archetypes;     // 16
    private             int                     archetypePos;   //  4
    private             Archetype               archetype;      //  8
    //
    private             StructChunk<T1>[]       chunks1;        //  8
    private             StructChunk<T2>[]       chunks2;        //  8
    private             StructChunk<T3>[]       chunks3;        //  8
    private             Chunk<T1>               chunk1;         // 16
    private             Chunk<T2>               chunk2;         // 16
    private             Chunk<T3>               chunk3;         // 16
    private             ChunkEntities           entities;       // 24
    private             int                     chunkPos;       //  4
    private             int                     chunkEnd;       //  4
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1, T2, T3> query)
    {
        copyT1          = query.copyT1;
        copyT2          = query.copyT2;
        copyT3          = query.copyT3;
        structIndex1    = query.signatureIndexes.T1;
        structIndex2    = query.signatureIndexes.T2;
        structIndex3    = query.signatureIndexes.T3;
        archetypes      = query.GetArchetypes();
        archetypePos    = 0;
        archetype       = archetypes.array[0];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
        chunks3         = ((StructHeap<T3>)heapMap[structIndex3]).chunks;
        chunkEnd        = archetype.ChunkCount();
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly Chunks<T1, T2, T3> Current   => new (chunk1, chunk2, chunk3, entities);
    
    // --- IEnumerator
    [ExcludeFromCodeCoverage]
    public void Reset()         => throw new NotImplementedException();

    [ExcludeFromCodeCoverage]
    object IEnumerator.Current  => (chunk1, chunk2, chunk3, entities);
    
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
           archetype    = archetypes.array[++archetypePos];
           chunkEnd     = archetype.ChunkEnd();
        }
        while (chunkEnd == -1);
        
        // --- set chunks of new archetype
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunks2         = ((StructHeap<T2>)heapMap[structIndex2]).chunks;
        chunks3         = ((StructHeap<T3>)heapMap[structIndex3]).chunks;
        chunkPos        = 0;
        componentLen    = chunkEnd == 0 ? archetype.ChunkRest() : ChunkSize;
    Next:
        chunk1      = new Chunk<T1>(chunks1[chunkPos].components, copyT1, componentLen, true);
        chunk2      = new Chunk<T2>(chunks2[chunkPos].components, copyT2, componentLen, true);
        chunk3      = new Chunk<T3>(chunks3[chunkPos].components, copyT3, componentLen, true);
        entities    = new ChunkEntities(archetype, chunkPos, componentLen);
        chunkPos++;
        return true;  
    }
    
    // --- IDisposable
    public void Dispose() { }
}

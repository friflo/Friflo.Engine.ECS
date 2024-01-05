// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct QueryChunksOld<T1, T2, T3>  : IEnumerable <(Chunk<T1>, Chunk<T2>, Chunk<T3>, ChunkEntities)>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    private readonly ArchetypeQuery<T1, T2, T3> query;

    public  override string         ToString() => query.signatureIndexes.GetString("Chunks: ");

    internal QueryChunksOld(ArchetypeQuery<T1, T2, T3> query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<(Chunk<T1>, Chunk<T2>, Chunk<T3>, ChunkEntities)>
    IEnumerable<(Chunk<T1>, Chunk<T2>, Chunk<T3>, ChunkEntities)>.GetEnumerator() => new ChunkEnumeratorOld<T1, T2, T3> (query);
    
    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator     IEnumerable.GetEnumerator() => new ChunkEnumeratorOld<T1, T2, T3> (query);
    
    // --- IEnumerable
    public ChunkEnumeratorOld<T1, T2, T3> GetEnumerator() => new (query);
}

public struct ChunkEnumeratorOld<T1, T2, T3> : IEnumerator<(Chunk<T1>, Chunk<T2>, Chunk<T3>, ChunkEntities)>
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
    
    
    internal  ChunkEnumeratorOld(ArchetypeQuery<T1, T2, T3> query)
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
    public readonly (Chunk<T1>, Chunk<T2>, Chunk<T3>, ChunkEntities) Current   => (chunk1, chunk2, chunk3, entities);
    
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
            componentLen = archetype.ChunkRestOld();
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
        componentLen    = chunkEnd == 0 ? archetype.ChunkRestOld() : ChunkSize;
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

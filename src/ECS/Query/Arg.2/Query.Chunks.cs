// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
public readonly struct QueryChunks<T1, T2>  : IEnumerable <(Chunk<T1>, Chunk<T2>, ChunkEntities)>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    private readonly ArchetypeQuery<T1, T2> query;

    public  override string         ToString() => query.signatureIndexes.GetString("Chunks: ");

    internal QueryChunks(ArchetypeQuery<T1, T2> query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    IEnumerator<(Chunk<T1>, Chunk<T2>, ChunkEntities)>
    IEnumerable<(Chunk<T1>, Chunk<T2>, ChunkEntities)>.GetEnumerator() => new ChunkEnumerator<T1, T2> (query);
    
    // --- IEnumerable
    IEnumerator     IEnumerable.GetEnumerator() => new ChunkEnumerator<T1, T2> (query);
    
    // --- new
    public ChunkEnumerator<T1, T2>  GetEnumerator() => new (query);
}

[ExcludeFromCodeCoverage]
public struct ChunkEnumerator<T1, T2> : IEnumerator<(Chunk<T1>, Chunk<T2>, ChunkEntities)>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    private readonly    ArchetypeQuery<T1, T2>                  query;  //  8
    private readonly    (Chunk<T1>, Chunk<T2>, ChunkEntities)[] chunks; //  8
    private readonly    int                                     last;   //  4
    private             int                                     index;  //  4
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1, T2> query)
    {
        this.query      = query;
        var chunkArrays = query.chunkArrays;
        var archetypes  = query.GetArchetypes();
        int chunkCount  = 0;
        var archs       = archetypes.array;
        
        // --- get final chunk count to reuse / create chunk array
        for (int n = 0; n < archetypes.length; n++) {
            chunkCount += archs[n].ChunkEnd() + 1;
        }
        // --- try to reuse a pooled chunk array
        while (chunkArrays.TryPop(out var chunkArray)) {
            if (chunkArray.Length < chunkCount) {
                continue;
            }
            chunks = chunkArray;
            break;
        }
        // --- create new chunk array if none was found 
        chunks ??= new (Chunk<T1>, Chunk<T2>, ChunkEntities)[chunkCount];
        
        // --- fill chunks array
        int pos = 0;
        for (int n = 0; n < archetypes.length; n++)
        {
            var archetype   = archs[n];
            int chunkEnd    = archetype.ChunkEnd();
            if (chunkEnd == -1) {
                continue;
            }
            var heapMap = archetype.heapMap;
            var chunks1 = ((StructHeap<T1>)heapMap[query.signatureIndexes.T1]).chunks;
            var chunks2 = ((StructHeap<T2>)heapMap[query.signatureIndexes.T2]).chunks;
            for (int chunkPos = 0; chunkPos <= chunkEnd; chunkPos++)
            {
                var componentLen    = chunkPos < chunkEnd ? StructInfo.ChunkSize : archetype.ChunkRest();
                var chunk1          = new Chunk<T1>(chunks1[chunkPos].components, query.copyT1, componentLen);
                var chunk2          = new Chunk<T2>(chunks2[chunkPos].components, query.copyT2, componentLen);
                var entities        = new ChunkEntities(archetype, chunkPos, componentLen);
                chunks[pos++]       = new ValueTuple<Chunk<T1>, Chunk<T2>, ChunkEntities>(chunk1, chunk2, entities);
            }
        }
        last    = pos - 1;
        index   = -1;
    }
    
    // --- IEnumerator<>
    public readonly (Chunk<T1>, Chunk<T2>, ChunkEntities) Current => chunks[index];

    // --- IEnumerator
    public void Reset() {
        index = -1;
    }
    
    object IEnumerator.Current => chunks[index];

    public bool MoveNext()
    {
        int i = index;
        if (i < last) {
            index = i + 1;
            ref var chunk = ref chunks[index];
            chunk.Item1.Copy();
            chunk.Item2.Copy();
            return true;
        }
        return false;
    }
    
    // --- IDisposable
    public void Dispose() {
        var array = chunks;
        // clear chunk items to reduce Chunk references 
        for (int n = 0; n <= last; n++) {
            array[n] = default;
        }
        query.chunkArrays.Push(array);
    }
}

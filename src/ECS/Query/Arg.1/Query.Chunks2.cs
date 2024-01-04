// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct QueryChunks2<T1>  : IEnumerable <(Chunk<T1>, ChunkEntities)>  // <- not implemented to avoid boxing
    where T1 : struct, IComponent
{
    private readonly ArchetypeQuery<T1> query;

    public  override string         ToString() => query.signatureIndexes.GetString("Chunks: ");

    internal QueryChunks2(ArchetypeQuery<T1> query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    IEnumerator<(Chunk<T1>, ChunkEntities)> IEnumerable<(Chunk<T1>, ChunkEntities)>.GetEnumerator() => new ChunkEnumerator2<T1> (query);
    
    // --- IEnumerable
    IEnumerator                                                         IEnumerable.GetEnumerator() => new ChunkEnumerator2<T1> (query);
    
    // --- new
    public ChunkEnumerator2<T1>                                                     GetEnumerator() => new ChunkEnumerator2<T1>(query);
}

public struct ChunkEnumerator2<T1> : IEnumerator<(Chunk<T1>, ChunkEntities)>
    where T1 : struct, IComponent
{
    private readonly    Stack<(Chunk<T1>, ChunkEntities)[]> chunkArrays;    //  8
    private readonly    (Chunk<T1>, ChunkEntities)[]        chunks;         //  8
    private readonly    int                                 last;           //  4
    private             int                                 index;          //  4
    
    
    internal  ChunkEnumerator2(ArchetypeQuery<T1> query)
    {
        chunkArrays     = query.chunkArrays;
        var archetypes  = query.GetArchetypes();
        int chunkCount  = 0;
        var archs       = archetypes.array;
        for (int n = 0; n < archetypes.length; n++) {
            chunkCount += archs[n].ChunkEnd() + 1;
        }
        chunks          = GetChunks(chunkArrays, chunkCount);
        int pos         = 0;
        for (int n = 0; n < archetypes.length; n++)
        {
            var archetype   = archs[n];
            int chunkEnd    = archetype.ChunkEnd();
            if (chunkEnd == -1) {
                continue;
            }
            var heapMap     = archetype.heapMap;
            var chunks1     = ((StructHeap<T1>)heapMap[query.signatureIndexes.T1]).chunks;
            for (int i = 0; i <= chunkEnd; i++)
            {
                var componentLen    = i < chunkEnd ? StructInfo.ChunkSize : archetype.ChunkRest();
                var chunk1          = new Chunk<T1>(chunks1[i].components, query.copyT1, componentLen);
                var entities        = new ChunkEntities(archetype, i, componentLen);
                chunks[pos++]       = new ValueTuple<Chunk<T1>, ChunkEntities>(chunk1, entities);
            }
        }
        last    = pos - 1;
        index   = -1;
    }
    
    private static (Chunk<T1>, ChunkEntities)[] GetChunks(Stack<(Chunk<T1>, ChunkEntities)[]> chunkArrays, int chunkCount)
    {
        while (chunkArrays.TryPop(out (Chunk<T1>, ChunkEntities)[] chunks)) {
            if (chunks.Length < chunkCount) {
                continue;
            }
            return chunks;
        }
        return new (Chunk<T1>, ChunkEntities)[chunkCount];
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly (Chunk<T1>, ChunkEntities) Current   => chunks[index];
    
    // --- IEnumerator
    public void Reset() {
        index = -1;
    }
    
    object IEnumerator.Current => Current;

    // --- IDisposable
    public void Dispose() {
        chunkArrays.Push(chunks);
    }
    
    public bool MoveNext()
    {
        if (index < last) {
            index++;
            return true;
        }
        return false;
    }
}

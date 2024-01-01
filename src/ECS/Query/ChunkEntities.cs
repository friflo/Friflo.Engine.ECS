// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct ChunkEntities : IEnumerable<Entity>
{
    public              ReadOnlySpan<int>   Ids         => new(archetype.entityIds, chunkPos * StructInfo.ChunkSize, length);
    public   override   string              ToString()  => $"Length: {length}";

    public   readonly   Archetype           archetype;  //  8
    public   readonly   int                 length;     //  4
    //
    private  readonly   int                 chunkPos;   //  4

    internal ChunkEntities(Archetype archetype, int chunkPos, int componentLen) {
        this.archetype  = archetype;
        length          = componentLen;
        this.chunkPos   = chunkPos;
    }
    
    public Entity this[int index] {
        get {
            if (index < length) {
                return new Entity(archetype.entityIds[chunkPos * StructInfo.ChunkSize + index], archetype.entityStore);
            }
            throw new IndexOutOfRangeException();
        }
    }
    
    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new ChunkEntitiesEnumerator(this);
    
    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new ChunkEntitiesEnumerator(this);
    
    // --- new
    public ChunkEntitiesEnumerator          GetEnumerator() => new ChunkEntitiesEnumerator(this);
}

public struct ChunkEntitiesEnumerator : IEnumerator<Entity>
{
    private readonly    int[]           entityIds;      //  8
    private readonly    EntityStore     store;          //  8
    private readonly    int             length;         //  4
    private             int             index;          //  4
    
    internal ChunkEntitiesEnumerator(in ChunkEntities chunkEntities) {
        entityIds   = chunkEntities.archetype.entityIds;
        store       = chunkEntities.archetype.entityStore;
        length      = chunkEntities.length;
    }
    
    // --- IEnumerator<>
    public readonly Entity Current   => new Entity(entityIds[index - 1], store);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < length) {
            index++;
            return true;
        }
        return false;  
    }

    public void Reset() {
        index = 0;
    }
    
    object IEnumerator.Current => Current;

    // --- IDisposable
    public void Dispose() { }
}
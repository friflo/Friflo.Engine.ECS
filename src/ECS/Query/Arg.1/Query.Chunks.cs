// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunks<T1>
    where T1 : struct, IComponent
{
    public              int             Length => Chunk1.Length;
    public readonly     Chunk<T1>       Chunk1;     //  16
    public readonly     ChunkEntities   Entities;   //  24

    public override     string          ToString() => Entities.GetChunksString();

    internal Chunks(Chunk<T1> chunk1, ChunkEntities entities) {
        Chunk1     = chunk1;
        Entities   = entities;
    }
    
    public void Deconstruct(out Chunk<T1> chunk1, out ChunkEntities entities) {
        chunk1      = Chunk1;
        entities    = Entities;
    }
}

/// <summary>
/// Contains the <see cref="Chunk{T}"/>'s storing components and entities of an <see cref="ArchetypeQuery{T1}"/>.
/// </summary>
public readonly struct QueryChunks<T1>  : IEnumerable <Chunks<T1>>
    where T1 : struct, IComponent
{
    private readonly ArchetypeQuery<T1> query;

    public  override string         ToString() => query.GetQueryChunksString();

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
    //
    private             int                     archetypePos;   //  4
    private             Chunks<T1>              chunks;         // 40
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1> query)
    {
        copyT1          = query.copyT1;
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
        // --- skip archetypes without entities
        do {
            if (archetypePos >= archetypes.last) {  // last = length - 1
                return false;
            }
            archetype   = archetypes.array[++archetypePos];
        }
        while (archetype.entityCount == 0); 
        
        // --- set chunks of new archetype
        var heapMap     = archetype.heapMap;
        var chunks1     = (StructHeap<T1>)heapMap[structIndex1];
        int count       = archetype.entityCount;
            
        var chunk1      = new Chunk<T1>(chunks1.components, copyT1, count);
        var entities    = new ChunkEntities(archetype, count);
        chunks          = new Chunks<T1>(chunk1, entities);
        return true;  
    }
    
    // --- IDisposable
    public void Dispose() { }
}

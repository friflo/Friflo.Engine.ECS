// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunks
{
    public              int             Length => entities.Length;

    public readonly     ChunkEntities   entities;   //  24

    public override     string          ToString() => entities.GetChunksString();

    internal Chunks(ChunkEntities entities) {
        this.entities   = entities;
    }
    
    public void Deconstruct(out ChunkEntities entities) {
        entities    = this.entities;
    }
}


public readonly struct QueryChunks  : IEnumerable <Chunks>
{
    private readonly ArchetypeQuery query;

    public  override string         ToString() => query.GetQueryChunksString();

    internal QueryChunks(ArchetypeQuery query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<Chunks>
    IEnumerable<Chunks>.GetEnumerator() => new ChunkEnumerator (query);
    
    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator     IEnumerable.GetEnumerator() => new ChunkEnumerator (query);
    
    // --- IEnumerable
    public ChunkEnumerator GetEnumerator() => new (query);
}

public struct ChunkEnumerator : IEnumerator<Chunks>
{
    private readonly    Archetypes              archetypes;     // 16
    //
    private             int                     archetypePos;   //  4
    private             Chunks                  chunks;         // 24
    
    
    internal  ChunkEnumerator(ArchetypeQuery query)
    {
        archetypes      = query.GetArchetypes();
        archetypePos    = -1;
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly Chunks Current   => chunks;
    
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
        int count       = archetype.entityCount;
            
        var entities    = new ChunkEntities(archetype, count);
        chunks          = new Chunks(entities);
        return true;  
    }
    
    // --- IDisposable
    public void Dispose() { }
}

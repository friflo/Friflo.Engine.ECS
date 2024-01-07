// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public readonly struct QueryEntities  : IEnumerable <ChunkEntities>
{
    private readonly ArchetypeQuery query;

    public  override string         ToString() => query.GetQueryChunksString();

    internal QueryEntities(ArchetypeQuery query) {
        this.query = query;
    }
    
    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<ChunkEntities>
    IEnumerable<ChunkEntities>.GetEnumerator()  => new ChunkEnumerator (query);
    
    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator     IEnumerable.GetEnumerator() => new ChunkEnumerator (query);
    
    // --- IEnumerable
    public ChunkEnumerator      GetEnumerator() => new (query);
}

public struct ChunkEnumerator : IEnumerator<ChunkEntities>
{
    private readonly    Archetypes              archetypes;     // 16
    //
    private             int                     archetypePos;   //  4
    private             ChunkEntities           entities;       // 24
    
    
    internal  ChunkEnumerator(ArchetypeQuery query)
    {
        archetypes      = query.GetArchetypes();
        archetypePos    = -1;
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly ChunkEntities Current   => entities;
    
    // --- IEnumerator
    [ExcludeFromCodeCoverage]
    public void Reset() {
        archetypePos    = -1;
        entities        = default;
    }

    [ExcludeFromCodeCoverage]
    object IEnumerator.Current  => entities;
    
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
            
        entities        = new ChunkEntities(archetype, count);
        return true;  
    }
    
    // --- IDisposable
    public void Dispose() { }
}

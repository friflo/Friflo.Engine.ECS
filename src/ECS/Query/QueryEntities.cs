// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide the result set of an <see cref="ArchetypeQuery"/> as a set of <see cref="Entity"/>'s.
/// </summary>
public readonly struct QueryEntities  : IEnumerable <Entity>
{
    public              int             Count       => query.EntityCount;
    public  override    string          ToString()  => $"Entity[{query.EntityCount}]";

    private readonly    ArchetypeQuery  query;

    internal QueryEntities(ArchetypeQuery query) {
        this.query = query;
    }
    
    /// <summary>
    /// Apply the given entity <paramref name="batch"/> to all entities in this set. 
    /// </summary>
    public void ApplyBatch(EntityBatch batch)
    {
        foreach (var entity in this) {
            entity.store.ApplyBatchTo(batch, entity.Id);
        }
    }
    
    // --- IEnumerable<>
    [ExcludeFromCodeCoverage]
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()  => new EntitiesEnumerator (query);
    
    // --- IEnumerable
    [ExcludeFromCodeCoverage]
    IEnumerator                 IEnumerable.GetEnumerator() => new EntitiesEnumerator (query);
    
    // --- IEnumerable
    public EntitiesEnumerator               GetEnumerator() => new (query);
}

/// <summary>
/// Used to enumerate the <see cref="ArchetypeQuery.Entities"/> of an  <see cref="ArchetypeQuery"/>.
/// </summary>
public struct EntitiesEnumerator : IEnumerator<Entity>
{
    private readonly    EntityStore store;          //  8
    private readonly    Archetypes  archetypes;     // 16
    //
    private             int[]       entityIds;      //  8
    private             int         entityIndex;    //  4    
    private             int         entityLast;     //  4
    private             int         archetypePos;   //  4
    //    
    private             Entity      current;        // 16

    
    internal  EntitiesEnumerator(ArchetypeQuery query)
    {
        store           = query.Store;
        archetypes      = query.GetArchetypes();
        entityIndex     = -1;
        entityLast      = -1;
        archetypePos    = -1;
    }
    
    // --- IEnumerator<>
    public readonly Entity Current => current;
    
    // --- IEnumerator
    [ExcludeFromCodeCoverage]
    public void Reset() {
        entityIndex     = -1;
        entityLast      = -1;
        archetypePos    = -1;
        current         = default;
    }

    [ExcludeFromCodeCoverage]
    object IEnumerator.Current  => current;
    
    public bool MoveNext()
    {
        if (entityIndex < entityLast) {
            current = new Entity(store, entityIds[++entityIndex]);
            return true;
        }
        Archetype archetype;
        // --- skip archetypes without entities
        do {
            if (archetypePos >= archetypes.last) {  // last = length - 1
                return false;
            }
            archetype   = archetypes.array[++archetypePos];
        }
        while (archetype.entityCount == 0); 
        
        entityIds       = archetype.entityIds;
        entityIndex     = 0;
        entityLast      = archetype.entityCount - 1;
        current         = new Entity(store, entityIds[0]);
        return true;  
    }
    
    // --- IDisposable
    public void Dispose() { }
}
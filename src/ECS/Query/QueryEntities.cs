// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide the result set of an <see cref="ArchetypeQuery"/> as a set of <see cref="Entity"/>'s.
/// </summary>
[DebuggerTypeProxy(typeof(QueryEntitiesDebugView))]
public readonly struct QueryEntities  : IEnumerable <Entity>
{
    /// <summary> Return the number of entities matching the query. </summary>
    public              int             Count       => query.Count;
    
    public   override   string          ToString()  => $"Entity[{query.Count}]";

    internal readonly   ArchetypeQuery  query;

    internal QueryEntities(ArchetypeQuery query) {
        this.query = query;
    }
    
    /// <summary>
    /// Returns the query result as an <see cref="EntityList"/> to perform structural changes.
    /// </summary>
    public EntityList ToEntityList() => ToEntityList(new EntityList());
    
    /// <summary>
    /// Returns the query result as an <see cref="EntityList"/> to perform structural changes.
    /// </summary>
    public EntityList ToEntityList(EntityList list)
    {
        list.Clear();
        list.entityStore = query.Store;
        foreach (var entity in query.Entities) {
            list.AddInternal(entity.Id);   
        }
        return list;
    }
    
    /// <summary>
    /// Apply the given entity <paramref name="batch"/> to all entities in this set.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#entitybatch---query">Example.</a>
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
        if (archetypes.chunkPositions != null) {
            if (entityIndex >= archetypes.last) {  // last = length - 1
                return false;
            }
            var pos = ++entityIndex;
            current = new Entity(store, archetypes.array[pos].entityIds[archetypes.chunkPositions[pos]]);
            return true;
        }
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

internal class QueryEntitiesDebugView
{
    [Browse(RootHidden)]
    public              Entity[]        Entities => GetEntities();

    [Browse(Never)]
    private readonly    QueryEntities   queryEntities;
        
    internal QueryEntitiesDebugView(QueryEntities queryEntities)
    {
        this.queryEntities = queryEntities;
    }
    
    private Entity[] GetEntities()
    {
        var entities = new Entity[queryEntities.Count];
        int n = 0; 
        foreach (var entity in queryEntities) {
            entities[n++] = entity;
        }
        return entities;
    }
}

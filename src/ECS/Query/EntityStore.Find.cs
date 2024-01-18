// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    [Obsolete("experimental")]
    public Entity GetUniqueEntity(string name)
    {
        var query = internBase.uniqueEntityQuery;
        if (query == null) {
            query = internBase.uniqueEntityQuery = Query<UniqueEntity>();
        }
        // --- enumerate entities with unique names
        var foundId = -1;
        foreach (var (uniqueEntity, entities) in query.Chunks)
        {
            var uniqueEntities = uniqueEntity.Span;
            for (int n = 0; n < uniqueEntities.Length; n++) {
                if (uniqueEntities[n].name != name) {
                    continue;
                }
                if (foundId != -1) {
                    throw MultipleEntitiesWithSameName(name);
                }
                foundId = entities[n];
            }
        }
        if (foundId != -1) {
            return new Entity((EntityStore)this, foundId);
        }
        throw new InvalidOperationException($"found no {nameof(UniqueEntity)} with name: \"{name}\"");
    }
    
    private static InvalidOperationException MultipleEntitiesWithSameName(string name) {
        return new InvalidOperationException($"found multiple {nameof(UniqueEntity)}'s with name: \"{name}\"");
    }
    
    /// <summary>
    /// Return the entity matching the given <paramref name="allTags"/>.<br/>
    /// <br/>
    /// Use Tags.Get&lt;>() to get filter tags.
    /// </summary>
    /// <exception cref="InvalidOperationException"> in case none or more than 1 matching entities found.</exception>
    [Obsolete("experimental")]
    public Entity FindEntityWithTags (in Tags allTags)
    {
        var query = internBase.findQuery ??= new ArchetypeQuery(this);
        query.Set(default, allTags);
        return FindSingleEntity(query);
    }
    
    /// <summary>
    /// Return the entity matching the given <paramref name="allTags"/>
    /// and the given <paramref name="requiredComponents"/>.<br/>
    /// <br/>
    /// Use Tags.Get&lt;>() to get filter tags.<br/>
    /// Use ComponentTypes.Get&lt;>() to get component types.<br/>
    /// </summary>
    /// <exception cref="InvalidOperationException"> in case none or more than 1 matching entities found.</exception>
    [Obsolete("experimental")]
    public Entity FindEntity (in Tags allTags, in ComponentTypes requiredComponents)
    {
        var query = internBase.findQuery ??= new ArchetypeQuery(this);
        query.Set(requiredComponents, allTags);
        return FindSingleEntity(query);
    }
    
    /// <summary>
    /// Return the entity matching the given <paramref name="allTags"/>.<br/>
    /// <br/>
    /// Use Tags.Get&lt;>() to get filter tags.
    /// </summary>
    /// <exception cref="InvalidOperationException"> in case none or more than 1 matching entities found.</exception>
    [Obsolete("experimental")]
    public Entity[] FindEntities (in Tags allTags, in ComponentTypes requiredComponents)
    {
        var query = internBase.findQuery ??= new ArchetypeQuery(this);
        query.Set(requiredComponents, allTags);
        return GetFindEntities(query);
    }
    
    /// <summary>
    /// Return the entity matching the given <paramref name="allTags"/>.<br/>
    /// <br/>
    /// Use Tags.Get&lt;>() to get filter tags.
    /// </summary>
    /// <exception cref="InvalidOperationException"> in case none or more than 1 matching entities found.</exception>
    [Obsolete("experimental")]
    public Entity[] FindEntitiesWithTags(in Tags allTags)
    {
        var query = internBase.findQuery ??= new ArchetypeQuery(this);
        query.Set(default, allTags);
        return GetFindEntities(query);
    }

    private Entity FindSingleEntity (ArchetypeQuery findQuery)
    {
        // --- check if query contains exact one entity 
        var queryEntityCount = findQuery.EntityCount;
        switch (queryEntityCount) {
            case 0:
                throw new InvalidOperationException("found no matching entity");
            case > 1:
                throw new InvalidOperationException($"found multiple matching entities. found: {queryEntityCount}");
        }
        
        // --- get the one entity
        var archetypes      = findQuery.GetArchetypes();
        Archetype archetype = null;
        for (int archIndex = 0; archIndex < archetypes.length; archIndex++) {
            archetype = archetypes.array[archIndex];
            if (archetype.entityCount == 0) {
                continue;
            }
            break;
        }
        var entityId = archetype!.entityIds[0];
        return new Entity((EntityStore)this, entityId);
    }
    
    /// <remarks>
    /// Cannot return <see cref="QueryEntities"/> in <see cref="FindEntities"/> as <see cref="InternBase.findQuery"/><br/>
    /// is reused and may change before <see cref="QueryEntities"/> is enumerated.
    /// </remarks>
    private Entity[] GetFindEntities (ArchetypeQuery findQuery)
    {
        var entities        = new Entity[findQuery.EntityCount];
        var queryEntities   = new QueryEntities(findQuery);
        int n = 0;
        var store = (EntityStore)this;
        foreach (var chunkEntities in queryEntities) {
            foreach (var id in chunkEntities.Ids) {
                entities[n++] = new Entity(store, id);
            }
        }
        return entities;
    }
}

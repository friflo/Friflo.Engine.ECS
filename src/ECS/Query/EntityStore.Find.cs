// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    [Obsolete("experimental")]
    public Entity FindEntity (in Tags withAllTags)
    {
        var query = internBase.findQuery ??= new ArchetypeQuery(this);
        query.Set(default, withAllTags);
        return FindSingleEntity(query);
    }
    
    [Obsolete("experimental")]
    public Entity FindEntity (in Tags withAllTags, in ComponentTypes requiredComponents)
    {
        var query = internBase.findQuery ??= new ArchetypeQuery(this);
        query.Set(requiredComponents, withAllTags);
        return FindSingleEntity(query);
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
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    /// <summary>
    /// Return the entity with a <see cref="UniqueEntity"/> component and its <see cref="UniqueEntity.name"/> == <paramref name="name"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// In case none or more than 1 <see cref="UniqueEntity"/> with the given <paramref name="name"/> found.
    /// </exception>
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
}

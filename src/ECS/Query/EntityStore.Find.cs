// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    /// <summary>
    /// Return the entity with a <see cref="UniqueEntity"/> component and its <see cref="UniqueEntity.uid"/> == <paramref name="uid"/>.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#unique-entity">Example.</a>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// In case none or more than one <see cref="UniqueEntity"/> with the given <paramref name="uid"/> found.
    /// </exception>
    /// <remarks>
    /// To Get all <see cref="UniqueEntity"/>'s of the store use <see cref="UniqueEntities"/>.
    /// </remarks>
    public Entity GetUniqueEntity(string uid)
    {
        var query = internBase.uniqueEntityQuery ??= CreateUniqueEntityQuery();

        // --- enumerate entities with unique names
        var foundId = 0;
        foreach (var (uniqueEntity, entities) in query.Chunks)
        {
            var uniqueEntities = uniqueEntity.Span;
            for (int n = 0; n < uniqueEntities.Length; n++) {
                if (uniqueEntities[n].uid != uid) {
                    continue;
                }
                if (foundId != 0) {
                    throw MultipleEntitiesWithSameName(uid);
                }
                foundId = entities[n];
            }
        }
        if (foundId != 0) {
            return new Entity((EntityStore)this, foundId);
        }
        throw new InvalidOperationException($"found no {nameof(UniqueEntity)} with uid: \"{uid}\"");
    }
    
    private QueryEntities GetUniqueEntities()
    {
        var query = internBase.uniqueEntityQuery ??= CreateUniqueEntityQuery();
        return query.Entities;
    }
    
    private ArchetypeQuery<UniqueEntity> CreateUniqueEntityQuery() => Query<UniqueEntity>().WithDisabled();
    
    private static InvalidOperationException MultipleEntitiesWithSameName(string name) {
        return new InvalidOperationException($"found multiple {nameof(UniqueEntity)}'s with uid: \"{name}\"");
    }
}

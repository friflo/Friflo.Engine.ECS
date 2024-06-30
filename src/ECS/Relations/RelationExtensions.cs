// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Relations;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class RelationExtensions
{
    /// <summary>
    /// Returns all unique relation components of the passed <paramref name="entity"/>.<br/>
    /// In case <typeparamref name="TComponent"/> is a <see cref="ILinkRelation"/> it returns all linked entities.
    /// </summary>
    public static RelationComponents<TComponent> GetRelations<TComponent>(this Entity entity)
        where TComponent : struct, IRelationComponent
    {
        return EntityRelations.GetRelations<TComponent>(entity);
    }
    
    /// <summary>
    /// Removes the relation component with the specified <paramref name="key"/> from an entity.
    /// </summary>
    public static bool RemoveRelation<T, TKey>(this Entity entity, TKey key) where T : struct, IRelationComponent<TKey> {
        return EntityRelations.RemoveRelation<T, TKey>(entity.Store, entity.Id, key);
    }
    
    /// <summary>
    /// Removes the specified link relation <paramref name="target"/> from an entity.
    /// </summary>
    public static bool RemoveLinkRelation<T>(this Entity entity, Entity target) where T : struct, ILinkRelation {
        return EntityRelations.RemoveRelation<T, Entity>(entity.Store, entity.Id, target);
    }
}
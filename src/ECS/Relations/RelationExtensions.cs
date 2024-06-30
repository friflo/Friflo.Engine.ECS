// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Relations;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class RelationExtensions
{
    public static RelationComponents<TComponent> GetRelations<TComponent>(this Entity entity)
        where TComponent : struct, IRelationComponent
    {
        return EntityRelations.GetRelations<TComponent>(entity);
    }
    
    public static bool RemoveRelation<T, TKey>(this Entity entity, TKey key) where T : struct, IRelationComponent<TKey> {
        return EntityRelations.RemoveRelation<T, TKey>(entity.Store, entity.Id, key);
    }
    
    public static bool RemoveLinkRelation<T>(this Entity entity, Entity target) where T : struct, ILinkRelation {
        return EntityRelations.RemoveRelation<T, Entity>(entity.Store, entity.Id, target);
    }
}
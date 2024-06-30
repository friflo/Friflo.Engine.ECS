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
        var index       = StructInfo<TComponent>.Index;
        var relations   = entity.Store.relationsMap[index];
        if (relations != null) {
            return relations.GetRelations<TComponent>(entity);
        }
        return default;
    }
    
    public static bool RemoveRelation<T, TValue>(this Entity entity, TValue value) where T : struct, IRelationComponent<TValue> {
        return EntityRelations.RemoveRelation<T, TValue>(entity.Store, entity.Id, value);
    }
}
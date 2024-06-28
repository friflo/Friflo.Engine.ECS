// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;

internal static class RelationExtensions
{
    public static Relations<TComponent> GetRelations<TComponent, TValue>(this Entity entity) where TComponent : struct, IRelationComponent<TValue>
    {
        var index       = StructInfo<TComponent>.Index;
        var relations   = (RelationsArchetype<TComponent, TValue>)entity.Store.relationsMap[index];
        if (relations != null) {
            return relations.GetRelations<TComponent>(entity);
        }
        return default;
    }
    
    public static bool RemoveRelation<T, TValue>(this Entity entity, TValue value) where T : struct, IRelationComponent<TValue> {
        return RelationsArchetype.RemoveRelation<T, TValue>(entity.Store, entity.Id, value);
    }
}
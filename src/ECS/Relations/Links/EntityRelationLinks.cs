// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;


/// Contains a single <see cref="Archetype"/> with a single <see cref="StructHeap{T}"/><br/>
internal class EntityRelationLinks<TRelationComponent> : EntityRelations<TRelationComponent, Entity>
    where TRelationComponent : struct, ILinkRelation
{
    /// Instance created at <see cref="EntityRelations.GetEntityRelations"/>
    public EntityRelationLinks(ComponentType componentType, Archetype archetype, StructHeap heap)
        : base(componentType, archetype, heap)
    {
        linkEntityMap   = new Dictionary<int, IdArray>();
        linkIdsHeap     = new IdArrayHeap();
    }
    
#region mutation

    /// <returns>true - component is newly added to the entity.<br/> false - component is updated.</returns>
    protected override bool AddComponent<TComponent>(int id, TComponent component)
    {
        Entity target   = RelationUtils<TComponent, Entity>.GetRelationKey(component);
        var added       = true;
        var position    = FindRelationPosition(id, target, out var positions, out _);
        if (position >= 0) {
            added = false;
            goto AssignComponent;
        }
        position = AddEntityRelation(id, positions);
        LinkRelationUtils.AddComponentValue(id, target.Id, this);
    AssignComponent:
        ((StructHeap<TComponent>)heap).components[position] = component;
        return added;
    }

    /// <returns>true if entity contained a relation of the given type before</returns>
    internal override bool RemoveRelation(int id, Entity target)
    {
        var position = FindRelationPosition(id, target, out var positions, out int index);
        if (position >= 0) {
            RemoveEntityRelation(id, position, positions, index);
            LinkRelationUtils.RemoveComponentValue(id, target.Id, this);
            return true;
        }
        return false;
    }
    
    internal override void RemoveLinksWithTarget(int targetId)
    {
        linkEntityMap.TryGetValue(targetId, out var sourceIds);
        var sourceIdSpan = sourceIds.GetIdSpan(linkIdsHeap);
        // TODO check if it necessary to make a copy of idSpan - e.g. by stackalloc
        // TODO optimize removing link relations. Currently O(N^2). Possible O(N). N: number of entity relations
        foreach (var sourceId in sourceIdSpan) {
            var target = new Entity(store, targetId);
            RemoveRelation(sourceId, target);
        }
    }
    #endregion
}
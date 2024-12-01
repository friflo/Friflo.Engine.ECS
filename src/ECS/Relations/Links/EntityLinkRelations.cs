// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable UnusedVariable
// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;


/// Contains a single <see cref="Archetype"/> with a single <see cref="StructHeap{T}"/><br/>
internal class EntityLinkRelations<TRelation> : GenericEntityRelations<TRelation, Entity>
    where TRelation : struct, ILinkRelation
{
    /// Instance created at <see cref="AbstractEntityRelations.GetEntityRelations"/>
    public EntityLinkRelations(ComponentType componentType, Archetype archetype, StructHeap heap)
        : base(componentType, archetype, heap)
    {
        linkEntityMap   = new Dictionary<int, IdArray>();
        linkIdsHeap     = new IdArrayHeap();
    }
    
    /// Expect: component is present
    internal override ref T GetEntityRelation<T>(int id, int targetId)
    {
        Entity target   = new Entity(store, targetId);
        int position    = FindRelationPosition(id, target, out _, out _);
        return ref ((StructHeap<T>)heap).components[position];
    }
    
    internal override void AddIncomingRelations(int target, List<EntityLink> result)
    {
        linkEntityMap.TryGetValue(target, out var sourceIds);
        var sourceIdSpan    = sourceIds.GetSpan(linkIdsHeap, store);
        var components      = heapGeneric.components;
        Entity targetEntity = new Entity(store, target);    
        foreach (var sourceId in sourceIdSpan) {
            var position = FindRelationPosition(sourceId, targetEntity, out var positions, out _);
            var link     = new EntityLink (new Entity(store, sourceId), target, components[position]);
            result.Add(link);
        }
    }
    
#region mutation

    /// <returns>true - component is newly added to the entity.<br/> false - component is updated.</returns>
    internal override bool AddComponent<T>(int id, in T component)
    {
        Entity target   = RelationUtils<T, Entity>.GetRelationKey(component);
        bool added      = true;
        int position    = FindRelationPosition(id, target, out var positions, out _);
        if (position >= 0) {
            added = false;
            goto AssignComponent;
        }
        position = AddEntityRelation(id, positions);
        LinkRelationUtils.AddComponentValue(id, target.Id, this);
    AssignComponent:
        ((StructHeap<T>)heap).components[position] = component;
        return added;
    }

    /// Executes in O(M)  M: number of entity relations
    /// <returns>true if entity contained a relation of the given type before</returns>
    internal override bool RemoveRelation(int id, Entity target)
    {
        int position = FindRelationPosition(id, target, out var positions, out int index);
        if (position >= 0) {
            RemoveEntityRelation(id, position, positions, index);
            LinkRelationUtils.RemoveComponentValue(id, target.Id, this);
            return true;
        }
        return false;
    }
    
    /// Executes in O(N * M).  N: number link relations  M: RemoveRelation() executes in O(M)
    internal override void RemoveLinksWithTarget(int targetId)
    {
        linkEntityMap.TryGetValue(targetId, out var sourceIds);
        var sourceIdSpan = sourceIds.GetSpan(linkIdsHeap, store);
        // TODO check if it necessary to make a copy of idSpan - e.g. by stackalloc
        foreach (var sourceId in sourceIdSpan) {
            var target = new Entity(store, targetId);
            RemoveRelation(sourceId, target);
        }
    }
    
    internal override void RemoveEntityRelations(int id)
    {
        positionMap.TryGetValue(id, out var positions);
        RemoveIncomingLinks(positions, id);
        while (positions.count > 0) {
            var lastIndex   = positions.count - 1;
            int position    = positions.GetAt(lastIndex, idHeap);
            positions       = RemoveEntityRelation(id, position, positions, lastIndex);
        }
    }
    
    private void RemoveIncomingLinks(IdArray positions, int id)
    {
        var positionsSpan   = positions.GetSpan(idHeap, store);
        var components      = ((StructHeap<TRelation>)heap).components;
        var linkMap         = linkEntityMap;
        foreach (var position in positionsSpan)
        {
            var targetId        = components[position].GetRelationKey().Id;
            var sourceIds       = linkMap[targetId];
            var sourceIdSpan    = sourceIds.GetSpan(linkIdsHeap, store);
            var idPosition      = sourceIdSpan.IndexOf(id);
            sourceIds.RemoveAt(idPosition, linkIdsHeap);
            if (sourceIds.count == 0) {
                linkMap.Remove(targetId);
            } else {
                linkMap[targetId] = sourceIds;
            }
        }
    }
    #endregion
}
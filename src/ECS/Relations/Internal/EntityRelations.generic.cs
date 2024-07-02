// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS.Index;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;


/// Contains a single <see cref="Archetype"/> with a single <see cref="StructHeap{T}"/><br/>
internal sealed class EntityRelations<TRelationComponent, TKey> : EntityRelations
    where TRelationComponent : struct, IRelationComponent<TKey>
{
    private  readonly   StructHeap<TRelationComponent>  heapGeneric;
    
    /// Instance created at <see cref="EntityRelations.GetEntityRelations"/>
    public EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap)
        : base(componentType, archetype, heap)
    {
       heapGeneric = (StructHeap<TRelationComponent>)heap;
    }
    
    private int FindRelationPosition(int id, TKey key, out IdArray positions, out int index)
    {
        relationPositions.TryGetValue(id, out positions);
        var positionSpan    = positions.GetIdSpan(idHeap);
        var positionCount   = positions.count;
        var components      = heapGeneric.components;
        for (index = 0; index < positionCount; index++) {
            var position    = positionSpan[index];
            var relation    = components[position].GetRelationKey(); // no boxing
            //  var relation    = RelationUtils<TRelationComponent, TKey>.GetRelationKey(components[position]);
            if (EqualityComparer<TKey>.Default.Equals(relation, key)) {
                return position;
            }
        }
        return -1;
    }
    
#region query
    internal override IComponent GetRelationAt(int id, int index)
    {
        relationPositions.TryGetValue(id, out var positions);
        var count = positions.count;
        if (count == 1) {
            return heapGeneric.components[positions.start];
        }
        var poolIndex       = IdArrayHeap.PoolIndex(count);
        var poolPositions   = idHeap.GetPool(poolIndex).Ids;
        return heapGeneric.components[poolPositions[index]];
    }
    
    internal ref TComponent GetRelation<TComponent>(int id, TKey key)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var position = FindRelationPosition(id, key, out _, out _);
        if (position >= 0) {
            return ref ((StructHeap<TComponent>)heap).components[position];
        }
        throw KeyNotFoundException(id, key);
    }
    
    internal bool TryGetRelation<TComponent>(int id, TKey key, out TComponent value)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var position = FindRelationPosition(id, key, out _, out _);
        if (position >= 0) {
            value = ((StructHeap<TComponent>)heap).components[position];
            return true;
        }
        value = default;
        return false;
    }
    
    internal override void ForAllEntityRelations<TComponent>(ForEachEntity<TComponent> lambda)
    {
        var components  = ((StructHeap<TComponent>)heap).components;
        var count       = archetype.Count;
        var ids         = archetype.entityIds;
        var entityStore = store;
        for (int n = 0; n < count; n++) {
            lambda(ref components[n], new Entity(entityStore, ids[n]));
        }
    }
    
    internal override (Entities entities, Chunk<TComponent> relations) GetAllEntityRelations<TComponent>()
    {
        int count       = archetype.Count;
        var entities    = new Entities(store, archetype.entityIds, 0, count);
        var components  = ((StructHeap<TComponent>)heap).components;
        var chunk       = new Chunk<TComponent>(components, null, count, 0);
        return (entities, chunk);
    }
    #endregion
    
#region mutation

/// <returns>true - component is newly added to the entity.<br/> false - component is updated.</returns>
protected override bool AddComponent<TComponent>(int id, TComponent component)
    {
        var relationKey = RelationUtils<TComponent, TKey>.GetRelationKey(component);
    //  var relationKey = ((IRelationComponent<TKey>)component).GetRelationKey(); // boxing version
        var added       = true;
        var position    = FindRelationPosition(id, relationKey, out var positions, out _);
        if (position >= 0) {
            added = false;
            goto AssignComponent;
        }
        position = AddEntityRelation(id, positions);
    AssignComponent:
        ((StructHeap<TComponent>)heap).components[position] = component;
        return added;
    }

    /// <returns>true if entity contained a relation of the given type before</returns>
    internal bool RemoveRelation(int id, TKey key)
    {
        var position = FindRelationPosition(id, key, out var positions, out int index);
        if (position >= 0) {
            RemoveEntityRelation(id, position, positions, index);
            return true;
        }
        return false;
    }
    #endregion
}
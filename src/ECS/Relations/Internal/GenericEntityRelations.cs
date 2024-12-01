// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;


/// Contains a single <see cref="Archetype"/> with a single <see cref="StructHeap{T}"/><br/>
internal class GenericEntityRelations<TRelation, TKey> : AbstractEntityRelations
    where TRelation : struct, IRelation<TKey>
{
    /// Single <see cref="StructHeap"/> stored in the <see cref="AbstractEntityRelations.archetype"/>.
    internal  readonly   StructHeap<TRelation>  heapGeneric;
    
    /// Instance created at <see cref="AbstractEntityRelations.GetEntityRelations"/>
    public GenericEntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap)
        : base(componentType, archetype, heap)
    {
        heapGeneric = (StructHeap<TRelation>)heap;
    }
    
    /// Executes in O(M)  M: number of entity relations
    protected int FindRelationPosition(int id, TKey key, out IdArray positions, out int index)
    {
        positionMap.TryGetValue(id, out positions);
        var positionSpan    = positions.GetSpan(idHeap, store);
        int positionCount   = positions.count;
        var components      = heapGeneric.components;
        for (index = 0; index < positionCount; index++) {
            int position    = positionSpan[index];
            TKey relation   = components[position].GetRelationKey(); // no boxing
            //  var relation    = RelationUtils<TRelation, TKey>.GetRelationKey(components[position]);
            if (EqualityComparer<TKey>.Default.Equals(relation, key)) {
                return position;
            }
        }
        return -1;
    }
    
#region query
    internal override IRelation GetRelationAt(int id, int index)
    {
        positionMap.TryGetValue(id, out var positions);
        var count       = positions.count;
        var components  = heapGeneric.components;
        var start       = positions.start;
        if (count == 1) {
            return components[start];
        }
        var poolPositions = IdArrayPool.GetIds(count, idHeap);
        return components[poolPositions[start + index]];
    }
    
    internal ref T GetRelation<T>(int id, TKey key)
        where T : struct, IRelation<TKey>
    {
        var position = FindRelationPosition(id, key, out _, out _);
        if (position >= 0) {
            return ref ((StructHeap<T>)heap).components[position];
        }
        throw KeyNotFoundException(id, key);
    }
    
    internal bool TryGetRelation<T>(int id, TKey key, out T value)
        where T : struct, IRelation<TKey>
    {
        var position = FindRelationPosition(id, key, out _, out _);
        if (position >= 0) {
            value = ((StructHeap<T>)heap).components[position];
            return true;
        }
        value = default;
        return false;
    }
    #endregion
    
#region mutation

    /// <returns>true - component is newly added to the entity.<br/> false - component is updated.</returns>
    internal override bool AddComponent<T>(int id, in T component)
    {
        var relationKey = RelationUtils<T, TKey>.GetRelationKey(component);
    //  var relationKey = ((IRelation<TKey>)component).GetRelationKey(); // boxing version
        var added       = true;
        var position    = FindRelationPosition(id, relationKey, out var positions, out _);
        if (position >= 0) {
            added = false;
            goto AssignComponent;
        }
        position = AddEntityRelation(id, positions);
    AssignComponent:
        ((StructHeap<T>)heap).components[position] = component;
        return added;
    }

    /// <returns>true if entity contained a relation of the given type before</returns>
    internal virtual bool RemoveRelation(int id, TKey key)
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
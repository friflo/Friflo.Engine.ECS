// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using Friflo.Engine.ECS.Index;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;


internal abstract class EntityRelations<TKey> : EntityRelations
{
    internal EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap)
        : base(componentType, archetype, heap)
    { }
    
    internal abstract bool RemoveRelation(int id, TKey value);
}
    

/// Contains a single <see cref="Archetype"/> with a single <see cref="StructHeap{T}"/><br/>
internal sealed class EntityRelations<TRelationComponent, TKey> : EntityRelations<TKey>
    where TRelationComponent : struct, IRelationComponent<TKey>
{
    private  readonly   StructHeap<TRelationComponent>  heapGeneric;
    
    /// Instance created at <see cref="EntityRelations.GetEntityRelations"/>
    public EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap)
        : base(componentType, archetype, heap)
    {
       heapGeneric     = (StructHeap<TRelationComponent>)heap;
    }
    
    internal override IComponent GetRelation(int id, int index)
    {
        entityMap.TryGetValue(id, out var array);
        var count = array.count;
        if (count == 1) {
            return heapGeneric.components[array.start];
        }
        var poolIndex   = IdArrayHeap.PoolIndex(count);
        var positions   = idHeap.GetPool(poolIndex).Ids;
        return heapGeneric.components[positions[index]];
    }
    
#region add component

/// <returns>true - component is newly added to the entity.<br/> false - component is updated.</returns>
protected override bool AddComponent<TComponent>(int id, TComponent component)
    {
        var relationKey     = RelationUtils<TComponent, TKey>.GetRelationKey(component);
    //  var relationKey     = ((IRelationComponent<TKey>)component).GetRelationKey(); // boxing version
        entityMap.TryGetValue(id, out var positions);
        var positionSpan    = positions.GetIdSpan(idHeap);
        var positionCount   = positions.count;
        var components      = heapGeneric.components;
        var added           = true;
        int position;
        for (int n = 0; n < positionCount; n++) {
            position        = positionSpan[n];
            var relation    = components[position].GetRelationKey(); // no boxing
        //  var relation    = RelationUtils<TComponent, TKey>.GetRelationKey(((StructHeap<TComponent>)heap).components[position]);
            if (EqualityComparer<TKey>.Default.Equals(relation, relationKey)) {
                added = false;
                goto AssignComponent;
            }
        }
        position = AddRelation(id, positions);
    AssignComponent:
        ((StructHeap<TComponent>)heap).components[position] = component;
        return added;
    }
    #endregion

#region remove component
    /// <returns>true if entity contained a relation of the given type before</returns>
    internal override bool RemoveRelation(int id, TKey value)
    {
        if (!entityMap.TryGetValue(id, out var positions)) {
            return false;
        }
        var components      = heapGeneric.components;
        var positionSpan    = positions.GetIdSpan(idHeap);
        var positionCount   = positions.count;
        for (int n = 0; n < positionCount; n++) {
            var position    = positionSpan[n];
        //  var relation    = RelationUtils<TRelationComponent, TKey>.GetRelationKey(components[position]);
            var relation    = components[position].GetRelationKey(); // no boxing
            if (!EqualityComparer<TKey>.Default.Equals(relation, value)) {
                continue;
            }
            RemoveRelation(id, position, positions, n);
            return true;
        }
        return false;
    }
    #endregion
}
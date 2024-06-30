// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using Friflo.Engine.ECS.Index;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;


internal abstract class EntityRelations<TValue> : EntityRelations
{
    internal EntityRelations(ComponentType componentType, Archetype archetype) : base(componentType, archetype) {}
    
    internal abstract bool RemoveRelation(int id, TValue value);
}
    

/// Contains a single <see cref="Archetype"/> with a single <see cref="StructHeap{T}"/><br/>
internal sealed class EntityRelations<TRelationComponent, TValue> : EntityRelations<TValue>
    where TRelationComponent : struct, IRelationComponent<TValue>
{
    private  readonly   StructHeap                      heap;
    private  readonly   StructHeap<TRelationComponent>  heapGeneric;
    
    /// Instance created at <see cref="EntityRelations.GetRelationArchetype"/>
    public EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap) : base(componentType, archetype) {
        this.heap       = heap;
        heapGeneric     = (StructHeap<TRelationComponent>)heap;
    }
    
    internal override IComponent GetRelation(Entity entity, int index)
    {
        entityMap.TryGetValue(entity.Id, out var array);
        var count = array.count;
        if (count == 1) {
            return heapGeneric.components[array.start];
        }
        var poolIndex   = IdArrayHeap.PoolIndex(count);
        var positions   = idHeap.GetPool(poolIndex).Ids;
        return heapGeneric.components[positions[index]];
    }
    
    internal RelationComponents<TComponent> GetRelations<TComponent>(Entity entity)
        where TComponent : struct, IComponent
    {
        entityMap.TryGetValue(entity.Id, out var array);
        var count           = array.count;
        var componentHeap   = (StructHeap<TComponent>)heap;
        switch (count) {
            case 0: return  new RelationComponents<TComponent>();
            case 1: return  new RelationComponents<TComponent>(componentHeap.components, array.start);
        }
        var poolIndex   = IdArrayHeap.PoolIndex(count);
        var positions   = idHeap.GetPool(poolIndex).Ids;
        return new RelationComponents<TComponent>(componentHeap.components, positions, array.start, array.count);
    }
    
#region add component

/// <returns>true - component is newly added to the entity.<br/> false - component is updated.</returns>
protected override bool AddComponent<TComponent>(int id, TComponent component)
    {
        var relationValue   = RelationUtils<TComponent, TValue>.GetRelationValue(component);
    //  var relationValue   = ((IRelationComponent<TValue>)component).GetRelation(); // boxing version
        entityMap.TryGetValue(id, out var positions);
        var positionSpan    = positions.GetIdSpan(idHeap);
        var positionCount   = positions.count;
        var components      = heapGeneric.components;
        var added           = true;
        int position;
        for (int n = 0; n < positionCount; n++) {
            position        = positionSpan[n];
            var relation    = components[position].GetRelation(); // no boxing
        //  var relation    = RelationUtils<TComponent, TValue>.GetRelationValue(((StructHeap<TComponent>)heap).components[position]);
            if (EqualityComparer<TValue>.Default.Equals(relation, relationValue)) {
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
    internal override bool RemoveRelation(int id, TValue value)
    {
        if (!entityMap.TryGetValue(id, out var positions)) {
            return false;
        }
        var components      = heapGeneric.components;
        var positionSpan    = positions.GetIdSpan(idHeap);
        var positionCount   = positions.count;
        for (int n = 0; n < positionCount; n++) {
            var position    = positionSpan[n];
        //  var relation    = RelationUtils<TRelationComponent, TValue>.GetRelationValue(components[position]);
            var relation    = components[position].GetRelation(); // no boxing
            if (!EqualityComparer<TValue>.Default.Equals(relation, value)) {
                continue;
            }
            RemoveRelation(id, position, positions, n);
            return true;
        }
        return false;
    }
    #endregion
}
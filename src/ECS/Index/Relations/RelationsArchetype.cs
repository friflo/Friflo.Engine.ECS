// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class RelationsArchetype
{
    internal  readonly   Archetype  archetype;
    
    internal RelationsArchetype(Archetype archetype) {
        this.archetype  = archetype;
    }
    
    protected abstract bool AddComponent<TComponent>(int id, TComponent component) where TComponent : struct, IComponent;
    
    internal static bool AddRelation<TComponent>(EntityStoreBase store, int id, TComponent component) where TComponent : struct, IComponent
    {
        var relation = GetRelationArchetype(store, StructInfo<TComponent>.Index);
        return relation.AddComponent(id, component);
    }
    
    internal static bool RemoveRelation<TComponent, TValue>(EntityStoreBase store, int id, TValue value)  where TComponent : struct, IRelationComponent<TValue>
    {
        var relation = (RelationsArchetype<TValue>)GetRelationArchetype(store, StructInfo<TComponent>.Index);
        return relation.RemoveRelation(id, value);
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2077", Justification = "TODO")] // TODO
    private static RelationsArchetype GetRelationArchetype(EntityStoreBase store, int structIndex)
    {
        var relations = store.relationsMap[structIndex];
        if (relations != null) {
            return relations;
        }
        var componentType   = EntityStoreBase.Static.EntitySchema.components[structIndex];
        var heap            = componentType.CreateHeap();
        var config          = EntityStoreBase.GetArchetypeConfig(store);
        var archetype       = new Archetype(config, heap);
        
        var obj             = Activator.CreateInstance(componentType.RelationType, archetype, heap);
        return store.relationsMap[structIndex] = (RelationsArchetype)obj;
    //  return store.relationsMap[structIndex] = new RelationArchetype<TComponent, TValue>(archetype, heap);
    }
}

internal abstract class RelationsArchetype<TValue> : RelationsArchetype
{
    internal RelationsArchetype(Archetype archetype) : base(archetype) {}
    
    internal abstract bool RemoveRelation(int id, TValue value);
}
    
/// <summary>
/// Contains a single <see cref="Archetype"/> with a single <see cref="StructHeap{T}"/>
/// </summary>
internal sealed class RelationsArchetype<TRelationComponent, TValue> : RelationsArchetype<TValue> where TRelationComponent : struct, IRelationComponent<TValue>
{

    private  readonly   Dictionary<int, IdArray>        entityMap   = new ();
    private  readonly   IdArrayHeap                     idHeap      = new();
    private  readonly   StructHeap                      heap;
    private  readonly   StructHeap<TRelationComponent>  heapGeneric;
    
    public RelationsArchetype(Archetype archetype, StructHeap heap) : base(archetype) {
        this.heap       = heap;
        heapGeneric     = (StructHeap<TRelationComponent>)heap;
    }
    
    internal Relations<TComponent> GetRelations<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        entityMap.TryGetValue(entity.Id, out var array);

        var count           = array.count;
        var componentHeap   = (StructHeap<TComponent>)heap;
        switch (count) {
            case 0: return  new Relations<TComponent>();
            case 1: return  new Relations<TComponent>(componentHeap.components, array.start);
        }
        var poolIndex   = IdArrayHeap.PoolIndex(count);
        var positions   = idHeap.GetPool(poolIndex).Ids;
        return new Relations<TComponent>(componentHeap.components, positions, array.start, array.count);
    }
    
#region add component

protected override bool AddComponent<TComponent>(int id, TComponent component)
    {
        var relationValue = RelationUtils<TComponent, TValue>.GetRelationValue(component);
    //  var relationValue = ((IRelationComponent<TValue>)component).GetRelation(); // boxing version
        entityMap.TryGetValue(id, out var positions);
        var positionSpan    = positions.GetIdSpan(idHeap);
        var components2     = heapGeneric.components;
        var components      = ((StructHeap<TComponent>)heap).components;
        int compIndex;
        for (int n = 0; n < positionSpan.Length; n++)
        {
            compIndex       = positionSpan[n];
        //  var relation    = RelationUtils<TComponent, TValue>.GetRelationValue(components[compIndex]);
            var relation    = components2[compIndex].GetRelation(); // no boxing
            if (EqualityComparer<TValue>.Default.Equals(relation, relationValue)) {
                components[compIndex] = component;
                return false;
            }
        }
        compIndex = AddRelation(id, positions);
        components[compIndex] = component;
        return true;
    }
    
    // non generic
    private int AddRelation(int id, IdArray positions)
    {
        int compIndex = Archetype.AddEntity(archetype, id);
        positions.AddId(compIndex, idHeap);
        entityMap[id] = positions;
        return compIndex;
    }
    #endregion

#region remove component
    internal override bool RemoveRelation(int id, TValue value)
    {
        if (!entityMap.TryGetValue(id, out var positions)) {
            return false;
        }
        var components      = heapGeneric.components;
        var positionSpan    = positions.GetIdSpan(idHeap);
        for (int n = 0; n < positionSpan.Length; n++)
        {
            var compIndex   = positionSpan[n];
        //  var relation    = RelationUtils<TRelationComponent, TValue>.GetRelationValue(components[compIndex]);
            var relation    = components[compIndex].GetRelation(); // no boxing
            if (!EqualityComparer<TValue>.Default.Equals(relation, value)) {
                continue;
            }
            RemoveRelation(id, compIndex, positions, n);
            return true;
        }
        return false;
    }
    
    // non generic
    private void RemoveRelation(int id, int compIndex, IdArray positions, int positionIndex)
    {
        Archetype.MoveLastComponentsTo(archetype, compIndex);
        if (positions.count == 1) {
            entityMap.Remove(id);
            return;
        }
        positions.RemoveAt(positionIndex, idHeap);
        entityMap[id] = positions;
    }
    #endregion
}
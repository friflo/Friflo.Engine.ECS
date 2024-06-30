// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Index;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;

internal abstract class EntityRelations
{
    internal  readonly  Archetype                   archetype;
    internal  readonly  Dictionary<int, IdArray>    entityMap   = new ();
    internal  readonly  IdArrayHeap                 idHeap      = new();
    internal  readonly  StructHeap                  heap;
    private   readonly  int                         indexBit;
    
    internal EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap) {
        this.archetype  = archetype;
        this.heap       = heap;
        var types       = new ComponentTypes(componentType);
        indexBit        = (int)types.bitSet.l0;
    }
    
    protected abstract bool         AddComponent<TComponent>(int id, TComponent component) where TComponent : struct, IComponent;
    internal  abstract IComponent   GetRelation             (int id, int index);
    
    internal static bool AddRelation<TComponent>(EntityStoreBase store, int id, TComponent component)
        where TComponent : struct, IComponent
    {
        var relations = GetEntityRelations(store, StructInfo<TComponent>.Index);
        return relations.AddComponent(id, component);
    }
    
    internal static bool RemoveRelation<TComponent, TValue>(EntityStoreBase store, int id, TValue value)
        where TComponent : struct, IRelationComponent<TValue>
    {
        var relations = (EntityRelations<TValue>)GetEntityRelations(store, StructInfo<TComponent>.Index);
        return relations.RemoveRelation(id, value);
    }
    
    internal int GetRelationCount  (Entity entity) {
        entityMap.TryGetValue(entity.Id, out var array);
        return array.count;
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
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2077", Justification = "TODO")] // TODO
    private static EntityRelations GetEntityRelations(EntityStoreBase store, int structIndex)
    {
        var relations = store.relationsMap[structIndex];
        if (relations != null) {
            return relations;
        }
        var componentType   = EntityStoreBase.Static.EntitySchema.components[structIndex];
        var heap            = componentType.CreateHeap();
        var config          = EntityStoreBase.GetArchetypeConfig(store);
        var archetype       = new Archetype(config, heap);
        var obj             = Activator.CreateInstance(componentType.RelationType, componentType, archetype, heap);
        return store.relationsMap[structIndex] = (EntityRelations)obj;
    //  return store.relationsMap[structIndex] = new RelationArchetype<TComponent, TValue>(archetype, heap);
    }
    
#region non generic add / remove
    internal int AddRelation(int id, IdArray positions)
    {
        if (positions.count == 0) {
            archetype.entityStore.nodes[id].indexBits |= indexBit;
        }
        int position = Archetype.AddEntity(archetype, id);
        positions.AddId(position, idHeap);
        entityMap[id] = positions;
        return position;
    }

    internal void RemoveRelation(int id, int position, IdArray positions, int positionIndex)
    {
        var type    = archetype;
        var map     = entityMap;
        
        // --- adjust position in entityMap of last component
        var lastPosition        = type.entityCount - 1;
        var lastId              = type.entityIds[lastPosition];
        map.TryGetValue(lastId, out var curPositions);
        var positionSpan        = curPositions.GetIdSpan(idHeap);
        var curPositionIndex    = positionSpan.IndexOf(lastPosition);
        curPositions.Set(curPositionIndex, position, idHeap);
        // array with length == 1 is stored in-place
        if (curPositions.count == 1) {
            map[lastId] = curPositions;
        }
        
        // --- move last relation to position of removed relation
        Archetype.MoveLastComponentsTo(type, position);
        if (positions.count == 1) {
            map.Remove(id);
            type.entityStore.nodes[id].indexBits &= ~indexBit;
            return;
        }
        positions.RemoveAt(positionIndex, idHeap);
        map[id] = positions;
    }
    #endregion
}

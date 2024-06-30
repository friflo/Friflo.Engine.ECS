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
    private   readonly  int                         indexBit;
    
    internal EntityRelations(ComponentType componentType, Archetype archetype) {
        this.archetype  = archetype;
        var types       = new ComponentTypes(componentType);
        indexBit        = (int)types.bitSet.l0;
    }
    
    protected abstract bool         AddComponent<TComponent>(int id, TComponent component) where TComponent : struct, IComponent;
    internal  abstract IComponent   GetRelation       (Entity entity, int index);
    
    internal static bool AddRelation<TComponent>(EntityStoreBase store, int id, TComponent component) where TComponent : struct, IComponent
    {
        var relation = GetRelationArchetype(store, StructInfo<TComponent>.Index);
        return relation.AddComponent(id, component);
    }
    
    internal static bool RemoveRelation<TComponent, TValue>(EntityStoreBase store, int id, TValue value)  where TComponent : struct, IRelationComponent<TValue>
    {
        var relation = (EntityRelations<TValue>)GetRelationArchetype(store, StructInfo<TComponent>.Index);
        return relation.RemoveRelation(id, value);
    }
    
    internal int GetRelationCount  (Entity entity) {
        entityMap.TryGetValue(entity.Id, out var array);
        return array.count;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2077", Justification = "TODO")] // TODO
    private static EntityRelations GetRelationArchetype(EntityStoreBase store, int structIndex)
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

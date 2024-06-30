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
    public    override  string                      ToString()          => $"relation count: {archetype.Count}";

    internal  readonly  Archetype                   archetype;
    internal  readonly  Dictionary<int, IdArray>    relationPositions   = new ();
    internal  readonly  IdArrayHeap                 idHeap              = new();
    internal  readonly  StructHeap                  heap;
    private   readonly  int                         indexBit;
    
    internal EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap) {
        this.archetype  = archetype;
        this.heap       = heap;
        var types       = new ComponentTypes(componentType);
        indexBit        = (int)types.bitSet.l0;
    }
    
    protected abstract bool         AddComponent<TComponent>(int id, TComponent component) where TComponent : struct, IComponent;
    internal  abstract IComponent   GetRelationAt           (int id, int index);       
    
    internal static bool AddRelation<TComponent>(EntityStoreBase store, int id, TComponent component)
        where TComponent : struct, IComponent
    {
        var relations = GetEntityRelations(store, StructInfo<TComponent>.Index);
        return relations.AddComponent(id, component);
    }
    
    internal static bool RemoveRelation<TComponent, TKey>(EntityStoreBase store, int id, TKey key)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var relations = (EntityRelations<TKey>)GetEntityRelations(store, StructInfo<TComponent>.Index);
        return relations.RemoveRelation(id, key);
    }
    
    internal int GetRelationCount(Entity entity) {
        relationPositions.TryGetValue(entity.Id, out var positions);
        return positions.count;
    }
    
    internal static ref TComponent GetRelation<TComponent, TKey>(Entity entity, TKey key)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var relations = (EntityRelations<TComponent,TKey>)entity.Store.relationsMap[StructInfo<TComponent>.Index];
        // throw NullReferenceException if relations not found
        return ref relations.GetRelation<TComponent>(entity.Id, key);
    }
    
    internal static bool TryGetRelation<TComponent, TKey>(Entity entity, TKey key, out TComponent value)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var relations = (EntityRelations<TComponent,TKey>)entity.Store.relationsMap[StructInfo<TComponent>.Index];
        if (relations == null) {
            value = default;    
            return false;
        }
        return relations.TryGetRelation(entity.Id, key, out value);
    }
    
    internal static RelationComponents<TComponent> GetRelations<TComponent>(Entity entity)
        where TComponent : struct, IRelationComponent
    {
        var relations = entity.Store.relationsMap[StructInfo<TComponent>.Index];
        if (relations == null) {
            return default;
        }
        relations.relationPositions.TryGetValue(entity.Id, out var positions);
        var count           = positions.count;
        var componentHeap   = (StructHeap<TComponent>)relations.heap;
        switch (count) {
            case 0: return  new RelationComponents<TComponent>();
            case 1: return  new RelationComponents<TComponent>(componentHeap.components, positions.start);
        }
        var poolIndex       = IdArrayHeap.PoolIndex(count);
        var poolPositions   = relations.idHeap.GetPool(poolIndex).Ids;
        return new RelationComponents<TComponent>(componentHeap.components, poolPositions, positions.start, positions.count);
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
    //  return store.relationsMap[structIndex] = new RelationArchetype<TComponent, TKey>(archetype, heap);
    }
    
#region non generic add / remove position
    internal int SetRelationPositions(int id, IdArray positions)
    {
        if (positions.count == 0) {
            archetype.entityStore.nodes[id].indexBits |= indexBit;
        }
        int position = Archetype.AddEntity(archetype, id);
        positions.AddId(position, idHeap);
        relationPositions[id] = positions;
        return position;
    }

    internal void RemoveRelationPosition(int id, int position, IdArray positions, int positionIndex)
    {
        var type    = archetype;
        var map     = relationPositions;
        
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

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
    internal  readonly  EntityStore                 store;
    /// map: entity id -> relation positions
    internal  readonly  Dictionary<int, IdArray>    relationPositions   = new();
    internal  readonly  IdArrayHeap                 idHeap              = new();
    internal  readonly  StructHeap                  heap;
    private   readonly  int                         indexBit;
    
    internal EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap) {
        this.archetype  = archetype;
        store           = archetype.entityStore;
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
        var relations = (EntityRelations<TComponent,TKey>)GetEntityRelations(store, StructInfo<TComponent>.Index);
        return relations.RemoveRelation(id, key);
    }
    
    internal int GetRelationCount(Entity entity) {
        relationPositions.TryGetValue(entity.Id, out var positions);
        return positions.count;
    }
    
    internal static KeyNotFoundException KeyNotFoundException(int id, object key)
    {
        return new KeyNotFoundException($"relation not found. key '{key}' id: {id}");        
    }
    
    internal static ref TComponent GetRelation<TComponent, TKey>(EntityStore store, int id, TKey key)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var relations = (EntityRelations<TComponent,TKey>)store.extension.relationsMap[StructInfo<TComponent>.Index];
        if (relations == null) {
            throw KeyNotFoundException(id, key);
        }
        return ref relations.GetRelation<TComponent>(id, key);
    }
    
    internal static bool TryGetRelation<TComponent, TKey>(EntityStore store, int id, TKey key, out TComponent value)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var relations = (EntityRelations<TComponent,TKey>)store.extension.relationsMap[StructInfo<TComponent>.Index];
        if (relations == null) {
            value = default;    
            return false;
        }
        return relations.TryGetRelation(id, key, out value);
    }
    
    internal static RelationComponents<TComponent> GetRelations<TComponent>(EntityStore store, int id)
        where TComponent : struct, IRelationComponent
    {
        var relations = store.extension.relationsMap[StructInfo<TComponent>.Index];
        if (relations == null) {
            return default;
        }
        relations.relationPositions.TryGetValue(id, out var positions);
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
    internal static EntityRelations GetEntityRelations(EntityStoreBase store, int structIndex)
    {
        var relationsMap    = ((EntityStore)store).extension.relationsMap;
        var relations       = relationsMap[structIndex];
        if (relations != null) {
            return relations;
        }
        var componentType   = EntityStoreBase.Static.EntitySchema.components[structIndex];
        var heap            = componentType.CreateHeap();
        var config          = EntityStoreBase.GetArchetypeConfig(store);
        var archetype       = new Archetype(config, heap);
        var obj             = Activator.CreateInstance(componentType.RelationType, componentType, archetype, heap);
        return relationsMap[structIndex] = (EntityRelations)obj;
    //  return store.relationsMap[structIndex] = new RelationArchetype<TComponent, TKey>(archetype, heap);
    }
    
#region non generic add / remove position
    
    internal int AddEntityRelation(int id, IdArray positions)
    {
        if (positions.count == 0) {
            store.nodes[id].indexBits |= indexBit;
        }
        int position = Archetype.AddEntity(archetype, id);
        positions.AddId(position, idHeap);
        relationPositions[id] = positions;
        return position;
    }
    
    internal IdArray RemoveEntityRelation(int id, int position, IdArray positions, int positionIndex)
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
            store.nodes[id].indexBits &= ~indexBit;
            return default;
        }
        positions.RemoveAt(positionIndex, idHeap);
        map[id] = positions;
        return positions;
    }
    
    internal void RemoveEntity (int id)
    {
        relationPositions.TryGetValue(id, out var positions);
        while (positions.count > 0) {
            var lastIndex   = positions.count - 1;
            int position    = positions.Get(lastIndex, idHeap);
            positions       = RemoveEntityRelation(id, position, positions, lastIndex);
        }
    }
    #endregion
}

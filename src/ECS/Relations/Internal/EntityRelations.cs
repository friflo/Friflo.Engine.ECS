// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Collections;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;

internal abstract class EntityRelations
{
    internal            int                         Count       => archetype.Count;
    public    override  string                      ToString()  => $"relation count: {archetype.Count}";

#region fields
    /// Single <see cref="Archetype"/> containing all relations of a specific <see cref="IRelationComponent{TKey}"/>
    internal  readonly  Archetype                   archetype;
    
    /// Single <see cref="StructHeap"/> stored in the <see cref="archetype"/>.
    internal  readonly  StructHeap                  heap;
    
    /// map:  entity id  ->  relation component positions in <see cref="archetype"/>
    internal  readonly  Dictionary<int, IdArray>    positionMap = new();
    
    internal  readonly  EntityStore                 store;
    internal  readonly  IdArrayHeap                 idHeap      = new();
    internal  readonly  int                         relationBit;
    
    //  --- link relations
    /// map:  indexed / linked entity (id)  ->  entities (ids) containing a <see cref="ILinkRelation"/> referencing the indexed / linked entity.
    internal            Dictionary<int, IdArray>    linkEntityMap;
    
    internal            IdArrayHeap                 linkIdsHeap;
    #endregion
    
#region general
    internal EntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap) {
        this.archetype  = archetype;
        store           = archetype.entityStore;
        this.heap       = heap;
        var types       = new ComponentTypes(componentType);
        relationBit     = (int)types.bitSet.l0;
    }
    
    internal  abstract bool             AddComponent<TComponent>     (int id, in TComponent component) where TComponent : struct, IComponent;
    internal  abstract IComponent       GetRelationAt                (int id, int index);
    internal  virtual  ref TComponent   GetEntityRelation<TComponent>(int id, int target)              where TComponent : struct, IComponent   => throw new InvalidOperationException($"type: {GetType().Name}");
    internal  virtual  void             AddIncomingRelations         (int target, List<EntityLink> result)                                     => throw new InvalidOperationException($"type: {GetType().Name}");
    internal  virtual  void             RemoveLinksWithTarget        (int targetId)                                                            => throw new InvalidOperationException($"type: {GetType().Name}");
    
    internal static KeyNotFoundException KeyNotFoundException(int id, object key)
    {
        return new KeyNotFoundException($"relation not found. key '{key}' id: {id}");        
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2077", Justification = "TODO")] // TODO
    internal static EntityRelations GetEntityRelations(EntityStoreBase store, int structIndex)
    {
        var relationsMap    = ((EntityStore)store).extension.relationsMap ??= CreateRelationsMap();
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
    
    private static EntityRelations[] CreateRelationsMap() {
        var schema = EntityStoreBase.Static.EntitySchema;
        return new EntityRelations[schema.maxIndexedStructIndex];
    }
    #endregion
    
#region query
    internal int GetRelationCount(Entity entity) {
        positionMap.TryGetValue(entity.Id, out var positions);
        return positions.count;
    }
    
    internal static RelationComponents<TComponent> GetRelations<TComponent>(EntityStore store, int id)
        where TComponent : struct, IRelationComponent
    {
        var relations = store.extension.relationsMap?[StructInfo<TComponent>.Index];
        if (relations == null) {
            return default;
        }
        relations.positionMap.TryGetValue(id, out var positions);
        int count       = positions.count;
        var components  = ((StructHeap<TComponent>)relations.heap).components;
        switch (count) {
            case 0: return  new RelationComponents<TComponent>();
            case 1: return  new RelationComponents<TComponent>(components, positions.start);
        }
        var poolPositions = IdArrayPool.GetIds(count, relations.idHeap);
        return new RelationComponents<TComponent>(components, poolPositions, positions.start, positions.count);
    }
    
    internal static ref TComponent GetRelation<TComponent, TKey>(EntityStore store, int id, TKey key)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var relations = (EntityRelations<TComponent,TKey>)store.extension.relationsMap?[StructInfo<TComponent>.Index];
        if (relations == null) {
            throw KeyNotFoundException(id, key);
        }
        return ref relations.GetRelation<TComponent>(id, key);
    }
    
    internal static bool TryGetRelation<TComponent, TKey>(EntityStore store, int id, TKey key, out TComponent value)
        where TComponent : struct, IRelationComponent<TKey>
    {
        var relations = (EntityRelations<TComponent,TKey>)store.extension.relationsMap?[StructInfo<TComponent>.Index];
        if (relations == null) {
            value = default;    
            return false;
        }
        return relations.TryGetRelation(id, key, out value);
    }
    
    internal void ForAllEntityRelations<TComponent>(ForEachEntity<TComponent> lambda)
        where TComponent : struct, IRelationComponent
    {
        var components  = ((StructHeap<TComponent>)heap).components;
        int count       = archetype.Count;
        var ids         = archetype.entityIds;
        var entityStore = store;
        for (int n = 0; n < count; n++) {
            lambda(ref components[n], new Entity(entityStore, ids[n]));
        }
    }
    
    internal (Entities entities, Chunk<TComponent> relations) GetAllEntityRelations<TComponent>()
        where TComponent : struct, IRelationComponent
    {
        int count       = archetype.Count;
        var entities    = new Entities(store, archetype.entityIds, 0, count);
        var components  = ((StructHeap<TComponent>)heap).components;
        var chunk       = new Chunk<TComponent>(components, count, 0);
        return (entities, chunk);
    }
    
    internal static Entities GetIncomingLinkRelations(EntityStore store, int target, int structIndex, out EntityRelations relations)
    {
        relations = store.extension.relationsMap?[structIndex];
        if (relations == null) {
            return default;
        }
        relations.linkEntityMap.TryGetValue(target, out var ids);
        return relations.linkIdsHeap.GetEntities(store, ids);
    }
    
    internal int CountIncomingLinkRelations(int target)
    {
        linkEntityMap.TryGetValue(target, out var sourceIds);
        return sourceIds.Count;
    }
    #endregion
    
#region mutation
    internal static bool AddRelation<TComponent>(EntityStoreBase store, int id, in TComponent component)
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
    
    protected int AddEntityRelation(int id, IdArray positions)
    {
        if (positions.count == 0) {
            store.nodes[id].isOwner |= relationBit;
        }
        int position = Archetype.AddEntity(archetype, id);
        positions.Add(position, idHeap);
        positionMap[id] = positions;
        return position;
    }
    
    /// Executes in O(M)  M: max(number of entity relations)
    protected IdArray RemoveEntityRelation(int id, int position, IdArray positions, int positionIndex)
    {
        var type        = archetype;
        var map         = positionMap;
        var localIdHeap = idHeap;
        
        // --- adjust position in entityMap of last component
        int lastPosition        = type.entityCount - 1;
        int lastId              = type.entityIds[lastPosition];
        map.TryGetValue(lastId, out var curPositions);
        var positionSpan        = curPositions.GetSpan(localIdHeap, store);
        int curPositionIndex    = positionSpan.IndexOf(lastPosition);
        curPositions.SetAt(curPositionIndex, position, localIdHeap);
        // array with length == 1 is stored in-place
        if (curPositions.count == 1) {
            map[lastId] = curPositions;
        }
        
        // --- move last relation to position of removed relation
        Archetype.MoveLastComponentsTo(type, position, false);
        if (positions.count == 1) {
            map.Remove(id);
            store.nodes[id].isOwner &= ~relationBit;
            return default;
        }
        positions.RemoveAt(positionIndex, localIdHeap);
        map[id] = positions;
        return positions;
    }
    
    /// remove all entity relations
    internal void RemoveEntityRelations (int id)
    {
        positionMap.TryGetValue(id, out var positions);
        while (positions.count > 0) {
            var lastIndex   = positions.count - 1;
            int position    = positions.GetAt(lastIndex, idHeap);
            positions       = RemoveEntityRelation(id, position, positions, lastIndex);
        }
    }
    #endregion
}

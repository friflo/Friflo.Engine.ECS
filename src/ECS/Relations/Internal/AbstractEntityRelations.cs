// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS.Collections;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;

internal delegate AbstractEntityRelations CreateEntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap);

internal abstract class AbstractEntityRelations
{
    internal            int                         version;
    internal            int                         Count       => archetype.Count;
    public    override  string                      ToString()  => $"relation count: {archetype.Count}";

    internal static readonly Dictionary<Type, CreateEntityRelations> CreateEntityRelationsNativeAot = new ();
#region fields
    /// Single <see cref="Archetype"/> containing all relations of a specific <see cref="IRelation{TKey}"/>
    internal  readonly  Archetype                   archetype;
    
    /// Single <see cref="StructHeap"/> stored in the <see cref="archetype"/>.
    internal  readonly  StructHeap                  heap;
    
    /// map:  entity id  ->  relation positions in <see cref="archetype"/>
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
    internal AbstractEntityRelations(ComponentType componentType, Archetype archetype, StructHeap heap) {
        this.archetype  = archetype;
        store           = archetype.entityStore;
        this.heap       = heap;
        var types       = new ComponentTypes(componentType);
        relationBit     = (int)types.bitSet.l0;
    }
    
    internal  abstract bool                 AddRelation<TRelation>       (int id, in TRelation relation) where TRelation : struct, IRelation;
    internal  abstract IRelation            GetRelationAt                (int id, int index);
    internal  virtual  ref TRelation        GetEntityRelation<TRelation >(int id, int target)              where TRelation  : struct   => throw new InvalidOperationException($"type: {GetType().Name}");
    internal  virtual  void                 AddIncomingRelations         (int target, List<EntityLink> result)                         => throw new InvalidOperationException($"type: {GetType().Name}");
    internal  virtual  void                 RemoveLinksWithTarget        (int targetId)                                                => throw new InvalidOperationException($"type: {GetType().Name}");
    
    internal static KeyNotFoundException KeyNotFoundException(int id, object key)
    {
        return new KeyNotFoundException($"relation not found. key '{key}' id: {id}");        
    }
    
    internal static AbstractEntityRelations GetEntityRelations(EntityStoreBase store, int structIndex)
    {
        var relationsMap    = ((EntityStore)store).extension.relationsMap ??= CreateRelationsMap();
        var relations       = relationsMap[structIndex];
        if (relations != null) {
            return relations;
        }
        var componentType   = EntityStoreBase.Static.EntitySchema.components[structIndex];
        return relationsMap[structIndex] = CreateEntityRelations(store, componentType);
    }

    /// Call constructors of<br/>
    /// <see cref="GenericEntityRelations{TRelation,TKey}"/>
    /// <see cref="EntityLinkRelations{TRelation}"/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2080", Justification = "TODO")] // TODO
    private static AbstractEntityRelations CreateEntityRelations(EntityStoreBase store, ComponentType componentType)
    {
        var heap        = componentType.CreateHeap();
        var config      = EntityStoreBase.GetArchetypeConfig(store);
        var archetype   = new Archetype(config, heap);
        
        var flags       = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance;
        var paramTypes  = new [] { typeof(ComponentType), typeof(Archetype), typeof(StructHeap) };
        var constructor = componentType.RelationType.GetConstructor(flags, null, paramTypes, null);
        if (constructor == null) {
            // constructor is null in Native AOT
            if (!CreateEntityRelationsNativeAot.TryGetValue(componentType.Type, out var create)) {
                throw new InvalidOperationException($"Native AOT requires registration of IRelation with aot.RegisterRelation(). type: {componentType.Type}.");   
            }
            return create(componentType, archetype, heap);
        }
        var args        = new object[] { componentType, archetype, heap };
        var obj         = constructor.Invoke(args);
        return (AbstractEntityRelations)obj;
    }
    
    private static AbstractEntityRelations[] CreateRelationsMap() {
        var schema = EntityStoreBase.Static.EntitySchema;
        return new AbstractEntityRelations[schema.maxIndexedStructIndex];
    }
    #endregion
    
#region query
    internal int GetRelationCount(Entity entity) {
        positionMap.TryGetValue(entity.Id, out var positions);
        return positions.count;
    }
    
    internal static Relations<TRelation> GetRelations<TRelation>(EntityStore store, int id)
        where TRelation : struct, IRelation
    {
        var relations = GetEntityRelations(store, StructInfo<TRelation>.Index);
        relations.positionMap.TryGetValue(id, out var positions);
        int count       = positions.count;
        var components  = ((StructHeap<TRelation>)relations.heap).components;
        switch (count) {
            case 0: return  new Relations<TRelation>(relations);
            case 1: return  new Relations<TRelation>(components, positions.start, relations);
        }
        var poolPositions = IdArrayPool.GetIds(count, relations.idHeap);
        return new Relations<TRelation>(components, poolPositions, positions.start, positions.count, relations);
    }
    
    internal static ref TRelation GetRelation<TRelation, TKey>(EntityStore store, int id, TKey key)
        where TRelation : struct, IRelation<TKey>
    {
        var relations = (GenericEntityRelations<TRelation,TKey>)store.extension.relationsMap?[StructInfo<TRelation>.Index];
        if (relations == null) {
            throw KeyNotFoundException(id, key);
        }
        return ref relations.GetRelation<TRelation>(id, key);
    }
    
    internal static bool TryGetRelation<TRelation, TKey>(EntityStore store, int id, TKey key, out TRelation value)
        where TRelation : struct, IRelation<TKey>
    {
        var relations = (GenericEntityRelations<TRelation,TKey>)store.extension.relationsMap?[StructInfo<TRelation>.Index];
        if (relations == null) {
            value = default;    
            return false;
        }
        return relations.TryGetRelation(id, key, out value);
    }
    
    internal void ForAllEntityRelations<TRelation>(ForEachEntity<TRelation> lambda)
        where TRelation : struct, IRelation
    {
        var components  = ((StructHeap<TRelation>)heap).components;
        int count       = archetype.Count;
        var ids         = archetype.entityIds;
        var entityStore = store;
        for (int n = 0; n < count; n++) {
            lambda(ref components[n], new Entity(entityStore, ids[n]));
        }
    }
    
    internal (Entities entities, Chunk<TRelation> relations) GetAllEntityRelations<TRelation>()
        where TRelation : struct, IRelation
    {
        int count       = archetype.Count;
        var entities    = new Entities(store, archetype.entityIds, 0, count);
        var components  = ((StructHeap<TRelation>)heap).components;
        var chunk       = new Chunk<TRelation>(components, count, 0);
        return (entities, chunk);
    }
    
    internal static Entities GetIncomingLinkRelations(EntityStore store, int target, int structIndex, out AbstractEntityRelations relations)
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
    internal static bool AddRelation<TRelation>(EntityStoreBase store, int id, in TRelation relation)
        where TRelation : struct, IRelation
    {
        var relations = GetEntityRelations(store, StructInfo<TRelation>.Index);
        relations.version++;
        return relations.AddRelation(id, relation);
    }
        
    internal static bool RemoveRelation<TRelation, TKey>(EntityStoreBase store, int id, TKey key)
        where TRelation : struct, IRelation<TKey>
    {
        var relations = (GenericEntityRelations<TRelation,TKey>)GetEntityRelations(store, StructInfo<TRelation>.Index);
        relations.version++;
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
    internal virtual void RemoveEntityRelations (int id)
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

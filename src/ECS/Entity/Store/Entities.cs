// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Engine.ECS.StoreOwnership;
using static Friflo.Engine.ECS.TreeMembership;

// ReSharper disable UseNullPropagation
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// This file contains implementation specific for storing Entity's.
// The reason to separate handling of Entity's is to enable 'entity / component support' without Entity's.
// EntityStore remarks.
public partial class EntityStore
{
    /// <summary>
    /// Return the <see cref="EntitySchema"/> containing all available
    /// <see cref="IComponent"/>, <see cref="ITag"/> and <see cref="Script"/> types.
    /// </summary>
    public static     EntitySchema         GetEntitySchema()=> Static.EntitySchema;
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> in the entity store.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/entity">Example.</a>
    /// </summary>
    /// <returns>An <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public Entity CreateEntity()
    {
        var id = NewId();
        CreateEntityInternal(defaultArchetype, id, out var revision);
        var entity = new Entity(this, id, revision);
        
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(entity);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed <paramref name="id"/> in the entity store.
    /// </summary>
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public Entity CreateEntity(int id)
    {
        CheckEntityId(id);
        CreateEntityInternal(defaultArchetype, id, out var revision);
        var entity = new Entity(this, id, revision); 
        
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(entity);
        return entity;
    }
    
    
    internal void CheckEntityId(int id)
    {
        if (id < Static.MinNodeId) {
            throw InvalidEntityIdException(id, nameof(id));
        }
        if (id < nodes.Length && nodes[id].archetype != null) {
            throw IdAlreadyInUseException(id, nameof(id));
        }
    }
    
    /// <returns> compIndex to access <see cref="StructHeap{T}.components"/> </returns>
    internal int CreateEntityInternal(Archetype archetype, int id, out short revision)
    {
        // Assign pid: assign pid if intern.pidType == PidType.RandomPids
        if (intern.pidType == PidType.RandomPids) {
            extension.GenerateRandomPidForId(id);
        }
        EnsureNodesLength(id + 1);
        return CreateEntityNode(archetype, id, out revision);
    }
    
    private static void RemoveIndexedComponents(Entity entity, long removedIndexTypes) {
        var removeTypes = new ComponentTypes();
        removeTypes.bitSet.l0 = removedIndexTypes;
        var heapMap = entity.archetype.heapMap;
        foreach (var removeType in removeTypes) {
            heapMap[removeType.StructIndex].RemoveIndex(entity);
        }
    }
    
    /// <summary>
    /// Copy all components, tags and scripts of the <paramref name="source"/> entity to the <paramref name="target"/> entity.<br/>
    /// The <paramref name="source"/> and <paramref name="target"/> entities can be in the same or different stores.
    /// </summary>
    /// <remarks>
    /// Child entities of the passed <paramref name="source"/> are not copied to the <paramref name="target"/> entity.<br/>
    /// If doing this these child entities would be children of both entities.
    /// </remarks>
    public static void CopyEntity(Entity source, Entity target)
    {
        var sourceArch      = source.GetArchetype() ?? throw EntityArgumentNullException(source, nameof(source));
        var curTargetArch   = target.GetArchetype() ?? throw EntityArgumentNullException(target, nameof(target));
        var targetStore     = target.store;
        if (source.store == targetStore) {
            if (targetStore.internBase.activeQueryLoops > 0) {
                throw StructuralChangeWithinQueryLoop();
            }
        }
        var targetArch = targetStore.GetArchetype(sourceArch.componentTypes, sourceArch.Tags);
        if (targetArch != curTargetArch) {
            var removedIndexTypes = (curTargetArch.componentTypes.bitSet.l0 & ~targetArch.componentTypes.bitSet.l0) & EntityExtensions.IndexTypesMask;
            // --- remove indexes of removed indexed components
            if (removedIndexTypes != 0) {
                RemoveIndexedComponents(target, removedIndexTypes);
            }
            // --- move entity targetArch 
            ref var node    = ref targetStore.nodes[target.Id];
            node.compIndex  = Archetype.MoveEntityTo(curTargetArch, target.Id, node.compIndex, targetArch);
            node.archetype  = targetArch;
        }
        // bit == 1: update component index.    bit == 0: add component index
        var updateIndexTypes    = curTargetArch.componentTypes.bitSet.l0 & targetArch.componentTypes.bitSet.l0;
        var context             = new CopyContext(source, target);
        Archetype.CopyComponents(sourceArch, targetArch, context, updateIndexTypes);
        
        targetStore.CloneScrips(source, target);
    }
    
    /// <summary>
    /// Create and return a clone of the passed <paramref name="entity"/> in the store.
    /// </summary>
    /// <remarks>
    /// Child entities of the passed <paramref name="entity"/> are not copied to the cloned entity.<br/>
    /// If doing this these child entities would be children of the passed entity <b>and</b> the clone.
    /// </remarks>
    public Entity CloneEntity(Entity entity)
    {
        var archetype   = entity.GetArchetype() ?? throw EntityArgumentNullException(entity, nameof(entity));
        if (this != entity.store)                  throw InvalidStoreException(nameof(entity));
        var id          = NewId();
        CreateEntityInternal(archetype, id, out var revision);
        var clone       = new Entity(this, id, revision);
        
        // var isBlittable = IsBlittable(entity);
        // if (true) {
        
        var context = new CopyContext(entity, clone);
        Archetype.CopyComponents(archetype, archetype, context, 0);
        
        CloneScrips(entity, clone);
        
        /* if (clone.HasComponent<TreeNode>()) {
            clone.GetComponent<TreeNode>() = default;   // clear child ids. See child entities note in remarks.
        } */
        /* keep old implementation using JSON serialization for reference
        } else {
            // --- serialize entity
            var converter       = EntityConverter.Default;
            converter.EntityToDataEntity(entity, dataBuffer, false);
            
            // --- deserialize DataEntity
            dataBuffer.pid      = IdToPid(clone.Id);
            dataBuffer.children = null;                     // clear children. See child entities note in remarks.
            // convert will use entity created above
            converter.DataEntityToEntity(dataBuffer, this, out string error); // error == null. No possibility for mapping errors
            AssertNoError(error);
        } */
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(clone);
        return clone;
    }
    
    private void CloneScrips(Entity entity, Entity clone)
    {
        var scriptTypeByType    = Static.EntitySchema.ScriptTypeByType;
        foreach (var script in entity.Scripts) {
            var scriptType      = scriptTypeByType[script.GetType()];
            var scriptClone     = scriptType.CloneScript(script);
            scriptClone.entity  = clone;
            extension.AddScript(clone, scriptClone, scriptType);
        }
    }
    
    // ReSharper disable once UnusedMember.Local
    [ExcludeFromCodeCoverage]
    private static void AssertNoError(string error) {
        if (error == null) {
            return;
        }
        throw new InvalidOperationException($"unexpected error: {error}");
    }
    
    // ReSharper disable once UnusedMember.Local
    [ExcludeFromCodeCoverage] // unused - method obsolete
    private static bool IsBlittable(Entity original)
    {
        foreach (var componentType in original.Archetype.componentTypes)
        {
            if (!componentType.IsBlittable) {
                return false;
            }
        }
        var scriptTypeByType    = Static.EntitySchema.ScriptTypeByType;
        var scripts             = original.Scripts;
        foreach (var script in scripts)
        {
            var scriptType = scriptTypeByType[script.GetType()];
            if (!scriptType.IsBlittable) {
                return false;
            }    
        }
        return true;
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private void AssertIdInNodes(int id) {
        if (id < nodes.Length) {
            return;
        }
        throw new InvalidOperationException("expect id < nodes.length");
    }
    
    /// <summary> expect <see cref="EntityStore.nodes"/> Length > id </summary>
    /// <returns> compIndex to access <see cref="StructHeap{T}.components"/> </returns>
    private int CreateEntityNode(Archetype archetype, int id, out short revision)
    {
        AssertIdInNodes(id);
        ref var node = ref nodes[id];
        revision     = node.revision;
        if (node.archetype != null) {
            return node.compIndex;
        }
        entityCount++;
        node.compIndex  = Archetype.AddEntity(archetype, id);
        node.archetype  = archetype;
        // node.flags      = Created;
        return node.compIndex;
    }
    
    internal void CreateEntityNodes(Archetype archetype, int count)
    {
        archetype.EnsureCapacity(count);
        int compIndexStart  = archetype.entityCount;
        var entityIds       = archetype.entityIds;
        NewIds(entityIds, compIndexStart, count, archetype);
        EnsureNodesLength(intern.sequenceId + 1); // may resize nodes
        var localNodes      = nodes;
        var maxIndex        = compIndexStart + count;
        for (int index = compIndexStart; index < maxIndex; index++)
        {
            var id          = entityIds[index];
            ref var node    = ref localNodes[id];
            node.compIndex  = index;
            node.archetype  = archetype;
            // node.flags      = Created;
        }
        if (intern.pidType == PidType.RandomPids) {
            for (int index = compIndexStart; index < maxIndex; index++) {
                extension.GenerateRandomPidForId(entityIds[index]);
            }
        }
        entityCount             += count;
        archetype.entityCount   += count;
    }
    
    private void SetRecycleIds (bool enable) {
        if (enable == recycleIds) {
            return;
        }
        recycleIds = enable;
        if (!enable) {
            intern.recycleIds.Clear();
        }
    }
    
    /// <summary>
    /// Set the passed <paramref name="entity"/> as the <see cref="StoreRoot"/> entity.
    /// </summary>
    public void SetStoreRoot(Entity entity) {
        var store = entity.GetStore() ?? throw EntityArgumentNullException(entity, nameof(entity));
        if (this != store) {
            throw InvalidStoreException(nameof(entity));
        }
        SetStoreRootEntity(entity);
    }
    
    private QueryEntities GetEntities() {
        var query = intern.entityQuery ??= new ArchetypeQuery(this);
        return query.Entities;
    }
    
    internal void CreateEntityEvents(Entities entities)
    {
        var create = intern.entityCreate;
        if (create == null) {
            return;
        }
        foreach (var entity in entities) {
            create(new EntityCreate(entity));
        }
    }
    
    internal void CreateEntityEvent(Entity entity)
    {
        if (intern.entityCreate == null) {
            return;
        }
        intern.entityCreate(new EntityCreate(entity));
    }
    
    internal void DeleteEntityEvent(Entity entity)
    {
        if (intern.entityDelete == null) {
            return;
        }
        intern.entityDelete(new EntityDelete(entity));
    }
}


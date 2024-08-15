// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Serialize;
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
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#entity">Example.</a>
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
        var id          = NewId();
        CreateEntityInternal(archetype, id, out var revision);
        var clone       = new Entity(this, id, revision);
        
        var isBlittable = IsBlittable(entity);

        // todo optimize - serialize / deserialize only non blittable components and scripts
        if (isBlittable) {
            var scriptTypeByType    = Static.EntitySchema.ScriptTypeByType;
            // CopyComponents() must be used only in case all component types are blittable
            Archetype.CopyComponents(archetype, entity.compIndex, clone.compIndex);
            if (clone.HasComponent<TreeNode>()) {
                clone.GetComponent<TreeNode>() = default;   // clear child ids. See child entities note in remarks.
            }
            // --- clone scripts
            foreach (var script in entity.Scripts) {
                var scriptType      = scriptTypeByType[script.GetType()];
                var scriptClone     = scriptType.CloneScript(script);
                scriptClone.entity  = clone;
                extension.AddScript(clone, scriptClone, scriptType);
            }
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
        }
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(clone);
        return clone;
    }
    
    [ExcludeFromCodeCoverage]
    private static void AssertNoError(string error) {
        if (error == null) {
            return;
        }
        throw new InvalidOperationException($"unexpected error: {error}");
    }
    
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
        revision++;
        node.compIndex  = Archetype.AddEntity(archetype, id);
        node.archetype  = archetype;
        node.revision   = revision;   
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
            node.revision++;
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


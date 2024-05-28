// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Serialize;
using static Friflo.Engine.ECS.NodeFlags;
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
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General#entity">Example.</a>
    /// </summary>
    /// <returns>An <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public Entity CreateEntity()
    {
        var id = NewId();
        CreateEntityInternal(defaultArchetype, id);
        var entity = new Entity(this, id);
        
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
        CreateEntityInternal(defaultArchetype, id);
        var entity = new Entity(this, id); 
        
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(entity);
        return entity;
    }
    
    
    internal void CheckEntityId(int id)
    {
        if (id < Static.MinNodeId) {
            throw InvalidEntityIdException(id, nameof(id));
        }
        if (id < nodes.Length && nodes[id].Is(Created)) {
            throw IdAlreadyInUseException(id, nameof(id));
        }
    }
    
    /// <returns> compIndex to access <see cref="StructHeap{T}.components"/> </returns>
    internal int CreateEntityInternal(Archetype archetype, int id)
    {
        EnsureNodesLength(id + 1);
        GeneratePid(id);
        return CreateEntityNode(archetype, id);
    }
    
    /// <summary>
    /// Create and return a clone of the passed <paramref name="entity"/> in the store.
    /// </summary>
    /// <returns></returns>
    public Entity CloneEntity(Entity entity)
    {
        var id          = NewId();
        var archetype   = entity.archetype;
        CreateEntityInternal(archetype, id);
        var clone       = new Entity(this, id);
        
        var isBlittable = IsBlittable(entity);

        // todo optimize - serialize / deserialize only non blittable components and scripts
        if (isBlittable) {
            var scriptTypeByType    = Static.EntitySchema.ScriptTypeByType;
            // CopyComponents() must be used only in case all component types are blittable
            Archetype.CopyComponents(archetype, entity.compIndex, clone.compIndex);
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
            dataBuffer.children = null; // child ids are not copied. If doing this these children would have two parents.
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
    private int CreateEntityNode(Archetype archetype, int id)
    {
        AssertIdInNodes(id);
        ref var node = ref nodes[id];
        if ((node.flags & Created) != 0) {
            return node.compIndex;
        }
        entityCount++;
        if (nodesMaxId < id) {
            nodesMaxId = id;
        }
        node.compIndex      = Archetype.AddEntity(archetype, id);
        node.archetype      = archetype;
        node.flags          = Created;
        return node.compIndex;
    }
    
    /// <summary> Note!  Sync implementation with <see cref="NewId"/>. </summary>
    internal void CreateEntityNodes(Archetype archetype, int count)
    {
        var sequenceId      = intern.sequenceId;
        int maxId           = sequenceId + count; 
        EnsureNodesLength(maxId + 1);
        archetype.EnsureCapacity(count);
        int compIndexStart  = archetype.entityCount;
        var entityIds       = archetype.entityIds;
        var localNodes      = nodes;
        var max             = localNodes.Length;
        for (int n = 0; n < count; n++)
        {
            var id = ++sequenceId;
            for (; id < max;)
            {
                if ((localNodes[id].flags & Created) != 0) {
                    id = ++sequenceId;
                    continue;
                }
                break;
            }
            AssertIdInNodes(id);
            ref var node = ref localNodes[id];
            if ((node.flags & Created) != 0) {
                continue;
            }
            var compIndex           = compIndexStart + n; 
            entityIds[compIndex]    = id;  // Archetype.AddEntity(archetype, id);
            node.compIndex          = compIndex;
            node.archetype          = archetype;
            node.flags              = Created;
        }
        intern.sequenceId = sequenceId;
        if (nodesMaxId < maxId) {
            nodesMaxId = maxId;
        }
        entityCount             += count;
        archetype.entityCount   += count;
    }
    
    /// <summary>
    /// Set the passed <paramref name="entity"/> as the <see cref="StoreRoot"/> entity.
    /// </summary>
    public void SetStoreRoot(Entity entity) {
        if (entity.IsNull) {
            throw new ArgumentNullException(nameof(entity));
        }
        if (this != entity.archetype.store) {
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


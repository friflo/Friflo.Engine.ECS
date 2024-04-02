﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Serialize;
using static Friflo.Engine.ECS.NodeFlags;

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
        var pid = GeneratePid(id);
        return CreateEntityNode(archetype, id, pid);
    }
    
    /// <summary>
    /// Create and return a clone of the of the passed <paramref name="entity"/> in the store.
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
                AddScript(clone, scriptClone, scriptType);
            }
        } else {
            // --- serialize entity
            var converter       = EntityConverter.Default;
            converter.EntityToDataEntity(entity, dataBuffer, false);
            
            // --- deserialize DataEntity
            dataBuffer.pid      = IdToPid(clone.Id);
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
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private static void AssertPid(long pid, long expected) {
        if (expected == pid) {
            return;
        }
        throw new InvalidOperationException($"invalid pid. expected: {expected}, was: {pid}");
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage] // assert invariant
    private static void AssertPid0(long pid, long expected) {
        if (pid == 0 || pid == expected) {
            return;
        }
        throw new InvalidOperationException($"invalid pid. expected: 0 or {expected}, was: {pid}");
    }

    /// <summary> expect <see cref="EntityStore.nodes"/> Length > id </summary>
    /// <returns> compIndex to access <see cref="StructHeap{T}.components"/> </returns>
    private int CreateEntityNode(Archetype archetype, int id, long pid)
    {
        AssertIdInNodes(id);
        ref var node = ref nodes[id];
        if ((node.flags & Created) != 0) {
            AssertPid(node.pid, pid);
            return node.compIndex;
        }
        entityCount++;
        if (nodesMaxId < id) {
            nodesMaxId = id;
        }
        AssertPid0(node.pid, pid);
        node.compIndex      = Archetype.AddEntity(archetype, id);
        node.archetype      = archetype;
        node.pid            = pid;
        node.scriptIndex    = EntityUtils.NoScripts;
        // node.parentId    = Static.NoParentId;     // Is not set. A previous parent node has .parentId already set.
        node.childIds       = Static.EmptyChildIds;
        node.flags          = Created;
        return node.compIndex;
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

/// <summary>
/// Reserved symbol name.
/// If exposing public it need to store an array of <see cref="Entity"/>'s.<br/>
/// Similar to <see cref="Archetypes"/>.
/// </summary>
internal struct Entities;
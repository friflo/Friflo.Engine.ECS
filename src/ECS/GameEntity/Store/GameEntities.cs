// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// This file contains implementation specific for storing GameEntity's.
// The reason to separate handling of GameEntity's is to enable 'entity / component support' without GameEntity's.
// EntityStore remarks.
public partial class GameEntityStore
{
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public GameEntity CreateEntity() {
        var id      = sequenceId++;
        EnsureNodesLength(id + 1);
        var pid = GeneratePid(id);
        return CreateEntityNode(id, pid);
    }
    
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public GameEntity CreateEntity(int id) {
        if (id < Static.MinNodeId) {
            throw InvalidEntityIdException(id, nameof(id));
        }
        if (id < nodes.Length && nodes[id].Is(Created)) {
            throw IdAlreadyInUseException(id, nameof(id));
        }
        EnsureNodesLength(id + 1);
        var pid = GeneratePid(id);
        return CreateEntityNode(id, pid);
    }
    
    public GameEntity CreateFrom(int id, int[] childIds = null) {
        if (id < Static.MinNodeId) {
            throw InvalidEntityIdException(id, nameof(id));
        }
        if (id < nodes.Length && nodes[id].Is(Created)) {
            throw IdAlreadyInUseException(id, nameof(id));
        }
        // --- ensure EntityNode's referenced by child ids are present in nodes[]
        var maxId       = id;
        var childCount  = 0;
        if (childIds != null) {
            childCount = childIds.Length;
            for (int n = 0; n < childCount; n++) {
                maxId = Math.Max(maxId, childIds[n]);   
            }
        }
        EnsureNodesLength(maxId + 1);
        var pid     = GeneratePid(id);
        var entity  = CreateEntityNode(id, pid);
        if (childIds != null) {
            SetChildNodes(id, childIds, childCount);
        }
        return entity;
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

    /// <summary>expect <see cref="GameEntityStore.nodes"/> Length > id</summary> 
    private GameEntity CreateEntityNode(int id, long pid)
    {
        AssertIdInNodes(id);
        ref var node = ref nodes[id];
        if (node.Is(Created)) {
            AssertPid(node.pid, pid);
            return node.entity;
        }
        nodesCount++;
        if (nodesMaxId < id) {
            nodesMaxId = id;
        }
        AssertPid0(node.pid, pid);
        node.pid        = pid;
        var entity      = new GameEntity(id, defaultArchetype);
        // node.parentId   = Static.NoParentId;     // Is not set. A previous parent node has .parentId already set.
        node.childIds   = Static.EmptyChildNodes;
        node.flags      = Created;
        node.entity     = entity;
        return entity;
    }
    
    public void SetRoot(GameEntity entity) {
        if (entity == null) {
            throw new ArgumentNullException(nameof(entity));
        }
        if (this != entity.archetype.store) {
            throw InvalidStoreException(nameof(entity));
        }
        SetRootId(entity.id);
    }
    
    /// <summary>
    /// Creates a new entity with the struct components and tags of the given <paramref name="archetype"/>
    /// </summary>
    public GameEntity CreateEntity(Archetype archetype)
    {
        if (this != archetype.store) {
            InvalidStoreException(nameof(archetype));
        }
        var entity          = archetype.gameEntityStore.CreateEntity();
        entity.archetype    = archetype;
        entity.compIndex    = archetype.EntityCount;
        archetype.AddEntity(entity.id);
        return entity;
    }
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using static Friflo.Fliox.Engine.ECS.NodeFlags;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// This file contains implementation specific for storing GameEntity's.
// The reason to separate handling of GameEntity's is to enable 'entity / component support' without GameEntity's.
// EntityStore remarks.
public sealed partial class EntityStore
{
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public GameEntity CreateEntity() {
        var id      = sequenceId++;
        EnsureNodesLength(id + 1);
        var pid = GeneratePid(id);
        return CreateEntityInternal(id, pid);
    }
    
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public GameEntity CreateEntity(int id) {
        if (id < Static.MinNodeId) {
            throw InvalidEntityIdException(id, nameof(id));
        }
        if (id < nodes.Length && nodes[id].Is(Created)) {
            throw IdAlreadyInUseException(id);
        }
        EnsureNodesLength(id + 1);
        var pid = GeneratePid(id);
        return CreateEntityInternal(id, pid);
    }
    
    public GameEntity CreateFrom(int id, int[] childIds = null) {
        if (id < Static.MinNodeId) {
            throw InvalidEntityIdException(id, "dataNode.id");
        }
        if (id < nodes.Length && nodes[id].Is(Created)) {
            throw IdAlreadyInUseException(id);
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
        var entity  = CreateEntityInternal(id, pid);
        if (childIds != null) {
            SetChildNodes(id, childIds, childCount);
        }
        return entity;
    }
    
    [Conditional("DEBUG")]
    private void AssertIdInNodes(int id) {
        if (id < nodes.Length) {
            return;
        }
        throw new InvalidOperationException("expect id < node.length");
    }
    
    [Conditional("DEBUG")]
    private static void AssertPid(long pid, long expected) {
        if (expected == pid) {
            return;
        }
        throw new InvalidOperationException($"invalid pid. expected: {expected}, was: {pid}");
    }
    
    [Conditional("DEBUG")]
    private static void AssertPid0(long pid, long expected) {
        if (pid == 0 || pid == expected) {
            return;
        }
        throw new InvalidOperationException($"invalid pid. expected: 0 or {expected}, was: {pid}");
    }

    /// <summary>expect <see cref="nodes"/> Length > id</summary> 
    private GameEntity CreateEntityInternal(int id, long pid)
    {
        AssertIdInNodes(id);
        ref var node = ref nodes[id];
        if (node.Is(Created)) {
            AssertPid(node.pid, pid);
            return node.entity;
        }
        nodeCount++;
        if (nodeMaxId < id) {
            nodeMaxId = id;
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
}

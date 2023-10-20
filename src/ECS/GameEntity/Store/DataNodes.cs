// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// This file contains implementation specific for storing DataNode's.
public partial class GameEntityStore
{
    // ------------------------------------- GameEntity -> DataNode -------------------------------------
    public DataNode DataNodeFromEntity(GameEntity entity) {
        var id = entity.id;
        ref var node = ref nodes[id];
        if (!storeSync.TryGetDataNode(id, out var dataNode)) {
            dataNode = new DataNode { pid = id };
            storeSync.AddDataNode(dataNode);
        }
        // --- process child ids
        if (node.childCount > 0) {
            var children = dataNode.children = new List<long>(node.childCount); 
            foreach (var childId in node.ChildIds) {
                var pid = nodes[childId].pid;
                children.Add(pid);  
            }
        }
        // --- write struct & class components
        var jsonComponents = ComponentWriter.Instance.Write(entity);
        dataNode.components = new JsonValue(jsonComponents); // create array copy for now
        
        // --- process tags
        var tagCount = entity.Tags.Count; 
        if (tagCount == 0) {
            dataNode.tags = null;
        } else {
            dataNode.tags = new List<string>(tagCount);
            foreach(var tag in entity.Tags) {
                dataNode.tags.Add(tag.tagName);
            }
        }
        return dataNode;
    }
    
    // ------------------------------------- DataNode -> GameEntity -------------------------------------
    /// <returns>an <see cref="attached"/> entity</returns>
    public GameEntity DataNodeToEntity(DataNode dataNode, out string error)
    {
        if (dataNode == null) {
            throw new ArgumentNullException(nameof(dataNode));
        }
        GameEntity entity;
        if (pidType == PidType.UsePidAsId) {
            entity = CreateFromDataNodeUsePidAsId(dataNode);
        } else {
            entity = CreateFromDataNodeRandomPid (dataNode);
        }
        error = ComponentReader.Instance.Read(dataNode, entity, this);
        return entity;
    }

    private GameEntity CreateFromDataNodeRandomPid(DataNode dataNode) {
        // --- map pid to id
        var pid     = dataNode.pid;
        var pidMap  = pid2Id;
        if (!pidMap.TryGetValue(pid, out int id)) {
            id = sequenceId++;
            pidMap.Add(pid, id);
        }
        // --- map children pid's to id's
        int[] childIds  = null;
        var children    = dataNode.children;
        var childCount  = 0;
        if (children != null) {
            childCount = children.Count;
            childIds = new int[childCount];
            for (int n = 0; n < childCount; n++) {
                var childPid = children[n];
                if (!pidMap.TryGetValue(childPid, out int childId)) {
                    childId = sequenceId++;
                    pidMap.Add(childPid, childId);
                }
                childIds[n] = childId;
            }
        }
        EnsureNodesLength(sequenceId);
        var entity  = CreateEntityNode(id, pid);

        if (childIds != null) {
            UpdateEntityNodes(childIds, children);
            SetChildNodes(id, childIds, childCount);
        }
        return entity;
    }
    
    private GameEntity CreateFromDataNodeUsePidAsId(DataNode dataNode) {
        var pid = dataNode.pid;
        if (pid < 0 || pid > int.MaxValue) {
            throw new ArgumentException("pid mus be in range [0, 2147483647]. was: {pid}", nameof(dataNode));
        }
        var id          = (int)pid;
        // --- use pid's as id's
        int[] childIds  = null;
        var children    = dataNode.children;
        var maxPid      = id;
        var childCount  = 0;
        if (children != null) {
            childCount  = children.Count; 
            childIds    = new int[childCount];
            for (int n = 0; n < childCount; n++) {
                childIds[n]= (int)children[n];
            }
            foreach (var childId in childIds) {
                maxPid = Math.Max(maxPid, childId);
            }
        }
        EnsureNodesLength(maxPid + 1);
        var entity  = CreateEntityNode(id, id);
        
        if (childIds != null) {
            UpdateEntityNodes(childIds, children);
            SetChildNodes(id, childIds, childCount);
        }
        return entity;
    }
    
    /// update EntityNode.pid of the child nodes
    private void UpdateEntityNodes(int[] childIds, List<long> children)
    {
        var localNodes = nodes;
        for (int n = 0; n < childIds.Length; n++) {
            var childId             = childIds[n];
            localNodes[childId].pid = children[n];
        }
    }
}

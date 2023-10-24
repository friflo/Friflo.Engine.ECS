// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// This file contains implementation specific for storing DatabaseEntity's.
// Loading and storing DatabaseEntity's is implemented in GameEntityStore to enable declare all its fields private.
public partial class GameEntityStore
{
    // ---------------------------------- GameEntity -> DatabaseEntity ----------------------------------
    internal void GameToDatabaseEntity(GameEntity entity, DatabaseEntity databaseEntity, ComponentWriter writer)
    {
        var id = entity.id;
        ref var node = ref nodes[id];

        // --- process child ids
        if (node.childCount > 0) {
            var children = databaseEntity.children = new List<long>(node.childCount); 
            foreach (var childId in node.ChildIds) {
                var pid = nodes[childId].pid;
                children.Add(pid);  
            }
        }
        // --- write components & behaviors
        var jsonComponents = writer.Write(entity);
        databaseEntity.components = new JsonValue(jsonComponents); // create array copy for now
        
        // --- process tags
        var tagCount = entity.Tags.Count; 
        if (tagCount == 0) {
            databaseEntity.tags = null;
        } else {
            databaseEntity.tags = new List<string>(tagCount);
            foreach(var tag in entity.Tags) {
                databaseEntity.tags.Add(tag.tagName);
            }
        }
    }
    
    // ---------------------------------- DatabaseEntity -> GameEntity ----------------------------------
    internal GameEntity DatabaseToGameEntity(DatabaseEntity databaseEntity, out string error, ComponentReader reader)
    {
        GameEntity entity;
        if (pidType == PidType.UsePidAsId) {
            entity = CreateFromDbEntityUsePidAsId(databaseEntity);
        } else {
            entity = CreateFromDbEntityRandomPid (databaseEntity);
        }
        error = reader.Read(databaseEntity, entity, this);
        return entity;
    }

    private GameEntity CreateFromDbEntityRandomPid(DatabaseEntity databaseEntity)
    {
        // --- map pid to id
        var pid     = databaseEntity.pid;
        var pidMap  = pid2Id;
        if (!pidMap.TryGetValue(pid, out int id)) {
            id = sequenceId++;
            pidMap.Add(pid, id);
        }
        // --- map children pid's to id's
        int[] childIds  = null;
        var children    = databaseEntity.children;
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
    
    private GameEntity CreateFromDbEntityUsePidAsId(DatabaseEntity databaseEntity)
    {
        var pid = databaseEntity.pid;
        if (pid < Static.MinNodeId || pid > int.MaxValue) {
            throw new ArgumentException("pid mus be in range [1, 2147483647]. was: {pid}", nameof(databaseEntity));
        }
        var id          = (int)pid;
        // --- use pid's as id's
        int[] childIds  = null;
        var children    = databaseEntity.children;
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

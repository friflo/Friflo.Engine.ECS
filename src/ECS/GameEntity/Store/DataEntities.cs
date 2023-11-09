// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox;

// ReSharper disable MergeIntoLogicalPattern
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// This file contains implementation specific for storing DataEntity's.
// Loading and storing DataEntity's is implemented in GameEntityStore to enable declare all its fields private.
public partial class GameEntityStore
{
// --------------------------------------- GameEntity -> DataEntity ---------------------------------------
#region GameEntity -> DataEntity
    internal void GameToDataEntity(GameEntity entity, DataEntity dataEntity, ComponentWriter writer, bool pretty)
    {
        ProcessChildren(dataEntity, nodes[entity.id]);
        
        // --- write components & scripts
        var jsonComponents = writer.Write(entity, pretty);
        if (!jsonComponents.IsNull()) {
            JsonUtils.FormatComponents(jsonComponents, ref writer.buffer);
            jsonComponents = new JsonValue(writer.buffer);
        }
        dataEntity.components = new JsonValue(jsonComponents); // create array copy for now
        
        ProcessTags(entity, dataEntity);
    }

    private static void ProcessTags(GameEntity entity, DataEntity dataEntity)
    {
        var tagCount    = entity.Tags.Count;
        var tags        = dataEntity.tags;
        if (tagCount == 0) {
            tags?.Clear();
        } else {
            if (tags == null) {
                tags = dataEntity.tags = new List<string>(tagCount);
            } else {
                tags.Clear();
            }
            foreach (var tag in entity.Tags) {
                tags.Add(tag.tagName);
            }
        }
        if (!entity.TryGetComponent<Unresolved>(out var unresolved)) {
            return;
        }
        var unresolvedTags = unresolved.tags;
        if (unresolvedTags != null) {
            tags ??= dataEntity.tags = new List<string>(unresolvedTags.Length);
            foreach (var tag in unresolvedTags) {
                tags.Add(tag);
            }
        }
    }

    private void ProcessChildren(DataEntity dataEntity, in EntityNode node)
    {
        var children = dataEntity.children;
        if (node.childCount > 0) {
            if (children == null) {
                children = dataEntity.children = new List<long>(node.childCount);
            } else {
                children.Clear();
            }
            foreach (var childId in node.ChildIds) {
                var pid = nodes[childId].pid;
                children.Add(pid);
            }
        } else {
            dataEntity.children?.Clear();
        }
    }
    #endregion
    
// --------------------------------------- DataEntity -> GameEntity ---------------------------------------
#region DataEntity -> GameEntity

    internal GameEntity DataToGameEntity(DataEntity dataEntity, out string error, ComponentReader reader)
    {
        GameEntity entity;
        if (pidType == PidType.UsePidAsId) {
            entity = CreateFromDataEntityUsePidAsId(dataEntity);
        } else {
            entity = CreateFromDataEntityRandomPid (dataEntity);
        }
        error = reader.Read(dataEntity, entity, this);
        return entity;
    }

    private GameEntity CreateFromDataEntityRandomPid(DataEntity dataEntity)
    {
        // --- map pid to id
        var pid     = dataEntity.pid;
        var pidMap  = pid2Id;
        if (!pidMap.TryGetValue(pid, out int id)) {
            id = sequenceId++;
            pidMap.Add(pid, id);
        }
        // --- map children pid's to id's
        int[] childIds  = null;
        var children    = dataEntity.children;
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
    
    private GameEntity CreateFromDataEntityUsePidAsId(DataEntity dataEntity)
    {
        var pid = dataEntity.pid;
        if (pid < Static.MinNodeId || pid > int.MaxValue) {
            throw PidOutOfRangeException(pid, $"{nameof(DataEntity)}.{nameof(dataEntity.pid)}");
        }
        var id          = (int)pid;
        // --- use pid's as id's
        int[] childIds  = null;
        var children    = dataEntity.children;
        var maxPid      = id;
        var childCount  = 0;
        if (children != null) {
            childCount  = children.Count; 
            childIds    = new int[childCount];
            for (int n = 0; n < childCount; n++) {
                var childId = children[n];
                if (childId < Static.MinNodeId || childId > int.MaxValue) {
                    throw PidOutOfRangeException(childId, $"{nameof(DataEntity)}.{nameof(dataEntity.children)}");
                }
                childIds[n]= (int)childId;
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
    #endregion
}

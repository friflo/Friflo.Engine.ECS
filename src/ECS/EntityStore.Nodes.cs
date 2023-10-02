// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Friflo.Fliox.Engine.Client;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// Hard rule: this file/section MUST NOT use GameEntity instances

namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    // --------------------------------- tree node methods ---------------------------------
    private void EnsureNodesLength(int length)
    {
        var curLength = nodes.Length;
        if (length <= curLength) {
            return;
        }
        var newLength = Math.Max(length, 2 * nodes.Length);
        Utils.Resize(ref nodes, newLength);
        for (int n = curLength; n < length; n++) {
            nodes[n] = new EntityNode (n);
        }
    }
    
    public void SetRandomSeed(int seed) {
        randPid = new Random(seed);
    }
    
    private int GeneratePid(int id) {
        return pidType == PidType.UsePidAsId ? id : GenerateRandomPidForId(id);
    }
    
    private int GenerateRandomPidForId(int id)
    {
        while(true) {
            var pid = randPid.Next();
            if (pid2Id.TryAdd(pid, id)) {
                return pid;
            }
        }
    }
    
    internal void AddChild (int id, int childId)
    {
        // update child node parent
        ref var childNode = ref nodes[childId];
        if (HasParent(childNode.parentId)) {
            if (childNode.parentId == id) {
                // case: child node is already a child the entity
                return;
            }
            // --- remove child from current parent
            RemoveChildNode(childNode.parentId, childId);
        }
        childNode.parentId = id;
        
        // --- add child to entity
        ref var node    = ref nodes[id];
        int index       = node.childCount;
        if (node.childIds == null) {
            node.childIds = new int[4];
        } else if (index == node.childIds.Length) {
            var newLen = Math.Max(4, 2 * node.childIds.Length);
            Utils.Resize(ref node.childIds, newLen); 
        }
        node.childIds[index] = childId;
        node.childCount++;
        SetTreeFlags(nodes, childId, node.flags & TreeNode);
    }
    
    internal void RemoveChild (int id, int childId)
    {
        ref var childNode = ref nodes[childId];
        if (id != childNode.parentId) {
            return;
        }
        childNode.parentId = Static.NoParentId;
        RemoveChildNode(id, childId);
        ClearTreeFlags(nodes, childId, TreeNode);
    }
    
    private void RemoveChildNode (int entity, int childEntity)
    {
        ref var node    = ref nodes[entity];
        var childNodes  = node.childIds;
        int len         = node.childCount;
        for (int n = 0; n < len; n++) {
            if (childEntity != childNodes[n]) {
                continue;
            }
            for (int i = n + 1; i < len; i++) {
                childNodes[i - 1] = childNodes[i];
            }
            break;
        }
        node.childCount--;
    }
    
    private void SetChildNodes(int id, int[] childIds, int childCount)
    {
        // --- add child ids to EntityNode
        ref var node    = ref nodes[id];
        node.childIds   = childIds;
        node.childCount = childCount;
        
        // --- set the parentId on child nodes
        for (int n = 0; n < childCount; n++)
        {
            var childId     = childIds[n];
            ref var child   = ref nodes[childId];
            if (child.parentId < Static.MinNodeId)
            {
                child.parentId = id;
                if (HasCycle(id, childId, out var exception)) {
                    throw exception;
                }
                continue;
            }
            if (child.parentId == id) {
                continue;
            }
            // could remove child from its current parent and set new child.parentId
            throw EntityAlreadyHasParent(childId, child.parentId, id);
        }
    }
    
    private bool HasCycle(int id, int childId, out InvalidOperationException exception)
    {
        if (id == childId) {
            // case: self reference
            exception = new InvalidOperationException($"self reference in entity: {id}");
            return true;
        }
        // --- iterate all parents of id and fail if id in these parents 
        var cur = id;
        while (true) {
            cur = nodes[cur].parentId;
            if (cur < Static.MinNodeId) {
                exception = null;
                return false;
            }
            if (cur == id) {
                exception = EntityDependencyCycleException(id);
                return true;
            }
        }
    }
    
    private InvalidOperationException EntityDependencyCycleException(int id) {
        var cur = id;
        var sb = new StringBuilder();
        sb.Append("dependency cycle in entity children: ");
        sb.Append(id);
        while (true) {
            cur = nodes[cur].parentId;
            if (cur != id) {
                sb.Append(" -> ");
                sb.Append(cur);
                continue;
            }
            break;
        }
        sb.Append(" -> ");
        sb.Append(id);
        return new InvalidOperationException(sb.ToString());
    }
    
    internal void DeleteNode(int id)
    {
        nodeCount--;
        ref var node        = ref nodes[id];
        
        // --- mark its child nodes as floating
        ClearTreeFlags(nodes, id, TreeNode);
        foreach (var childId in node.ChildIds) {
            nodes[childId].parentId = Static.NoParentId;
        }
        var parentId    = node.parentId;
        // --- clear node entry
        node            = new EntityNode(id); // clear node
        
        // --- remove child from parent 
        if (!HasParent(parentId)) {
            return;
        }
        ref var parent  = ref nodes[parentId];
        var childIds    = parent.childIds;
        var count       = parent.childCount;
        for (int n = count - 1; n >= 0; n--) {
            if (childIds[n] != id) {
                continue;
            }
            parent.childCount--;
            for (var i = n + 1; i < count; i++) {
                childIds[i - 1] = childIds[i];
            }
            return;
        }
        throw new InvalidOperationException($"unexpected state: child id not found: {id}");
    }
    
    private static void SetTreeFlags(EntityNode[] nodes, int id, NodeFlags flag) {
        ref var node    = ref nodes[id];
        if (node.IsNot(Created) || node.Is(flag)) {
            return;
        }
        node.flags |= flag;
        foreach (var childId in node.ChildIds) {
            SetTreeFlags(nodes, childId, flag);
        }
    }
    
    private static void ClearTreeFlags(EntityNode[] nodes, int id, NodeFlags flag) {
        ref var node    = ref nodes[id];
        if (node.IsNot(Created) || node.IsNot(flag)) {
            return;
        }
        node.flags &= ~flag;
        foreach (var childId in node.ChildIds) {
            ClearTreeFlags(nodes, childId, flag);
        }
    }
    
    private void SetRootId(int id) {
        if (HasRoot) {
            throw new InvalidOperationException("EntityStore already has a root entity");
        }
        if (HasParent(nodes[id].parentId)) {
            throw new InvalidOperationException("entity must not have a parent to be root");
        }
        rootId                  = id;
        nodes[rootId].parentId  = Static.RootId;
        SetTreeFlags(nodes, id, TreeNode);
    }
    
    public DataNode EntityAsDataNode(GameEntity entity, EntityStoreClient client) {
        var id = entity.id;
        ref var node = ref nodes[id];
        if (!client.entities.Local.TryGetEntity(id, out var dataNode)) {
            dataNode = new DataNode { pid = id };
            client.entities.Local.Add(dataNode);
        }
        if (node.childCount > 0) {
            var children = dataNode.children = new List<int>(node.childCount); 
            foreach (var childId in node.ChildIds) {
                var pid = nodes[childId].pid;
                children.Add(pid);  
            }
        }
        dataNode.components = ComponentWriter.Instance.Write(entity);
        return dataNode;
    }
}

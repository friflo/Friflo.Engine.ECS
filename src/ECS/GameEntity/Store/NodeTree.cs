// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Friflo.Fliox.Engine.Client;
using Friflo.Json.Fliox;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public partial class GameEntityStore
{
    // --------------------------------- tree node methods ---------------------------------
    private void EnsureNodesLength(int length)
    {
        var curLength = nodes.Length;
        if (length <= curLength) {
            return;
        }
        var newLength = Math.Max(length, 2 * curLength);
        Utils.Resize(ref nodes, newLength);
        var localNodes = nodes;
        for (int n = curLength; n < length; n++) {
            localNodes[n] = new EntityNode (n);
        }
    }
    
    public void SetRandomSeed(int seed) {
        randPid = new Random(seed);
    }
    
    private long GeneratePid(int id) {
        return pidType == PidType.UsePidAsId ? id : GenerateRandomPidForId(id);
    }
    
    private long GenerateRandomPidForId(int id)
    {
        while(true) {
            // generate random int to have numbers with small length e.g. 2147483647 (max int)
            // could also generate long which requires more memory when persisting entities
            long pid = randPid.Next();
            if (pid2Id.TryAdd(pid, id)) {
                return pid;
            }
        }
    }
    
    internal static bool HasParent(int id)  =>   id >= Static.MinNodeId;
    
    internal void AddChild (int id, int childId)
    {
        var localNodes = nodes;
        // update child node parent
        ref var childNode = ref localNodes[childId];
        if (HasParent(childNode.parentId)) {
            if (childNode.parentId == id) {
                // case: entity with given id is already a child of this entity
                return;
            }
            // --- remove child from current parent
            RemoveChildNode(childNode.parentId, childId);
        }
        childNode.parentId = id;
        
        // --- add entity with given id as child to this entity
        ref var node    = ref localNodes[id];
        int index       = node.childCount;
        if (node.childIds == null) {
            node.childIds = new int[4];
        } else if (index == node.childIds.Length) {
            var newLen = Math.Max(4, 2 * node.childIds.Length);
            Utils.Resize(ref node.childIds, newLen); 
        }
        node.childIds[index] = childId;
        node.childCount++;
        SetTreeFlags(localNodes, childId, node.flags & RootTreeNode);
    }
    
    internal void RemoveChild (int id, int childId)
    {
        ref var childNode = ref nodes[childId];
        if (id != childNode.parentId) {
            return;
        }
        childNode.parentId = Static.NoParentId;
        RemoveChildNode(id, childId);
        ClearTreeFlags(nodes, childId, RootTreeNode);
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
        var localNodes  = nodes;
        // --- add child ids to EntityNode
        ref var node    = ref localNodes[id];
        node.childIds   = childIds;
        node.childCount = childCount;
        
        // --- set the parentId on child nodes
        for (int n = 0; n < childCount; n++)
        {
            var childId     = childIds[n];
            ref var child   = ref localNodes[childId];
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
    
    private static Exception EntityAlreadyHasParent(int child, int curParent, int newParent) {
        var msg = $"child has already a parent. child: {child} current parent: {curParent}, new parent: {newParent}";
        return new InvalidOperationException(msg);
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
    
    protected internal override void UpdateEntityCompIndex(int id, int compIndex) {
        nodes[id].entity.compIndex = compIndex;
    }
    
    internal void DeleteNode(int id)
    {
        nodesCount--;
        var localNodes  = nodes;
        ref var node    = ref localNodes[id];
        
        // --- mark its child nodes as floating
        ClearTreeFlags(localNodes, id, RootTreeNode);
        foreach (var childId in node.ChildIds) {
            localNodes[childId].parentId = Static.NoParentId;
        }
        var parentId    = node.parentId;
        // --- clear node entry
        node            = new EntityNode(id); // clear node
        
        // --- remove child from parent 
        if (!HasParent(parentId)) {
            return;
        }
        ref var parent  = ref localNodes[parentId];
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
            childIds[n + 1] = 0; // clear last child id for debug clarity. not necessary because of nodeCount--
            return;
        }
        throw new InvalidOperationException($"unexpected state: child id not found. parent id: {parentId}, child id: {id}");
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
            throw new InvalidOperationException($"EntityStore already has a root entity. current root id: {rootId}");
        }
        ref var parentId = ref nodes[id].parentId;
        if (HasParent(parentId)) {
            throw new InvalidOperationException($"entity must not have a parent to be root. current parent id: {parentId}");
        }
        rootId      = id;
        parentId    = Static.RootId;
        SetTreeFlags(nodes, id, RootTreeNode);
    }
    
    public DataNode EntityAsDataNode(GameEntity entity) {
        var id = entity.id;
        ref var node = ref nodes[id];
        if (!clientNodes.TryGetEntity(id, out var dataNode)) {
            dataNode = new DataNode { pid = id };
            clientNodes.Add(dataNode);
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
}

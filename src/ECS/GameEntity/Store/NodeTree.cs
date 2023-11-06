// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
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
    
    private static bool HasParent(int id)  =>   id >= Static.MinNodeId;
    
    internal void AddChild (int id, int childId)
    {
        var localNodes      = nodes;
        // update child node parent
        ref var childNode   = ref localNodes[childId];
        var curIndex        = -1;
        var curParentId     = childNode.parentId;
        if (HasParent(curParentId)) {
            if (curParentId == id) {
                // case: entity with given id is already a child of this entity
                return;
            }
            // --- remove child from current parent
            curIndex = RemoveChildNode(curParentId, childId);
        }
        childNode.parentId = id;
        
        // --- add entity with given id as child to this entity
        ref var parent  = ref localNodes[id];
        int index       = parent.childCount;
        if (parent.childIds == null) {
            parent.childIds = new int[4];
        } else if (index == parent.childIds.Length) {
            var newLen = Math.Max(4, 2 * parent.childIds.Length);
            Utils.Resize(ref parent.childIds, newLen); 
        }
        parent.childIds[index] = childId;
        parent.childCount++;
        SetTreeFlags(localNodes, childId, parent.flags & TreeNode);
        
        if (curIndex != - 1) {
            OnRemoveChildNode(curParentId, childId, curIndex);
        }
        OnAddChildNode(id, childId, index);
    }
    
    internal void InsertChild (int id, int index, int childId)
    {
        var localNodes = nodes;
        // update child node parent
        ref var childNode = ref localNodes[childId];
        if (HasParent(childNode.parentId))
        {
            if (childNode.parentId != id) {
                // --- case: child has a different parent => remove node from current parent
                int curIndex = RemoveChildNode(childNode.parentId, childId);
                MoveChildNode(ref childNode, index);
                
                OnRemoveChildNode(childNode.parentId, childId, curIndex);
                OnAddChildNode(id, childId, index);
                return;
            }
            // case: entity with given id is already a child of this entity => move child
            MoveChildNode(ref childNode, index);
            
            OnAddChildNode(id, childId, index);
            return;
        }
        // --- insert entity with given id as child to its parent
        ref var parent = ref localNodes[id];
        if (index > parent.childCount) {
            throw new IndexOutOfRangeException();
        }
        if (parent.childIds == null) {
            parent.childIds = new int[4];
        } else if (index == parent.childIds.Length) {
            var newLen = Math.Max(4, 2 * parent.childIds.Length);
            Utils.Resize(ref parent.childIds, newLen); 
        }
        var childIds = parent.childIds;
        for (int n = parent.childCount; n > index; n++) {
            childIds[n] = childIds[n - 1];    
        }
        childIds[index] = childId;
        parent.childCount++;
        SetTreeFlags(localNodes, childId, parent.flags & TreeNode);
        
        OnAddChildNode(id, childId, index);
    }
    
    internal bool RemoveChild (int id, int childId)
    {
        ref var childNode = ref nodes[childId];
        if (id != childNode.parentId) {
            return false;
        }
        childNode.parentId  = Static.NoParentId;
        var curIndex        = RemoveChildNode(id, childId);
        ClearTreeFlags(nodes, childId, TreeNode);
        
        OnRemoveChildNode(id, childId, curIndex);
        return true;
    }
    
    private int RemoveChildNode (int entity, int childEntity)
    {
        ref var parent  = ref nodes[entity];
        var childNodes  = parent.childIds;
        int len         = parent.childCount;
        int childIndex  = -1;
        for (int n = 0; n < len; n++) {
            if (childEntity != childNodes[n]) {
                continue;
            }
            childIndex = n;
            for (int i = n + 1; i < len; i++) {
                childNodes[i - 1] = childNodes[i];
            }
            break;
        }
        parent.childCount--;
        return childIndex;
    }
    
    private void MoveChildNode(ref EntityNode childNode, int newIndex)
    {
        ref var parent  = ref nodes[childNode.parentId];
        var childIds    = parent.childIds;
        int curIndex    = GetChildIndex(childIds, parent.childCount, childNode.id);
        if (newIndex < curIndex) {
            // case: move forward   curIndex: [--, --, --, id]
            //                      newIndex: [id, --, --, --]
            for (int n = curIndex; n > newIndex; n--) {
                childIds[n] = childIds[n - 1];
            }
        } else {
            // case: move backward  curIndex: [id, --, --, --]
            //                      newIndex: [--, --, --, id]
            for (int n = newIndex; n < curIndex; n--) {
                childIds[n] = childIds[n + 1];
            }
        }
        childIds[newIndex] = childNode.id;
    }
    
    private static int GetChildIndex(int[] childIds, int childCount, int id)
    {
        for (int n = 0; n < childCount; n++) {
            if (childIds[n] != id) {
                continue;
            }
            return n;
        }
        throw new InvalidOperationException("id not found in childIds");
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
        ClearTreeFlags(localNodes, id, TreeNode);
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
    
    private void SetStoreRootEntity(GameEntity entity) {
        if (storeRoot != null) {
            throw new InvalidOperationException($"EntityStore already has a {nameof(StoreRoot)}. {nameof(StoreRoot)} id: {storeRoot.id}");
        }
        var id = entity.id;
        ref var parentId = ref nodes[id].parentId;
        if (HasParent(parentId)) {
            throw new InvalidOperationException($"entity must not have a parent to be {nameof(StoreRoot)}. current parent id: {parentId}");
        }
        storeRoot   = entity;
        parentId    = Static.StoreRootParentId;
        SetTreeFlags(nodes, id, TreeNode);
    }
    
    // ---------------------------------- child nodes change notification ----------------------------------
        
    private         ChildNodesChangedHandler collectionChanged;
    
    public  event   ChildNodesChangedHandler CollectionChanged
    {
        add     => collectionChanged += value;
        remove  => collectionChanged -= value;
    }

    private void OnAddChildNode(int parentId, int childId, int childIndex)
    {
        if (collectionChanged == null) {
            return;
        }
        var args = new ChildNodesChangedArgs(ChildNodesChangedAction.Add, parentId, childId, childIndex);
        collectionChanged(this, args);
    }
    
    private void OnRemoveChildNode(int parentId, int childId, int childIndex)
    {
        if (collectionChanged == null) {
            return;
        }
        var args = new ChildNodesChangedArgs(ChildNodesChangedAction.Remove, parentId, childId, childIndex);
        collectionChanged(this, args);
    }
    
    
    // ------------------------------------- GameEntity access -------------------------------------
    internal TreeMembership  GetTreeMembership(int id) {
        return nodes[id].Is(TreeNode) ? TreeMembership.treeNode : TreeMembership.floating;
    }

    internal GameEntity GetParent(int id)
    { 
        var parentNode  = nodes[id].parentId;
        return HasParent(parentNode) ? nodes[parentNode].entity : null;
    }
    
    internal ChildNodes GetChildNodes(int id)
    {
        ref var node    = ref nodes[id];
        return new ChildNodes(nodes, node.childIds, node.childCount);
    }
    
    internal ReadOnlySpan<int> GetChildIds(int id)
    {
        ref var node = ref nodes[id];
        return new ReadOnlySpan<int>(node.childIds, 0, node.childCount);
    }
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// ReSharper disable ConvertToAutoPropertyWhenPossible
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
    
    internal void AddChild (int parentId, int childId)
    {
        var localNodes      = nodes;
        // update child node parent
        ref var childNode   = ref localNodes[childId];
        var curParentId     = childNode.parentId;
        if (HasParent(curParentId)) {
            if (curParentId == parentId) {
                // case: entity with given id is already a child of this entity
                return;
            }
            // --- remove child from current parent
            int curIndex = RemoveChildNode(ref localNodes[curParentId], childId);
            OnChildNodeRemove(curParentId, childId, curIndex);
        }
        // --- add entity with given id as child to this entity
        ref var parent      = ref localNodes[parentId];
        int index           = parent.childCount;
        childNode.parentId  = parentId;
        EnsureChildIdsCapacity(ref parent, index);
        parent.childIds[index] = childId;
        parent.childCount++;
        SetTreeFlags(localNodes, childId, parent.flags & TreeNode);
        
        OnChildNodeAdd(parentId, childId, index);
    }
    
    internal void InsertChild (int parentId, int childId, int childIndex)
    {
        var localNodes  = nodes;
        ref var parent  = ref localNodes[parentId];
        if (childIndex > parent.childCount) {
            throw new IndexOutOfRangeException();
        }
        // update child node parent
        ref var childNode   = ref localNodes[childId];
        var curParentId     = childNode.parentId;
        if (HasParent(curParentId))
        {
            int curIndex;
            ref var curParent = ref localNodes[curParentId];
            if (curParentId != parentId) {
                // --- case: child has a different parent => remove node from current parent
                curIndex = RemoveChildNode(ref curParent, childId);
                OnChildNodeRemove(curParentId, childId, curIndex);
                goto InsertNode;
            }
            // case: entity with given id is already a child of this entity => move child
            curIndex = GetChildIndex(ref curParent, childId);
            if (curIndex == childIndex) {
                // case: child entity is already at the requested childIndex
                return;
            }
            curIndex = RemoveChildNode(ref curParent, childId);
            OnChildNodeRemove(curParentId, childId, curIndex);
            
            InsertChildNode(ref localNodes[parentId], childId, childIndex);
            OnChildNodeAdd (parentId,                 childId, childIndex);
            return;
        }
    InsertNode:
        // --- insert entity with given id as child to its parent
        childNode.parentId  = parentId;
        EnsureChildIdsCapacity(ref parent, childIndex);
        InsertChildNode(ref parent, childId, childIndex);
        SetTreeFlags(localNodes, childId, parent.flags & TreeNode);
        
        OnChildNodeAdd(parentId,          childId, childIndex);
    }
    
    internal bool RemoveChild (int parentId, int childId)
    {
        var localNodes      = nodes;
        ref var childNode   = ref localNodes[childId];
        if (parentId != childNode.parentId) {
            return false;
        }
        childNode.parentId  = Static.NoParentId;
        var curIndex        = RemoveChildNode(ref localNodes[parentId], childId);
        ClearTreeFlags(localNodes, childId, TreeNode);
        
        OnChildNodeRemove(parentId, childId, curIndex);
        return true;
    }
    
    internal ref readonly EntityNode GetChildNodeByIndex(int parentId, int childIndex) {
        var childIds = nodes[parentId].childIds;
        return ref nodes[childIds[childIndex]];
    }
    
    internal int GetChildIndex(int parentId, int childId) => GetChildIndex(ref nodes[parentId], childId);
    
    private static int GetChildIndex(ref EntityNode parent, int childId)
    {
        var childIds    = parent.childIds;
        int count       = parent.childCount;
        for (int n = 0; n < count; n++) {
            if (childId != childIds[n]) {
                continue;
            }
            return n;
        }
        return -1;
    }
    
    private static int RemoveChildNode (ref EntityNode parent, int childId)
    {
        var childIds    = parent.childIds;
        int count       = parent.childCount;
        for (int n = 0; n < count; n++) {
            if (childId != childIds[n]) {
                continue;
            }
            for (int i = n + 1; i < count; i++) {
                childIds[i - 1] = childIds[i];
            }
            parent.childCount   = --count;
            childIds[count]     = 0;  // clear last child id for debug clarity. not necessary because of childCount--
            return n;
        }
        throw new InvalidOperationException($"unexpected state: child id not found. parent id: {parent.id}, child id: {childId}");
    }
    
    private static void InsertChildNode (ref EntityNode parent, int childId, int childIndex)
    {
        var childNodes  = parent.childIds;
        for (int n = parent.childCount; n > childIndex; n--) {
            childNodes[n] = childNodes[n - 1];
        }
        childNodes[childIndex] = childId;
        parent.childCount++;
    }
    
    private static void EnsureChildIdsCapacity(ref EntityNode parent, int index)
    {
        if (parent.childIds == null) {
            parent.childIds = new int[4];
            return;
        }
        if (index == parent.childIds.Length) {
            var newLen = Math.Max(4, 2 * parent.childIds.Length);
            Utils.Resize(ref parent.childIds, newLen); 
        }
    }
    
    private void SetChildNodes(int parentId, int[] childIds, int childCount)
    {
        var localNodes  = nodes;
        // --- add child ids to EntityNode
        ref var node    = ref localNodes[parentId];
        node.childIds   = childIds;
        node.childCount = childCount;
        
        // --- set the parentId on child nodes
        for (int n = 0; n < childCount; n++)
        {
            var childId     = childIds[n];
            ref var child   = ref localNodes[childId];
            if (child.parentId < Static.MinNodeId)
            {
                child.parentId = parentId;
                if (HasCycle(parentId, childId, out var exception)) {
                    throw exception;
                }
                continue;
            }
            if (child.parentId == parentId) {
                continue;
            }
            // could remove child from its current parent and set new child.parentId
            throw EntityAlreadyHasParent(childId, child.parentId, parentId);
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
        var cur         = id;
        var localNodes  = nodes;
        while (true) {
            cur = localNodes[cur].parentId;
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
        int curIndex    = RemoveChildNode(ref localNodes[parentId], id);
        OnChildNodeRemove(parentId, id, curIndex);
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
    /// <summary>
    /// Set a single <see cref="ChildNodesChangedHandler"/> to get events listed at <see cref="ChildNodesChanged"/>.<br/>
    /// Event handlers previously added with <see cref="ChildNodesChanged"/> are removed.<br/>
    /// </summary>
    public ChildNodesChangedHandler ChildNodesChangedHandler { get => childNodesChanged; set => childNodesChanged = value; }
    
    /// <summary>
    /// A <see cref="ECS.ChildNodesChangedHandler"/> added to a <see cref="GameEntityStore"/> get events on
    /// <list type="bullet">
    ///   <item><see cref="GameEntity.AddChild"/></item>
    ///   <item><see cref="GameEntity.InsertChild"/></item>
    ///   <item><see cref="GameEntity.RemoveChild"/></item>
    ///   <item><see cref="GameEntity.DeleteEntity"/></item>
    /// </list>
    /// </summary>
    public event ChildNodesChangedHandler ChildNodesChanged
    {
        add     => childNodesChanged += value;
        remove  => childNodesChanged -= value;
    }

    private void OnChildNodeAdd(int parentId, int childId, int childIndex)
    {
        if (childNodesChanged == null) {
            return;
        }
        var args = new ChildNodesChangedArgs(ChildNodesChangedAction.Add, parentId, childId, childIndex);
        childNodesChanged(this, args);
    }
    
    private void OnChildNodeRemove(int parentId, int childId, int childIndex)
    {
        if (childNodesChanged == null) {
            return;
        }
        var args = new ChildNodesChangedArgs(ChildNodesChangedAction.Remove, parentId, childId, childIndex);
        childNodesChanged(this, args);
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

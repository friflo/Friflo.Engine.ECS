// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Threading;
using static Friflo.Engine.ECS.NodeFlags;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStore
{
    // --------------------------------- tree node methods ---------------------------------
    /// <summary>
    /// Allocates memory for entities in the store to enable creating entities without reallocation.
    /// </summary>
    /// <returns>The number of entities that can be added without reallocation. </returns>
    public int EnsureCapacity(int capacity)
    {
        var curLength   = nodes.Length;
        var last        = intern.sequenceId + 1;
        var curCapacity = curLength - last;
        if (curCapacity >= capacity) {
            return curCapacity;
        }
        var newLength   = last + capacity;
        ArrayUtils.Resize(ref nodes, newLength);
        var localNodes = nodes;
        for (int n = curLength; n < newLength; n++) {
            localNodes[n].id = n;
            // localNodes[n] = new EntityNode (n);      // see: EntityNode.id comment
        }
        return newLength - last;
    }
    
    private void EnsureNodesLength(int length)
    {
        var curLength = nodes.Length;
        if (length <= curLength) {
            return;
        }
        var newLength = Math.Max(length, 2 * curLength); // could grow slower to minimize heap pressure
        ArrayUtils.Resize(ref nodes, newLength);
        var localNodes = nodes;
        for (int n = curLength; n < newLength; n++) {
            localNodes[n].id = n;
            // localNodes[n] = new EntityNode (n);      // see: EntityNode.id comment
        }
    }
    
    /// <summary>
    /// Set the seed used to create random entity <see cref="Entity.Pid"/>'s for an entity store <br/>
    /// created with <see cref="PidType"/> == <see cref="PidType.RandomPids"/>.
    /// </summary>
    public void SetRandomSeed(int seed) {
        intern.randPid = new Random(seed);
    }
    
    private long GeneratePid(int id) {
        return intern.pidType == PidType.UsePidAsId ? id : GenerateRandomPidForId(id);
    }
    
    private long GenerateRandomPidForId(int id)
    {
        while(true) {
            // generate random int to have numbers with small length e.g. 2147483647 (max int)
            // could also generate long which requires more memory when persisting entities
            long pid = intern.randPid.Next();
            if (intern.pid2Id.TryAdd(pid, id)) {
                return pid;
            }
        }
    }
    
    private static bool HasParent(int id)  =>   id >= Static.MinNodeId;
    
    internal int AddChild (int parentId, int childId)
    {
        var localNodes      = nodes;
        ref var childNode   = ref localNodes[childId];
        var curParentId     = childNode.parentId;
        if (HasParent(curParentId)) {
            if (curParentId == parentId) {
                // case: entity with given id is already a child of this entity
                return -1;
            }
            // --- remove child from current parent
            int curIndex = RemoveChildNode(ref localNodes[curParentId], childId);
            OnChildNodeRemove(curParentId, childId, curIndex);
        } else {
            if (parentId == childId) {
                // case: tried to add entity to itself as a child
                throw AddEntityAsChildToItselfException(parentId);
            }
        }
        // --- add entity with given id as child to this entity
        ref var parent      = ref localNodes[parentId];
        int index           = parent.childCount;
        childNode.parentId  = parentId;
        parent.childCount++;
        EnsureChildIdsCapacity(ref parent,  parent.childCount);
        parent.childIds[index] = childId;
        SetTreeFlags(localNodes, childId, parent.flags & TreeNode);
        
        OnChildNodeAdd(parentId, childId, index);
        return index;
    }
    
    internal void InsertChild (int parentId, int childId, int childIndex)
    {
        var localNodes  = nodes;
        ref var parent  = ref localNodes[parentId];
        if (childIndex > parent.childCount) {
            throw new IndexOutOfRangeException();
        }
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
            curIndex = GetChildIndex(curParent, childId);
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
        InsertChildNode(ref parent, childId, childIndex);
        SetTreeFlags(localNodes, childId, parent.flags & TreeNode);
        
        OnChildNodeAdd(parentId,    childId, childIndex);
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
    
    /* redundant: Entity.GetChildIndex() provide same feature but cleaner
    internal ref readonly EntityNode GetChildNodeByIndex(int parentId, int childIndex) {
        var childIds = nodes[parentId].childIds;
        return ref nodes[childIds[childIndex]];
    } */
    
    /* redundant: Entity.ChildEntities[] provide same feature but cleaner
    internal Entity GetChildEntityByIndex(int parentId, int childIndex) {
        var childIds = nodes[parentId].childIds;
        return new Entity(childIds[childIndex], this);
    } */
    
    internal int GetChildIndex(int parentId, int childId) => GetChildIndex(nodes[parentId], childId);
    
    private static int GetChildIndex(in EntityNode parent, int childId)
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
        EnsureChildIdsCapacity(ref parent, parent.childCount + 1);
        var childIds = parent.childIds;
        for (int n = parent.childCount; n > childIndex; n--) {
            childIds[n] = childIds[n - 1];
        }
        childIds[childIndex] = childId;
        parent.childCount++;
    }
    
    private static void EnsureChildIdsCapacity(ref EntityNode parent, int length)
    {
        if (parent.childIds == null) {
            parent.childIds = new int[Math.Max(4, length)];
            return;
        }
        if (length > parent.childIds.Length) { 
            var newLen = Math.Max(2 * parent.childIds.Length, length);
            ArrayUtils.Resize(ref parent.childIds, newLen);
        }
    }
    
    private void SetChildNodes(int parentId, ReadOnlySpan<int> newChildIds)
    {
        if (intern.childEntitiesChanged != null) {
            // case: childNodesChanged handler exists       => assign new child ids one by one to send events
            SetChildNodesWithEvents(parentId, newChildIds);
            return;
        }
        // case: no registered childNodesChanged handlers   => assign new child ids at once
        if (newChildIds.Length == 0) { // todo fix
            return;
        }
        ref var node        = ref nodes[parentId];
        var     newCount    = newChildIds.Length;
        int[]   childIds;
        if (newCount <= node.childCount) {
            childIds = node.childIds;
            newChildIds.CopyTo(childIds);
        } else {
            childIds = node.childIds = newChildIds.ToArray();
        }
        node.childCount = newCount;
        SetChildParents(childIds, newCount, parentId);
    }
    
    private void SetChildNodesWithEvents(int parentId, ReadOnlySpan<int> newIds)
    {
        ref var node    = ref nodes[parentId];
        var newCount    = newIds.Length;
        var childIds    = node.childIds;
        if (childIds == null || newCount > childIds.Length) {
            ArrayUtils.Resize(ref node.childIds, newCount);
            childIds = node.childIds;
        }
        // --- 1. Remove missing ids in new child ids.          E.g.    cur ids [2, 3, 4, 5]
        //                                                             *newIds  [6, 4, 2, 5]    => remove: 3
        //                                                              result  [2, 4, 5]
        ChildIds_RemoveMissingIds(newIds, ref node);
        
        // --- 2. Insert new ids at their specified position.   E.g.    cur ids [2, 4, 5]
        //                                                             *newIds  [6, 4, 2, 5]    => insert: 6
        //                                                              result  [6, 2, 4, 5]    childCount = newCount
        ChildIds_InsertNewIds    (newIds, ref node);
        
        // --- 3. Establish specified id order.                 E.g.    cur ids [6, 2, 4, 5]
        //                                                             *newIds  [6, 4, 2, 5]
        // 3.1  get range (first,last) where positions are different => range   [6, x, x, 5]
        // 3.2  remove range                                         =>         [6, 2, 4, 5]    => remove 4
        //                                                                      [6, 2, 5]       => remove 2
        //                                                              result  [6, 5]
        // 3.3  insert range in specified order                      =>         [6, 5]          => insert 4
        //                                                                      [6, 4, 5]       => insert 2
        //                                                             childIds [6, 4, 2, 5]    finished
        ChildIds_GetRange(childIds, newIds, out int first, out int last);
        ChildIds_RemoveRange(ref node, first, last);
        ChildIds_InsertRange(ref node, first, last, newIds);
        
        SetChildParents(childIds, newCount, parentId);
    }

    // --- 1.
    private void ChildIds_RemoveMissingIds(ReadOnlySpan<int> newIds, ref EntityNode node)
    {
        var childIds = node.childIds;
        var newIdSet = idBufferSet;
        newIdSet.Clear();
        foreach (var id in newIds) {
            newIdSet.Add(id);
        }
        for (int index = node.childCount - 1; index >= 0; index--) {
            var id = childIds[index];
            if (newIdSet.Contains(id)) {
                continue;
            }
            for (int i = index + 1; i < node.childCount; i++) {
                childIds[i - 1] = childIds[i];
            }
            --node.childCount;
            childIds[node.childCount]   = 0; // not necessary but simplify debugging
            nodes[id].parentId          = Static.NoParentId;
            OnChildNodeRemove(node.id, id, index);
        }
    }

    // --- 2.
    private void ChildIds_InsertNewIds(ReadOnlySpan<int> newIds, ref EntityNode node)
    {
        var childIds = node.childIds;
        var curIdSet = idBufferSet;
        curIdSet.Clear();
        for (int n = 0; n < node.childCount; n++) {
            curIdSet.Add(childIds[n]);
        }
        var newCount = newIds.Length;
        for (int index = 0; index < newCount; index++)
        {
            var id = newIds[index];
            if (curIdSet.Contains(id)) {
                // case: child ids contains id already
                continue;
            }
            // case: child ids does not contain id      => insert at specified position
            for (int n = node.childCount; n > index; n--) {
                childIds[n] = childIds[n - 1];
            }
            childIds[index] = id;
            ++node.childCount;
            OnChildNodeAdd(node.id, id, index);
        }
    }

    // --- 3.1
    private static void ChildIds_GetRange(int[] childIds, ReadOnlySpan<int> newIds, out int first, out int last)
    {
        var count = newIds.Length;
        first = 0;
        for (; first < count; first++) {
            var id = newIds[first];
            if (childIds[first] == id) {
                // case: id is already at specified position
                continue;
            }
            break;
        }
        last = count - 1;
        for (; last > first; last--) {
            var id = newIds[last];
            if (childIds[last] == id) {
                // case: id is already at specified position
                continue;
            }
            break;
        }
    }
    
    // --- 3.2
    private void ChildIds_RemoveRange(ref EntityNode node, int first, int last)
    {
        var childIds    = node.childIds;
        for (int index = last; index >= first; index--)
        {
            int removedId = childIds[index];
            for (int n = index + 1; n < node.childCount; n++) {
                childIds[n - 1] = childIds[n];
            }
            --node.childCount;
            childIds[node.childCount]   = 0; // not necessary but simplify debugging
            nodes[removedId].parentId   = Static.NoParentId;
            OnChildNodeRemove(node.id, removedId, index);
        }
    }
    
    // --- 3.3
    private void ChildIds_InsertRange(ref EntityNode node, int first, int last, ReadOnlySpan<int> newIds)
    {
        var childIds = node.childIds;
        for (int index = first; index <= last; index++)
        {
            for (int n = node.childCount; n > index; n--) {
                childIds[n] = childIds[n - 1];
            }
            var addedId     = newIds[index];
            childIds[index] = addedId;
            ++node.childCount;
            OnChildNodeAdd(node.id, addedId, index);
        }
    }


    private void SetChildParents(int[] ids, int count, int parentId)
    {
        var localNodes = nodes;
        for (int n = 0; n < count; n++)
        {
            var childId     = ids[n];
            ref var child   = ref localNodes[childId];
            if (child.parentId < Static.MinNodeId) {
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
    
    protected internal override void    UpdateEntityCompIndex(int id, int compIndex) {
        nodes[id].compIndex = compIndex;
    }
    
    internal int NewId()
    {
        var localNodes  = nodes;
        var max         = localNodes.Length;
        var id          = Interlocked.Increment(ref intern.sequenceId);
        for (; id < max;)
        {
            if ((localNodes[id].flags & Created) != 0) {
                id = Interlocked.Increment(ref intern.sequenceId);
                continue;
            }
            break;
        }
        return id;
    }
    
    /// <remarks> Set <see cref="EntityNode.archetype"/> = null. </remarks>
    internal void DeleteNode(int id)
    {
        entityCount--;
        var localNodes  = nodes;
        ref var node    = ref localNodes[id];
        
        // --- mark its child nodes as floating
        ClearTreeFlags(localNodes, id, TreeNode);
        foreach (var childId in node.ChildIds) {
            localNodes[childId].parentId = Static.NoParentId;
        }
        RemoveAllEntityEventHandlers(this, node);
        var parentId    = node.parentId;
        // --- clear node entry.
        //     Set node.archetype = null
        node            = new EntityNode(id); // clear node
        
        // --- remove child from parent 
        if (!HasParent(parentId)) {
            return;
        }
        int curIndex = RemoveChildNode(ref localNodes[parentId], id);
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
    
    private void SetStoreRootEntity(Entity entity) {
        if (!storeRoot.IsNull) {
            throw new InvalidOperationException($"EntityStore already has a {nameof(StoreRoot)}. {nameof(StoreRoot)} id: {storeRoot.Id}");
        }
        var id = entity.Id;
        ref var parentId = ref nodes[id].parentId;
        if (HasParent(parentId)) {
            throw new InvalidOperationException($"entity must not have a parent to be {nameof(StoreRoot)}. current parent id: {parentId}");
        }
        storeRoot   = entity;
        parentId    = Static.StoreRootParentId;
        SetTreeFlags(nodes, id, TreeNode);
    }
    
    // ---------------------------------- child nodes change notification ----------------------------------
    private void OnChildNodeAdd(int parentId, int childId, int childIndex)
    {
        var childEntitiesChanged = intern.childEntitiesChanged;
        if (childEntitiesChanged == null) {
            return;
        }
        var args = new ChildEntitiesChanged(ChildEntitiesChangedAction.Add, this, parentId, childId, childIndex);
        childEntitiesChanged(args);
    }
    
    private void OnChildNodeRemove(int parentId, int childId, int childIndex)
    {
        var childEntitiesChanged = intern.childEntitiesChanged;
        if (childEntitiesChanged == null) {
            return;
        }
        var args = new ChildEntitiesChanged(ChildEntitiesChangedAction.Remove, this, parentId, childId, childIndex);
        childEntitiesChanged(args);
    }
    
    
    // ------------------------------------- Entity access -------------------------------------
    internal TreeMembership  GetTreeMembership(int id) {
        return nodes[id].Is(TreeNode) ? TreeMembership.treeNode : TreeMembership.floating;
    }

    internal static Entity GetParent(EntityStore store, int id)
    {
        var parentNode  = store.nodes[id].parentId;
        parentNode      = HasParent(parentNode) ? parentNode : Static.NoParentId;
        return new Entity(store, parentNode); // ENTITY_STRUCT
    }
    
    internal static ChildEntities GetChildEntities(EntityStore store, int id)
    {
        ref var node    = ref store.nodes[id];
        return new ChildEntities(store, node.childIds, node.childCount);
    }
    
    internal static ReadOnlySpan<int> GetChildIds(EntityStore store, int id)
    {
        ref var node = ref store.nodes[id];
        return new ReadOnlySpan<int>(node.childIds, 0, node.childCount);
    }
}

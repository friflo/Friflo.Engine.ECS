// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
    }
    
    /// <summary>
    /// Set the seed used to create random entity <see cref="Entity.Pid"/>'s for an entity store <br/>
    /// created with <see cref="PidType"/> == <see cref="PidType.RandomPids"/>.
    /// </summary>
    public void SetRandomSeed(int seed) {
        extension.randPid = new Random(seed);
    }
    
    private static bool HasParent(int id)  =>   id >= Static.MinNodeId;
    
    internal int AddChild (int parentId, int childId)
    {
        var localNodes      = nodes;
    //  ref var childNode   = ref localNodes[childId];
    //  var curParentId     = childNode.parentId;
        extension.parentMap.TryGetValue(childId, out int curParentId);        
        if (HasParent(curParentId)) {
            if (curParentId == parentId) {
                // case: entity with given id is already a child of this entity
                return -1;
            }
            // --- remove child from current parent
        //  int curIndex = RemoveChildNode(ref localNodes[curParentId], curParentId, childId)
            int curIndex = RemoveChildNode(curParentId, childId);
            OnChildNodeRemove(curParentId, childId, curIndex);
        } else {
            if (parentId == childId) {
                // case: tried to add entity to itself as a child
                throw AddEntityAsChildToItselfException(parentId);
            }
        }
        // --- add entity with given id as child to this entity
    //  ref var parent      = ref localNodes[parentId];
        var parentEntity = new Entity(this, parentId);
        if (!parentEntity.HasTreeNode()) { // todo could optimize
            parentEntity.AddComponent<TreeNode>();
        }
        ref var parent      = ref parentEntity.GetTreeNode();
        int index           = parent.childCount;
    //  childNode.parentId  = parentId;
        extension.parentMap[childId]  = parentId;
        parent.childCount++;
        EnsureChildIdsCapacity(ref parent,  parent.childCount);
        parent.childIds[index] = childId;
        SetTreeFlags(localNodes, childId, nodes[parentId].flags & NodeFlags.TreeNode);
        
        OnChildNodeAdd(parentId, childId, index);
        return index;
    }
    
    internal void InsertChild (int parentId, int childId, int childIndex)
    {
        var localNodes  = nodes;
    //  ref var parent  = ref localNodes[parentId];
        var parentEntity = new Entity(this, parentId);
        if (!parentEntity.HasTreeNode()) {  // todo could optimize
            parentEntity.AddComponent<TreeNode>();
        }
        ref var parent = ref parentEntity.GetTreeNode();
        
        if (childIndex > parent.childCount) {
            throw new IndexOutOfRangeException();
        }
    //  ref var childNode   = ref localNodes[childId];
    //  var curParentId     = childNode.parentId;
        extension.parentMap.TryGetValue(childId, out int curParentId);
        if (HasParent(curParentId))
        {
            int curIndex;
        //  ref var curParent = ref localNodes[curParentId];
            var curParentEntity = new Entity(this, curParentId);
            if (curParentId != parentId) {
                // --- case: child has a different parent => remove node from current parent
                curIndex = RemoveChildNode(curParentId, childId);
                OnChildNodeRemove(curParentId, childId, curIndex);
                goto InsertNode;
            }
            // case: entity with given id is already a child of this entity => move child
            curIndex = GetChildIndex(curParentEntity, childId);
            if (curIndex == childIndex) {
                // case: child entity is already at the requested childIndex
                return;
            }
            curIndex = RemoveChildNode(curParentId, childId);
            OnChildNodeRemove(curParentId, childId, curIndex);
            
            InsertChildNode(ref parent, childId, childIndex);
            OnChildNodeAdd (parentId,                 childId, childIndex);
            return;
        }
    InsertNode:
        // --- insert entity with given id as child to its parent
    //  childNode.parentId  = parentId;
        extension.parentMap[childId] = parentId;
        InsertChildNode(ref parent, childId, childIndex);
        SetTreeFlags(localNodes, childId, nodes[parentId].flags & NodeFlags.TreeNode);
        
        OnChildNodeAdd(parentId,    childId, childIndex);
    }
    
    internal bool RemoveChild (int parentId, int childId)
    {
        var localNodes      = nodes;
    //  ref var childNode   = ref localNodes[childId];
        extension.parentMap.TryGetValue(childId, out int curParentId);
        if (parentId != curParentId) {
            return false;
        }
    //  childNode.parentId  = Static.NoParentId;
        extension.parentMap.Remove(childId);
        var curIndex        = RemoveChildNode(parentId, childId);
        ClearTreeFlags(localNodes, childId, NodeFlags.TreeNode);
        
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
    
    internal static int GetChildIndex(Entity parent, int childId)
    {
        parent.TryGetTreeNode(out var node);
        var childIds    = node.childIds;
        int count       = node.childCount;
        for (int n = 0; n < count; n++) {
            if (childId != childIds[n]) {
                continue;
            }
            return n;
        }
        return -1;
    }
    
    private int RemoveChildNode (int parentId, int childId)
    {
        var parent          = new Entity(this, parentId);
        ref var treeNode    = ref parent.GetTreeNode();
        var childIds        = treeNode.childIds;
        int count           = treeNode.childCount;
        for (int n = 0; n < count; n++) {
            if (childId != childIds[n]) {
                continue;
            }
            for (int i = n + 1; i < count; i++) {
                childIds[i - 1] = childIds[i];
            }
            treeNode.childCount   = --count;
            childIds[count]     = 0;  // clear last child id for debug clarity. not necessary because of childCount--
            return n;
        }
        throw new InvalidOperationException($"unexpected state: child id not found. parent id: {parentId}, child id: {childId}");
    }
    
    private static void InsertChildNode (ref TreeNode parent, int childId, int childIndex)
    {
        EnsureChildIdsCapacity(ref parent, parent.childCount + 1);
        var childIds = parent.childIds;
        for (int n = parent.childCount; n > childIndex; n--) {
            childIds[n] = childIds[n - 1];
        }
        childIds[childIndex] = childId;
        parent.childCount++;
    }
    
    private static void EnsureChildIdsCapacity(ref TreeNode parent, int length)
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
    
    private void SetChildNodes(Entity parent, ReadOnlySpan<int> newChildIds)
    {
        if (extension.childEntitiesChanged != null) {
            // case: childNodesChanged handler exists       => assign new child ids one by one to send events
            SetChildNodesWithEvents(parent, newChildIds);
            return;
        }
        // case: no registered childNodesChanged handlers   => assign new child ids at once
        if (newChildIds.Length == 0) { // todo fix
            return;
        }
    //  ref var node        = ref nodes[parent.Id];
        var     newCount    = newChildIds.Length;
        int[]   childIds;
        if (!parent.HasTreeNode()) {    // todo could optimize
            parent.AddComponent<TreeNode>();
        }
        ref var node = ref parent.GetTreeNode();
        if (newCount <= node.childCount) {
            childIds = node.childIds;
            newChildIds.CopyTo(childIds);
        } else {
            childIds = node.childIds = newChildIds.ToArray();
        }
        node.childCount = newCount;
        SetChildParents(childIds, newCount, parent.Id);
    }
    
    private void SetChildNodesWithEvents(Entity parent, ReadOnlySpan<int> newIds)
    {
        if (!parent.HasTreeNode()) {    // todo could optimize
            parent.AddComponent<TreeNode>();
        }
        ref var node    = ref parent.GetTreeNode();
        var newCount    = newIds.Length;
        var childIds    = node.childIds;
        if (childIds == null || newCount > childIds.Length) {
            ArrayUtils.Resize(ref node.childIds, newCount);
            childIds = node.childIds;
        }
        // --- 1. Remove missing ids in new child ids.          E.g.    cur ids [2, 3, 4, 5]
        //                                                             *newIds  [6, 4, 2, 5]    => remove: 3
        //                                                              result  [2, 4, 5]
        ChildIds_RemoveMissingIds(newIds, ref node, parent.Id);
        
        // --- 2. Insert new ids at their specified position.   E.g.    cur ids [2, 4, 5]
        //                                                             *newIds  [6, 4, 2, 5]    => insert: 6
        //                                                              result  [6, 2, 4, 5]    childCount = newCount
        ChildIds_InsertNewIds    (newIds, ref node, parent.Id);
        
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
        ChildIds_RemoveRange(ref node, first, last, parent.Id);
        ChildIds_InsertRange(ref node, first, last, newIds, parent.Id);
        
        SetChildParents(childIds, newCount, parent.Id);
    }

    // --- 1.
    private void ChildIds_RemoveMissingIds(ReadOnlySpan<int> newIds, ref TreeNode node, int parentId)
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
        //  nodes[id].parentId          = Static.NoParentId;
            extension.parentMap.Remove(id);
        //  OnChildNodeRemove(node.id, id, index);
            OnChildNodeRemove(parentId, id, index);
        }
    }

    // --- 2.
    private void ChildIds_InsertNewIds(ReadOnlySpan<int> newIds, ref TreeNode node, int parentId)
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
            OnChildNodeAdd(parentId, id, index);
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
    private void ChildIds_RemoveRange(ref TreeNode node, int first, int last, int parentId)
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
        //  nodes[removedId].parentId   = Static.NoParentId;
            extension.parentMap.Remove(removedId);
            OnChildNodeRemove(parentId, removedId, index);
        }
    }
    
    // --- 3.3
    private void ChildIds_InsertRange(ref TreeNode node, int first, int last, ReadOnlySpan<int> newIds, int parentId)
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
            OnChildNodeAdd(parentId, addedId, index);
        }
    }


    private void SetChildParents(int[] ids, int count, int parentId)
    {
    //  var localNodes = nodes;
        for (int n = 0; n < count; n++)
        {
            var childId     = ids[n];
        //  ref var child   = ref localNodes[childId];
            extension.parentMap.TryGetValue(childId, out int curParentId);
            if (curParentId < Static.MinNodeId) {
                extension.parentMap[childId] = parentId;
                if (HasCycle(parentId, childId, out var exception)) {
                    throw exception;
                }
                continue;
            }
            if (curParentId == parentId) {
                continue;
            }
            /* if (child.parentId < Static.MinNodeId) {
                child.parentId = parentId;
                if (HasCycle(parentId, childId, out var exception)) {
                    throw exception;
                }
                continue;
            }
            if (child.parentId == parentId) {
                continue;
            } */
            // could remove child from its current parent and set new child.parentId
            throw EntityAlreadyHasParent(childId, curParentId, parentId);
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
    //  var localNodes  = nodes;
        while (true) {
        //  cur = localNodes[cur].parentId;
            extension.parentMap.TryGetValue(cur, out cur);
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
        //  cur = nodes[cur].parentId;
            extension.parentMap.TryGetValue(cur, out cur);
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
    
    /// <summary> Note!  Sync implementation with <see cref="NewId"/>. </summary>
    private void NewIds(int[] ids, int start, int count)
    {
        var localNodes  = nodes;
        var max         = localNodes.Length;
        var sequenceId  = intern.sequenceId;
        for (int n = 0; n < count; n++)
        {
            for (; ++sequenceId < max;)
            {
                if ((localNodes[sequenceId].flags & Created) != 0) {
                    continue;
                }
                break;
            }
            ids[n + start] = sequenceId;
        }
        intern.sequenceId = sequenceId;
    }
    
    /// <summary> Note!  Sync implementation with <see cref="NewIdInterlocked"/>  and <see cref="NewIds"/>. </summary>
    internal int NewId()
    {
        var localNodes  = nodes;
        var max         = localNodes.Length;
        var id          = ++intern.sequenceId;
        for (; id < max;)
        {
            if ((localNodes[id].flags & Created) != 0) {
                id = ++intern.sequenceId;
                continue;
            }
            break;
        }
        return id;
    }
    
    /// <summary> Same as <see cref="NewId"/> but thread safe for <see cref="CommandBuffer"/>. </summary>
    internal int NewIdInterlocked()
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
    internal void DeleteNode(Entity entity)
    {
        int id = entity.Id;
        entityCount--;
    //  var localNodes  = nodes;
        ref var node    = ref nodes[id];
        
        // --- mark its child nodes as floating
        ClearTreeFlags(nodes, id, NodeFlags.TreeNode);
        foreach (var childId in entity.ChildIds) {
        //  localNodes[childId].parentId = Static.NoParentId;
            extension.parentMap.Remove(childId);
        }
        RemoveAllEntityEventHandlers(this, node, id);
    //  var parentId    = node.parentId;
        extension.parentMap.TryGetValue(id, out int parentId);
        // --- clear node entry.
        //     Set node.archetype = null
        node = default;
        extension.RemoveEntity(id);

        // --- remove child from parent 
        if (!HasParent(parentId)) {
            return;
        }
        int curIndex = RemoveChildNode(parentId, id);
        OnChildNodeRemove(parentId, id, curIndex);
    }
    
    private void SetTreeFlags(EntityNode[] nodes, int id, NodeFlags flag) {
        ref var node    = ref nodes[id];
        if (node.IsNot(Created) || node.Is(flag)) {
            return;
        }
        node.flags |= flag;
        var entity = new Entity(this, id);
        foreach (var childId in entity.ChildIds) {
            SetTreeFlags(nodes, childId, flag);
        }
    }
    
    private void ClearTreeFlags(EntityNode[] nodes, int id, NodeFlags flag) {
        ref var node    = ref nodes[id];
        if (node.IsNot(Created) || node.IsNot(flag)) {
            return;
        }
        node.flags &= ~flag;
        var entity = new Entity(this, id);
        foreach (var childId in entity.ChildIds) {
            ClearTreeFlags(nodes, childId, flag);
        }
    }
    
    private void SetStoreRootEntity(Entity entity) {
        if (!storeRoot.IsNull) {
            throw new InvalidOperationException($"EntityStore already has a {nameof(StoreRoot)}. {nameof(StoreRoot)} id: {storeRoot.Id}");
        }
        var id = entity.Id;
    //  ref var parentId = ref nodes[id].parentId;
        extension.parentMap.TryGetValue(id, out var parentId);
        if (HasParent(parentId)) {
            throw new InvalidOperationException($"entity must not have a parent to be {nameof(StoreRoot)}. current parent id: {parentId}");
        }
        storeRoot   = entity;
    //  parentId    = Static.StoreRootParentId;
    //  extension.parentMap.Add(id, Static.StoreRootParentId);
        SetTreeFlags(nodes, id, NodeFlags.TreeNode);
    }
    
    // ---------------------------------- child nodes change notification ----------------------------------
    private void OnChildNodeAdd(int parentId, int childId, int childIndex)
    {
        var childEntitiesChanged = extension.childEntitiesChanged;
        if (childEntitiesChanged == null) {
            return;
        }
        var args = new ChildEntitiesChanged(ChildEntitiesChangedAction.Add, this, parentId, childId, childIndex);
        childEntitiesChanged(args);
    }
    
    private void OnChildNodeRemove(int parentId, int childId, int childIndex)
    {
        var childEntitiesChanged = extension.childEntitiesChanged;
        if (childEntitiesChanged == null) {
            return;
        }
        var args = new ChildEntitiesChanged(ChildEntitiesChangedAction.Remove, this, parentId, childId, childIndex);
        childEntitiesChanged(args);
    }
    
    
    // ------------------------------------- Entity access -------------------------------------
    internal TreeMembership  GetTreeMembership(int id) {
        return nodes[id].Is(NodeFlags.TreeNode) ? TreeMembership.treeNode : TreeMembership.floating;
    }

    public int GetInternalParentId(int id)
    {
    //    var parentNode  = store.nodes[id].parentId;
    //    parentNode      = HasParent(parentNode) ? parentNode : Static.NoParentId;
        extension.parentMap.TryGetValue(id, out int parentId);
    //  parentId = HasParent(parentId) ? parentId : Static.NoParentId;
        return parentId; // ENTITY_STRUCT
    }
    
    internal static ChildEntities GetChildEntities(Entity entity)
    {
        return new ChildEntities(entity);
    }
    
    internal static ReadOnlySpan<int> GetChildIds(Entity entity)
    {
    //  ref var node = ref store.nodes[id];
    //  return new ReadOnlySpan<int>(node.childIds, 0, node.childCount);
        entity.TryGetTreeNode(out var node);
        return new ReadOnlySpan<int>(node.childIds, 0, node.childCount);
    }
}

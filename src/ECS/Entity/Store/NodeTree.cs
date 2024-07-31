// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Engine.ECS.Collections;
using Friflo.Engine.ECS.Index;

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
        int curLength   = nodes.Length;
        int last        = intern.sequenceId + 1;
        int curCapacity = curLength - last;
        if (curCapacity >= capacity) {
            return curCapacity;
        }
        int newLength   = last + capacity;
        ArrayUtils.Resize(ref nodes, newLength);
        return newLength - last;
    }
    
    private void EnsureNodesLength(int length)
    {
        int curLength = nodes.Length;
        if (length <= curLength) {
            return;
        }
        int newLength = Math.Max(length, 2 * curLength); // could grow slower to minimize heap pressure
        ArrayUtils.Resize(ref nodes, newLength);
    }
    
    /// <summary>
    /// Set the seed used to create random entity <see cref="Entity.Pid"/>'s for an entity store <br/>
    /// created with <see cref="PidType"/> == <see cref="PidType.RandomPids"/>.
    /// </summary>
    public void SetRandomSeed(int seed) {
        extension.randPid = new Random(seed);
    }
    
    /// This message must be used if adding or removing ids from an entity <see cref="TreeNode"/>.
    private static ref TreeNode GetTreeNodeRef(Entity entity)
    {
        var heap = entity.archetype.heapMap[StructInfo<TreeNode>.Index];
        if (heap == null) {
            entity.AddComponent<TreeNode>(); // set entity.archetype
            heap = entity.archetype.heapMap[StructInfo<TreeNode>.Index];
        } 
        return ref ((StructHeap<TreeNode>)heap).components[entity.compIndex];
    }
    
#region get / set parent
    private static bool HasParent(int id)  =>   id >= Static.MinNodeId;

    private int GetTreeParentId(int entityId)
    {
        var parentMap = extension.parentMap;
        if (entityId >= parentMap.Length) {
            return Static.NoParentId;
        }
        return parentMap[entityId];
    }
    
    private void RemoveTreeParent(int entityId) {
        extension.parentMap[entityId] = Static.NoParentId;
    }
    
    internal void SetTreeParent(int entityId, int parentId)
    {
        var parentMap = extension.parentMap;
        if (entityId >= parentMap.Length) {
            extension.parentMap = ArrayUtils.Resize(ref parentMap, nodes.Length);
        }
        parentMap[entityId] = parentId;
    }
    
    internal TreeMembership  GetTreeMembership(int id)
    {
        while (true)
        {
            var parentId = GetTreeParentId(id);
            if (parentId == Static.NoParentId) {
                return storeRoot.Id == id ? TreeMembership.treeNode : TreeMembership.floating;
            }
            id = parentId;
        }
    }
    
    private bool WouldCreateCycle(int id, int parentId)
    {
        if (id == parentId) {
            return true;
        }
        while (true) {
            var parent = GetTreeParentId(id);
            if (parent == Static.NoParentId) {
                return false;
            }
            if (parent == parentId) {
                return true;
            }
            id = parent;
        }
    }
    #endregion
    
    internal int AddChild (int parentId, int childId)
    {
        if (WouldCreateCycle(parentId, childId)) {
            throw OperationCycleException(parentId, childId);
        }
        var curParentId = GetTreeParentId(childId);
        if (HasParent(curParentId)) {
            if (curParentId == parentId) {
                // case: entity with given id is already a child of this entity
                return -1;
            }
            // --- remove child from current parent
        //  int curIndex = RemoveChildNode(ref localNodes[curParentId], curParentId, childId)
            int curIndex = RemoveChildNode(curParentId, childId);
            OnChildNodeRemove(curParentId, childId, curIndex);
        }
        // --- add entity with given id as child to this entity
        var parentEntity    = new Entity(this, parentId);
        ref var parent      = ref GetTreeNodeRef(parentEntity);
        int index           = parent.childIds.count;
        SetTreeParent(childId, parentId);
        
        parent.childIds.Add(childId, extension.childHeap);
        // SetTreeFlags(nodes, childId, nodes[parentId].flags & NodeFlags.TreeNode);
        
        OnChildNodeAdd(parentId, childId, index);
        return index;
    }
    
    internal void InsertChild (int parentId, int childId, int childIndex)
    {
        if (WouldCreateCycle(parentId, childId)) {
            throw OperationCycleException(parentId, childId);
        }
        var parentEntity    = new Entity(this, parentId);
        ref var parent      = ref GetTreeNodeRef(parentEntity);
        if (childIndex > parent.childIds.count) {
            throw new IndexOutOfRangeException();
        }
        var curParentId = GetTreeParentId(childId);
        if (HasParent(curParentId))
        {
            int curIndex;
            var curParentEntity = new Entity(this, curParentId);
            if (curParentId != parentId) {
                // --- case: child has a different parent => remove node from current parent
                curIndex = RemoveChildNode(curParentId, childId);
                OnChildNodeRemove(curParentId, childId, curIndex);
                goto InsertNode;
            }
            // case: entity with given id is already a child of this entity => move child
            var childIds    = GetChildIds(curParentEntity);
            curIndex        = childIds.LastIndexOf(childId); // enable O(1) if childIndex == last child
            if (curIndex == childIndex) {
                // case: child entity is already at the requested childIndex
                return;
            }
            curIndex = RemoveChildNode(curParentId, childId);
            OnChildNodeRemove(curParentId, childId, curIndex);
            
            parent.childIds.InsertAt(childIndex, childId, extension.childHeap);
            OnChildNodeAdd (parentId,            childId, childIndex);
            return;
        }
    InsertNode:
        // --- insert entity with given id as child to its parent
        SetTreeParent(childId, parentId);
        parent.childIds.InsertAt(childIndex, childId, extension.childHeap);
        // SetTreeFlags(nodes, childId, nodes[parentId].flags & NodeFlags.TreeNode);
        
        OnChildNodeAdd(parentId,    childId, childIndex);
    }
    
    internal bool RemoveChild (int parentId, int childId)
    {
        int curParentId = GetTreeParentId(childId);
        if (parentId != curParentId) {
            return false;
        }
        RemoveTreeParent(childId);
        int curIndex        = RemoveChildNode(parentId, childId);
        // ClearTreeFlags(nodes, childId, NodeFlags.TreeNode);
        
        OnChildNodeRemove(parentId, childId, curIndex);
        return true;
    }
    
    internal static int GetChildIndex(Entity parent, int childId)
    {
        var childIds = GetChildIds(parent);
        return childIds.IndexOf(childId);
    }
    
    private int RemoveChildNode (int parentId, int childId)
    {
        var parent          = new Entity(this, parentId);
        ref var treeNode    = ref GetTreeNodeRef(parent);
        var childIds        = treeNode.GetChildIds(this);
        int index           = childIds.LastIndexOf(childId); // enable O(1) if childId == last child
        if (index != -1) {
            treeNode.childIds.RemoveAt(index, extension.childHeap, keepOrder: true);
            return index;
        }
        throw new InvalidOperationException($"unexpected state: child id not found. parent id: {parentId}, child id: {childId}");
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
        ref var node = ref GetTreeNodeRef(parent);
        node.childIds.SetArray(newChildIds, extension.childHeap);
        SetChildParents(node, parent.Id);
    }
    
    private void SetChildNodesWithEvents(Entity parent, ReadOnlySpan<int> newIds)
    {
        ref var node    = ref GetTreeNodeRef(parent);
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
        ChildIds_GetRange   (    node, newIds, out int first, out int last);
        ChildIds_RemoveRange(ref node, first, last, parent.Id);
        ChildIds_InsertRange(ref node, first, last, newIds, parent.Id);
        
        SetChildParents     (    node, parent.Id);
    }

    // --- 1.
    private void ChildIds_RemoveMissingIds(ReadOnlySpan<int> newIds, ref TreeNode node, int parentId)
    {
        var newIdSet = idBufferSet;
        newIdSet.Clear();
        foreach (int id in newIds) {
            newIdSet.Add(id);
        }
        var childIds = node.GetChildIds(this);
        for (int index = node.childIds.count - 1; index >= 0; index--) {
            int id = childIds[index];
            if (newIdSet.Contains(id)) {
                continue;
            }
            node.childIds.RemoveAt(index, extension.childHeap, keepOrder: true);
            RemoveTreeParent(id);
            OnChildNodeRemove(parentId, id, index);
        }
    }

    // --- 2.
    private void ChildIds_InsertNewIds(ReadOnlySpan<int> newIds, ref TreeNode node, int parentId)
    {
        var curIdSet = idBufferSet;
        curIdSet.Clear();
        foreach (int id in node.GetChildIds(this)) {
            curIdSet.Add(id);
        }
        int newCount = newIds.Length;
        for (int index = 0; index < newCount; index++)
        {
            int id = newIds[index];
            if (curIdSet.Contains(id)) {
                // case: child ids contains id already
                continue;
            }
            // case: child ids does not contain id      => insert at specified position
            node.childIds.InsertAt(index, id, extension.childHeap);
            OnChildNodeAdd(parentId, id, index);
        }
    }

    // --- 3.1
    private void ChildIds_GetRange(in TreeNode node, ReadOnlySpan<int> newIds, out int first, out int last)
    {
        var childIds    = node.GetChildIds(this);
        int count       = newIds.Length;
        first           = 0;
        for (; first < count; first++)
        {
            if (childIds[first] == newIds[first]) {
                // case: id is already at specified position
                continue;
            }
            break;
        }
        last = count - 1;
        for (; last > first; last--)
        {
            if (childIds[last] == newIds[last]) {
                // case: id is already at specified position
                continue;
            }
            break;
        }
    }
    
    // --- 3.2
    private void ChildIds_RemoveRange(ref TreeNode node, int first, int last, int parentId)
    {
        var heap = extension.childHeap;
        for (int index = last; index >= first; index--)
        {
            int removedId = node.childIds.GetAt(index, heap);
            node.childIds.RemoveAt(index, heap, keepOrder: true);
            RemoveTreeParent(removedId);
            OnChildNodeRemove(parentId, removedId, index);
        }
    }
    
    // --- 3.3
    private void ChildIds_InsertRange(ref TreeNode node, int first, int last, ReadOnlySpan<int> newIds, int parentId)
    {
        var heap = extension.childHeap;
        for (int index = first; index <= last; index++)
        {
            int addedId = newIds[index];
            node.childIds.InsertAt(index, addedId, heap);
            OnChildNodeAdd(parentId, addedId, index);
        }
    }

    private void SetChildParents(in TreeNode node, int parentId)
    {
        foreach (int childId in node.GetChildIds(this))
        {
            int curParentId = GetTreeParentId(childId);
            if (curParentId == Static.NoParentId) {
                SetTreeParent(childId, parentId);
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
        int cur         = id;
        while (true) {
            cur = GetTreeParentId(cur);
            if (cur == Static.NoParentId) {
                exception = null;
                return false;
            }
            if (cur == id) {
                exception = CycleException("cycle in entity children: ", id, id);
                return true;
            }
        }
    }
    
    private InvalidOperationException OperationCycleException(int id, int other) {
        if (id == other) {
            return new InvalidOperationException($"operation would cause a cycle: {id} -> {id}");
        }
        return CycleException("operation would cause a cycle: ", id, other);
    }
    
    private InvalidOperationException CycleException(string message, int id, int other)
    {
        int cur = id;
        var sb = new StringBuilder();
        sb.Append(message);
        sb.Append(id);
        while (true)
        {
            cur = GetTreeParentId(cur);
            sb.Append(" -> ");
            sb.Append(cur);
            if (cur != other) {
                continue;
            }
            break;
        }
        if (other != id) {
            sb.Append(" -> ");
            sb.Append(id);
        }
        return new InvalidOperationException(sb.ToString());
    }
    
    protected internal override void    UpdateEntityCompIndex(int id, int compIndex) {
        nodes[id].compIndex = compIndex;
    }
    
    /// <summary> Note!  Sync implementation with <see cref="NewId"/>. </summary>
    private void NewIds(int[] ids, int start, int count, Archetype archetype)
    {
        var localNodes  = nodes;
        int n = 0;
        if (recycleIds) {
            for (; n < count; n++)
            {
                if (!intern.recycleIds.TryPop(out int id)) {
                    break;
                }
                localNodes[id].archetype    = archetype; // mark created. So id is not used twice by loop below
                ids[n + start]              = id;
            }
        }
        int max         = localNodes.Length;
        int sequenceId  = intern.sequenceId;
        for (; n < count; n++)
        {
            for (; ++sequenceId < max;)
            {
                if (localNodes[sequenceId].archetype != null) {
                    continue;
                }
                break;
            }
            ids[n + start] = sequenceId;
        }
        intern.sequenceId = sequenceId;
    }

    /// <summary> Note!  Sync implementation with <see cref="NewIds"/>. </summary>
    internal int NewId()
    {
        var localNodes = nodes;
        int id;
        if (recycleIds) {
            while (intern.recycleIds.TryPop(out id))
            {
                if (localNodes[id].archetype != null) {
                    continue;
                }
                return id;
            }
        }
        int max = localNodes.Length;
        id      = ++intern.sequenceId;
        for (; id < max;)
        {
            if (localNodes[id].archetype != null) {
                id = ++intern.sequenceId;
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
        if (recycleIds) {
            intern.recycleIds.Push(id);
        }
        entityCount--;
        ref var node = ref nodes[id];
        if (node.isOwner != 0) {
            RemoveEntityReferences(entity, node);
        }
        if (node.isLinked != 0) {
            RemoveLinksToEntity(entity);
        }
        // ClearTreeFlags(nodes, id, NodeFlags.TreeNode); // --- mark its child nodes as floating
        foreach (int childId in entity.ChildIds) {
            RemoveTreeParent(childId);
        }
        RemoveAllEntityEventHandlers(this, node, id);
        int parentId = GetTreeParentId(id);
        // --- clear node entry.
        var revision    = node.revision;
        node = default;
        node.revision   = revision;        
        extension.RemoveEntity(id);

        // --- remove child from parent 
        if (!HasParent(parentId)) {
            return;
        }
        int curIndex = RemoveChildNode(parentId, id);
        OnChildNodeRemove(parentId, id, curIndex);
    }
    
    private void RemoveEntityReferences(Entity entity, in EntityNode node)
    {
        var indexTypes          = new ComponentTypes();
        var relationTypes       = new ComponentTypes();
        var schema              = Static.EntitySchema;
        var isOwner             = node.isOwner;
        indexTypes.bitSet.l0    = schema.indexTypes.   bitSet.l0 & isOwner; // intersect
        relationTypes.bitSet.l0 = schema.relationTypes.bitSet.l0 & isOwner; // intersect
        
        // --- remove entity id from component index
        var indexMap = extension.indexMap;
        foreach (var componentType in indexTypes) {
            var componentIndex = indexMap[componentType.StructIndex];
            componentIndex.RemoveEntityFromIndex(entity.Id, node.archetype, node.compIndex);
        }
        // --- remove entity relations from entity
        var relationsMap = extension.relationsMap;
        foreach (var componentType in relationTypes) {
            var relations = relationsMap[componentType.StructIndex];
            relations.RemoveEntityRelations(entity.Id);
        }
    }
    
    private void RemoveLinksToEntity(Entity target)
    {
        EntityExtensions.GetIncomingLinkTypes(target, out var indexTypes, out var relationTypes);
        
        // --- remove link components from entities having the passed entity id as target
        var indexMap = extension.indexMap;
        foreach (var componentType in indexTypes) {
            var entityIndex = (EntityIndex)indexMap[componentType.StructIndex];
            entityIndex.RemoveLinksWithTarget(target.Id);
        }
        // --- remove link relations from entities having the passed entity id as target
        var relationsMap = extension.relationsMap;
        foreach (var componentType in relationTypes) {
            var relations = relationsMap[componentType.StructIndex];
            relations.RemoveLinksWithTarget(target.Id);
        }
    }
    
    /* private void SetTreeFlags(EntityNode[] nodes, int id, NodeFlags flag) {
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
        foreach (int childId in entity.ChildIds) {
            ClearTreeFlags(nodes, childId, flag);
        }
    } */
    
    private void SetStoreRootEntity(Entity entity) {
        if (!storeRoot.IsNull) {
            throw new InvalidOperationException($"EntityStore already has a {nameof(StoreRoot)}. {nameof(StoreRoot)} id: {storeRoot.Id}");
        }
        int id = entity.Id;
        var parentId = GetTreeParentId(id);
        if (HasParent(parentId)) {
            throw new InvalidOperationException($"entity must not have a parent to be {nameof(StoreRoot)}. current parent id: {parentId}");
        }
        storeRoot   = entity;
        // SetTreeFlags(nodes, id, NodeFlags.TreeNode);
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


    public int GetInternalParentId(int id) => GetTreeParentId(id);
    
    internal static ReadOnlySpan<int> GetChildIds(Entity entity)
    {
        entity.TryGetTreeNode(out var node);
        return node.GetChildIds(entity.store);
    }
}

// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Threading;
using Friflo.Engine.ECS.Collections;
using Friflo.Engine.ECS.Index;
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
    
    private static bool HasParent(int id)  =>   id >= Static.MinNodeId;
    
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
        /* if (!parentEntity.HasTreeNode()) { // todo could optimize
            parentEntity.AddComponent(new TreeNode(extension.hierarchyHeap));
        }*/
        ref var parent      = ref GetTreeNodeRef(parentEntity);
        int index           = parent.childIds.count;
    //  childNode.parentId  = parentId;
        extension.parentMap[childId]  = parentId;
        parent.childIds.Add(childId, extension.childHeap);
        SetTreeFlags(localNodes, childId, nodes[parentId].flags & NodeFlags.TreeNode);
        
        OnChildNodeAdd(parentId, childId, index);
        return index;
    }
    
    internal void InsertChild (int parentId, int childId, int childIndex)
    {
        var localNodes  = nodes;
    //  ref var parent  = ref localNodes[parentId];
        var parentEntity = new Entity(this, parentId);
        /* if (!parentEntity.HasTreeNode()) {  // todo could optimize
            parentEntity.AddComponent(new TreeNode(extension.hierarchyHeap));
        } */
        ref var parent = ref GetTreeNodeRef(parentEntity);
        
        if (childIndex > parent.childIds.count) {
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
            
            parent.childIds.InsertAt(childIndex, childId, extension.childHeap);
            OnChildNodeAdd (parentId,            childId, childIndex);
            return;
        }
    InsertNode:
        // --- insert entity with given id as child to its parent
    //  childNode.parentId  = parentId;
        extension.parentMap[childId] = parentId;
        parent.childIds.InsertAt(childIndex, childId, extension.childHeap);
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
        int curIndex        = RemoveChildNode(parentId, childId);
        ClearTreeFlags(localNodes, childId, NodeFlags.TreeNode);
        
        OnChildNodeRemove(parentId, childId, curIndex);
        return true;
    }
    
    internal static int GetChildIndex(Entity parent, int childId)
    {
        parent.TryGetTreeNode(out var node);
        var childIds = node.GetChildIds(parent.store);
        return childIds.IndexOf(childId);
    }
    
    private int RemoveChildNode (int parentId, int childId)
    {
        var parent          = new Entity(this, parentId);
        ref var treeNode    = ref GetTreeNodeRef(parent);
        var childIds        = treeNode.GetChildIds(this);
        int index           = childIds.IndexOf(childId);
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
    //  ref var node        = ref nodes[parent.Id];

        /* if (!parent.HasTreeNode()) {    // todo could optimize
            parent.AddComponent(new TreeNode(extension.hierarchyHeap));
        } */
        ref var node = ref GetTreeNodeRef(parent);
        node.childIds.SetArray(newChildIds, extension.childHeap);
        SetChildParents(node, parent.Id);
    }
    
    private void SetChildNodesWithEvents(Entity parent, ReadOnlySpan<int> newIds)
    {
        /* if (!parent.HasTreeNode()) {    // todo could optimize
            parent.AddComponent(new TreeNode(extension.hierarchyHeap));
        } */
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
        //  nodes[id].parentId          = Static.NoParentId;
            extension.parentMap.Remove(id);
        //  OnChildNodeRemove(node.id, id, index);
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
        //  nodes[removedId].parentId   = Static.NoParentId;
            extension.parentMap.Remove(removedId);
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
    //  var localNodes = nodes;
        foreach (int childId in node.GetChildIds(this))
        {
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
        int cur         = id;
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
        int cur = id;
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
        int max         = localNodes.Length;
        int sequenceId  = intern.sequenceId;
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
        int max         = localNodes.Length;
        int id          = ++intern.sequenceId;
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
        int max         = localNodes.Length;
        int id          = Interlocked.Increment(ref intern.sequenceId);
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
        if (node.isOwner != 0) {
            RemoveEntityReferences(entity, node);
        }
        if (node.isLinked != 0) {
            RemoveLinksToEntity(entity);
        }
        // --- mark its child nodes as floating
        ClearTreeFlags(nodes, id, NodeFlags.TreeNode);
        foreach (int childId in entity.ChildIds) {
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
        foreach (int childId in entity.ChildIds) {
            ClearTreeFlags(nodes, childId, flag);
        }
    }
    
    private void SetStoreRootEntity(Entity entity) {
        if (!storeRoot.IsNull) {
            throw new InvalidOperationException($"EntityStore already has a {nameof(StoreRoot)}. {nameof(StoreRoot)} id: {storeRoot.Id}");
        }
        int id = entity.Id;
    //  ref var parentId = ref nodes[id].parentId;
        extension.parentMap.TryGetValue(id, out int parentId);
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
        return node.GetChildIds(entity.store);
    }
}

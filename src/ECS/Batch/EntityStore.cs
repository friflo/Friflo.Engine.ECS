// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseNullPropagation
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/*
public class BatchInUseException : InvalidOperationException {
    internal BatchInUseException(string message) : base (message) {}
} */

public partial class EntityStoreBase
{
#region EntityBatch
    [Browse(Never)]
    internal int PooledEntityBatchCount => internBase.entityBatches.Count;

    internal EntityBatch GetBatch(int entityId)
    {
        if (!internBase.entityBatches.TryPop(out var batch)) {
            batch = new EntityBatch(this);
        }
        batch.entityId  = entityId;
        batch.owner     = BatchOwner.EntityBatch;
        return batch;
    }
    
    internal void ApplyBatchTo(EntityBatch batch, int entityId)
    {
        ref var node    = ref ((EntityStore)this).nodes[entityId];
        var archetype   = node.archetype;
        var compIndex   = node.compIndex;
        
        // --- apply AddTag() / RemoveTag() commands
        var newTags     = archetype.tags;
        newTags.Add    (batch.tagsAdd);
        newTags.Remove (batch.tagsRemove);
        
        // --- apply AddComponent() / RemoveComponent() commands
        var newComponentTypes = archetype.componentTypes;
        newComponentTypes.Add   (batch.componentsAdd);
        newComponentTypes.Remove(batch.componentsRemove);
        
        // --- change archetype
        var newArchetype = GetArchetype(newComponentTypes, newTags);
        if (newArchetype != archetype) {
            node.compIndex  = compIndex = Archetype.MoveEntityTo(archetype, entityId, compIndex, newArchetype);
            node.archetype  = newArchetype;
        }
        
        // --- assign AddComponent() values
        var newHeapMap  = newArchetype.heapMap;
        var components  = batch.batchComponents;
        foreach (var componentType in batch.componentsAdd) {
            newHeapMap[componentType.StructIndex].SetBatchComponent(components, compIndex);
        }
        
        // ----------- Send events for all batch commands. See: SEND_EVENT notes
        // --- send tags changed event
        var tagsChanged = internBase.tagsChanged;
        if (tagsChanged != null) {
            if (!newTags.bitSet.Equals(archetype.tags.bitSet)) {
                tagsChanged.Invoke(new TagsChanged(this, entityId, newTags, archetype.tags));
            }
        }
        // --- send component removed event
        if (internBase.componentRemoved != null) {
            SendComponentRemoved(batch, entityId, archetype, compIndex);
        }
        // --- send component added event
        if (internBase.componentAdded != null) {
            SendComponentAdded  (batch, entityId, archetype, compIndex);
        }
    }
    
    private void SendComponentAdded(EntityBatch batch, int entityId, Archetype archetype, int compIndex)
    {
        var oldHeapMap      = archetype.heapMap;
        var componentAdded  = internBase.componentAdded;
        foreach (var componentType in batch.componentsAdd)
        {
            var structIndex = componentType.StructIndex;
            var structHeap  = oldHeapMap[structIndex];
            ComponentChangedAction action;
            if (structHeap == null) {
                action = ComponentChangedAction.Add;
            } else {
                // --- case: archetype contains the component type  => archetype remains unchanged
                structHeap.StashComponent(compIndex);
                action = ComponentChangedAction.Update;
            }
            componentAdded.Invoke(new ComponentChanged (this, entityId, action, structIndex, structHeap));
        }
    }
    
    private void SendComponentRemoved(EntityBatch batch, int entityId, Archetype archetype, int compIndex)
    {
        var oldHeapMap          = archetype.heapMap;
        var componentRemoved    = internBase.componentRemoved;
        foreach (var componentType in batch.componentsRemove)
        {
            var structIndex = componentType.StructIndex;
            var oldHeap     = oldHeapMap[structIndex];
            if (oldHeap == null) {
                continue;
            }
            oldHeap.StashComponent(compIndex);
            componentRemoved.Invoke(new ComponentChanged (this, entityId, ComponentChangedAction.Remove, structIndex, oldHeap));
        }
    }
    #endregion
    
#region CreateEntityBatch
    internal void ReturnBatch(EntityBatch batch) {
        internBase.entityBatches.Push(batch);
    }
    
    /// <summary>
    /// Returns a <see cref="CreateEntityBatch"/> used to create entities with components and tags added to the batch.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#batch---create-entity">Example.</a>
    /// </summary>
    /// <remarks>
    /// The returned batch creates an entity with previously added components and tags when calling
    /// <see cref="CreateEntityBatch.CreateEntity()"/>.<br/>
    /// <br/>
    /// If <paramref name="autoReturn"/> == true the batch is returned to the EntityStore when
    /// calling <see cref="CreateEntityBatch.CreateEntity()"/>.<br/>
    /// Subsequent use of the batch throws <see cref="BatchAlreadyReturnedException"/>.<br/>
    /// <br/>
    /// If <paramref name="autoReturn"/> == false <see cref="CreateEntityBatch.CreateEntity()"/> can be called
    /// multiple times to create multiple entities.<br/>
    /// The caller should call <see cref="CreateEntityBatch.Return"/> after usage to prevent unnecessary memory allocations.<br/>
    /// <br/>
    /// When calling <see cref="CreateEntityBatch.CreateEntity()"/> or <see cref="CreateEntityBatch.Return"/>
    /// the batch executes without memory allocations.
    /// </remarks>
    public CreateEntityBatch Batch(bool autoReturn = true)
    {
        if (!internBase.createEntityBatches.TryPop(out var batch)) {
            batch = new CreateEntityBatch(this);
        }
        batch.autoReturn = autoReturn;
        batch.isReturned = false;
        return batch;
    }
    
    [Browse(Never)]
    internal int PooledCreateEntityBatchCount => internBase.createEntityBatches.Count;
    
    internal void ReturnCreateBatch(CreateEntityBatch batch) => internBase.createEntityBatches.Push(batch);
    #endregion
    
#region EntityList
    internal EntityList GetEntityList()
    {
        if (!internBase.entityLists.TryPop(out var list)) {
            list = new EntityList((EntityStore)this);
        }
        return list;
    }
    
    internal void ReturnEntityList(EntityList list)
    {
        internBase.entityLists.Push(list);
    }
    #endregion
}
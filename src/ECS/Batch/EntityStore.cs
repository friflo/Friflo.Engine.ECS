// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable UseNullPropagation
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    internal EntityBatch GetBatch(int entityId)
    {
        var batch = internBase.entityBatch ??= new EntityBatch(this);
        batch.Clear();
        batch.entityId = entityId;
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
    
    internal CreateEntityBatch GetCreateBatch()
    {
        var batch = internBase.createEntityBatch ??= new CreateEntityBatch(this);
        batch.Clear();
        return batch;
    }
}
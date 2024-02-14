// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
// ReSharper disable UseNullPropagation
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    internal EntityBatch GetBatch(int entityId)
    {
        var batch       = internBase.batch ??= new EntityBatch(this);
        batch.entityId  = entityId;
        return batch;
    }
    
    internal void ApplyEntityBatch(EntityBatch batch)
    {
        var entityId    = batch.entityId;
        ref var node    = ref batch.entityStore.nodes[entityId];
        var archetype   = node.archetype;
        var compIndex   = node.compIndex;
        
        var newTags     = archetype.tags;
        newTags.Add    (batch.addedTags);
        newTags.Remove (batch.removedTags);
        
        var newComponentTypes = archetype.componentTypes;
        newComponentTypes.Add   (batch.addedComponents);
        newComponentTypes.Remove(batch.removedComponents);
        
        var newArchetype    = GetArchetype(newComponentTypes, newTags);
        node.compIndex      = compIndex = Archetype.MoveEntityTo(archetype, entityId, compIndex, newArchetype);
        node.archetype      = newArchetype;
        
        var newHeapMap = newArchetype.heapMap;
        foreach (var componentType in batch.addedComponents)
        {
            var structIndex = componentType.StructIndex;
            newHeapMap[structIndex].SetBatchComponent(batch.components, compIndex);
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
            SendComponentRemoved(batch, archetype, entityId);
        }
        // --- send component added event
        if (internBase.componentAdded != null) {
            SendComponentAdded(batch, archetype, compIndex);
        }
    }
    
    private void SendComponentAdded(EntityBatch batch, Archetype archetype, int compIndex)
    {
        var oldHeapMap      = archetype.heapMap;
        var componentAdded  = internBase.componentAdded;
        foreach (var componentType in batch.addedComponents)
        {
            var structIndex = componentType.StructIndex;
            var structHeap  = oldHeapMap[structIndex];
            var oldHeap     = structHeap;
            ComponentChangedAction action;
            if (structHeap == null) {
                action = ComponentChangedAction.Add;
            } else {
                // --- case: archetype contains the component type  => archetype remains unchanged
                oldHeap.StashComponent(compIndex);
                action = ComponentChangedAction.Update;
            }
            componentAdded.Invoke(new ComponentChanged (this, batch.entityId, action, structIndex, oldHeap));
        }
    }
    
    private void SendComponentRemoved(EntityBatch batch, Archetype archetype, int compIndex)
    {
        var oldHeapMap          = archetype.heapMap;
        var componentRemoved    = internBase.componentRemoved;
        foreach (var componentType in batch.removedComponents)
        {
            var structIndex = componentType.StructIndex;
            var oldHeap     = oldHeapMap[structIndex];
            if (oldHeap == null) {
                continue;
            }
            oldHeap.StashComponent(compIndex);
            componentRemoved.Invoke(new ComponentChanged (this, batch.entityId, ComponentChangedAction.Remove, structIndex, oldHeap));
        }
    }
}
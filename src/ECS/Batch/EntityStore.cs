// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
        
    internal EntityBatch GetBatch(int entityId)
    {
        var batch       = internBase.batch ??= new EntityBatch(this);
        batch.entityId  = entityId;
        return batch;
    }
    
    internal void Apply(EntityBatch batch)
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
        node.compIndex      = Archetype.MoveEntityTo(archetype, entityId, compIndex, newArchetype);
        node.archetype      = newArchetype;
        
        var tagsChanged = internBase.tagsChanged;
        if (tagsChanged != null) {
            if (!newTags.bitSet.Equals(archetype.tags.bitSet)) {
                // Send event. See: SEND_EVENT notes
                tagsChanged.Invoke(new TagsChanged(this, entityId, newTags, archetype.tags));
            }   
        }

        var oldHeapMap      = archetype.heapMap;
        var componentAdded  = internBase.componentAdded;
        if (componentAdded != null)
        {
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
                componentAdded.Invoke(new ComponentChanged (this, entityId, action, structIndex, oldHeap));
            }
        }
        var componentRemoved  = internBase.componentRemoved;
        if (componentRemoved != null)
        {
            foreach (var componentType in batch.removedComponents)
            {
                var structIndex = componentType.StructIndex;
                var structHeap  = oldHeapMap[structIndex];
                var oldHeap     = structHeap;
                if (structHeap == null) {
                    continue;
                }
                oldHeap.StashComponent(compIndex);
                componentRemoved.Invoke(new ComponentChanged (this, entityId, ComponentChangedAction.Remove, structIndex, oldHeap));
            }
        }
    }
}
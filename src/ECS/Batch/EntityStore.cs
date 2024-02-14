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
        var newTags     = archetype.tags;
        newTags.Add    (batch.addedTags);
        newTags.Remove (batch.removedTags);
        var componentTypes = archetype.componentTypes;
        
        var newArchetype = GetArchetype(componentTypes, newTags);
        Archetype.MoveEntityTo(archetype, entityId, node.compIndex, newArchetype);
        
        if (!newTags.bitSet.Equals(archetype.tags.bitSet)) {
            var tagsChanged = internBase.tagsChanged;
            if (tagsChanged != null) {
                tagsChanged.Invoke(new TagsChanged(this, entityId, newTags, archetype.tags));
            }
        }
        
        for (int n = 0; n < batch.commandCount; n++)
        {
            var command = batch.commands[n]; 
            switch (command.type) {
                case BatchCommandType.AddComponent:
                    break;
                case BatchCommandType.RemoveComponent:
                    break;
            }
        }
    }
}
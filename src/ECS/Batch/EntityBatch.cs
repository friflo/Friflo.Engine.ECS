// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class  EntityBatch
{
    private  readonly   BatchComponent[]    components;
    internal readonly   BatchCommand[]      commands;
    internal int                            commandCount;
    private  readonly   EntityStoreBase     store;
    internal readonly   EntityStore         entityStore;
    internal            int                 entityId;
    internal            Tags                addedTags;
    internal            Tags                removedTags;
    
    internal EntityBatch(EntityStoreBase store)
    {
        this.store          = store;
        entityStore         = (EntityStore)store;
        var schema          = EntityStoreBase.Static.EntitySchema;
        int maxStructIndex  = schema.maxStructIndex;
        components          = new BatchComponent[maxStructIndex];
        commands            = new BatchCommand[16];
        
        var componentTypes = schema.components;
        for (int n = 1; n < maxStructIndex; n++) {
            components[n] = componentTypes[n].CreateBatchComponent();
        }
    }
    
    internal void Apply()
    {
        store.Apply(this);
    }
    
    internal EntityBatch Add<T>(T component) where T : struct, IComponent
    {
        ref var command     = ref commands[commandCount++];
        var structIndex     = StructHeap<T>.StructIndex;
        command.typeIndex   = structIndex;
        command.type        = BatchCommandType.AddComponent;
        ((BatchComponent<T>)components[structIndex]).value = component;
        return this;   
    }
    
    internal EntityBatch Remove<T>() where T : struct, IComponent
    {
        ref var command     = ref commands[commandCount++];
        command.typeIndex   = StructHeap<T>.StructIndex;
        command.type        = BatchCommandType.RemoveComponent;
        return this;   
    }
    
    internal EntityBatch AddTag<T>() where T : struct, ITag
    {
        var tagIndex = TagType<T>.TagIndex;
        addedTags.  bitSet.SetBit   (tagIndex);
        removedTags.bitSet.ClearBit (tagIndex);
        return this;
    }
    
    internal EntityBatch AddTags(in Tags tags)
    {
        addedTags.  Add     (tags);
        removedTags.Remove  (tags);
        return this;
    }
    
    internal EntityBatch RemoveTag<T>() where T : struct, ITag
    {
        var tagIndex = TagType<T>.TagIndex;
        removedTags.bitSet.SetBit   (tagIndex);
        addedTags.  bitSet.ClearBit (tagIndex);
        return this;
    }
    
    internal EntityBatch RemoveTags(in Tags tags)
    {
        addedTags.  Remove  (tags);
        removedTags.Add     (tags);
        return this;
    }
}

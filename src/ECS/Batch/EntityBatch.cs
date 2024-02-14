// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
// ReSharper disable RedundantTypeDeclarationBody
namespace Friflo.Engine.ECS;

internal class BatchComponent { }

internal class BatchComponent<T> : BatchComponent where T : struct, IComponent
{
    internal        T       value;

    public override string  ToString() => typeof(T).Name;
}


internal sealed class  EntityBatch
{
    internal readonly   BatchComponent[]    components;         //  8
    private  readonly   EntityStoreBase     store;              //  8
    internal readonly   EntityStore         entityStore;        //  8
    internal            int                 entityId;           //  4
    internal            Tags                addedTags;          // 32
    internal            Tags                removedTags;        // 32
    internal            ComponentTypes      addedComponents;    // 32
    internal            ComponentTypes      removedComponents;  // 32
    
    internal EntityBatch(EntityStoreBase store)
    {
        this.store          = store;
        entityStore         = (EntityStore)store;
        var schema          = EntityStoreBase.Static.EntitySchema;
        int maxStructIndex  = schema.maxStructIndex;
        components          = new BatchComponent[maxStructIndex];
        
        var componentTypes = schema.components;
        for (int n = 1; n < maxStructIndex; n++) {
            components[n] = componentTypes[n].CreateBatchComponent();
        }
    }
    
    internal void Apply() {
        store.ApplyEntityBatch(this);
    }
    
    internal EntityBatch AddComponent<T>(T component) where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        addedComponents.    bitSet.SetBit   (structIndex);
        removedComponents.  bitSet.ClearBit (structIndex);
        ((BatchComponent<T>)components[structIndex]).value = component;
        return this;   
    }
    
    internal EntityBatch RemoveComponent<T>() where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        removedComponents.  bitSet.SetBit   (structIndex);
        addedComponents.    bitSet.ClearBit (structIndex);
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

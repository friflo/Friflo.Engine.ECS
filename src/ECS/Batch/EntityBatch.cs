// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal class BatchComponent { }

internal class BatchComponent<T> : BatchComponent where T : struct, IComponent
{
    internal    T   value;
}

internal enum BatchOwner
{
    Application = 0,
    EntityStore = 1,
}

internal sealed class  EntityBatch
{
#region public properties
    public              int     CommandCount    => GetCommandCount();
    public   override   string  ToString()      => GetString();
    #endregion

#region internal fields
    [Browse(Never)] internal            BatchComponent[]    batchComponents;    //  8
    [Browse(Never)] private  readonly   ComponentType[]     componentTypes;     //  8
    [Browse(Never)] private  readonly   EntityStoreBase     store;              //  8   - used only if owner == EntityStore
    [Browse(Never)] internal            int                 entityId;           //  4   - used only if owner == EntityStore
    [Browse(Never)] private  readonly   BatchOwner          owner;              //  4
    [Browse(Never)] internal            Tags                tagsAdd;            // 32
    [Browse(Never)] internal            Tags                tagsRemove;         // 32
    [Browse(Never)] internal            ComponentTypes      componentsAdd;      // 32
    [Browse(Never)] internal            ComponentTypes      componentsRemove;   // 32
    #endregion
    
#region internal methods
    public EntityBatch()
    {
        componentTypes  = EntityStoreBase.Static.EntitySchema.components;
        owner           = BatchOwner.Application;
    }
    
    internal EntityBatch(EntityStoreBase store)
    {
        componentTypes  = EntityStoreBase.Static.EntitySchema.components;
        owner           = BatchOwner.EntityStore;
        this.store      = store;
    }
    
    public void Clear()
    {
        tagsAdd             = default;
        tagsRemove          = default;
        componentsAdd       = default;
        componentsRemove    = default;
    }
    
    private int GetCommandCount()
    {
        return  tagsAdd          .Count +
                tagsRemove       .Count +
                componentsAdd    .Count +
                componentsRemove .Count;
    }
    
    private string GetString()
    {
        var hasAdds     = componentsAdd.Count    > 0 || tagsAdd.Count    > 0;
        var hasRemoves  = componentsRemove.Count > 0 || tagsRemove.Count > 0;
        if (!hasAdds && !hasRemoves) {
            return "empty";
        }
        var sb = new StringBuilder();
        if (hasAdds) {
            sb.Append("add: [");
            foreach (var component in componentsAdd) {
                sb.Append(component.Name);
                sb.Append(", ");
            }
            foreach (var tag in tagsAdd) {
                sb.Append('#');
                sb.Append(tag.Name);
                sb.Append(", ");
            }
            sb.Length -= 2;
            sb.Append("]  ");
        }
        if (hasRemoves) {
            sb.Append("remove: [");
            foreach (var component in componentsRemove) {
                sb.Append(component.Name);
                sb.Append(", ");
            }
            foreach (var tag in tagsRemove) {
                sb.Append('#');
                sb.Append(tag.Name);
                sb.Append(", ");
            }
            sb.Length -= 2;
            sb.Append(']');
        }
        return sb.ToString();
    }
    #endregion
    
#region commands
    public void Apply()
    {
        if (owner == BatchOwner.Application) throw new InvalidOperationException("Apply() can only be used on Entity.Batch. Use ApplyTo()");
        store.ApplyBatchTo(this, entityId);
    }
    
    public EntityBatch ApplyTo(Entity entity)
    {
        entity.store.ApplyBatchTo(this, entity.Id);
        return this;
    }
    
    public EntityBatch AddComponent<T>(T component) where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        componentsAdd.      bitSet.SetBit   (structIndex);
        componentsRemove.   bitSet.ClearBit (structIndex);
        var components      = batchComponents           ??= CreateBatchComponents();
        var batchComponent  = components[structIndex]   ??= CreateBatchComponent(structIndex);
        ((BatchComponent<T>)batchComponent).value = component;
        return this;   
    }
    
    private static BatchComponent[] CreateBatchComponents() {
        var maxStructIndex = EntityStoreBase.Static.EntitySchema.maxStructIndex;
        return new BatchComponent[maxStructIndex];
    }
    
    private BatchComponent CreateBatchComponent(int structIndex) {
        return componentTypes[structIndex].CreateBatchComponent();
    }
    
    public EntityBatch RemoveComponent<T>() where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        componentsRemove.   bitSet.SetBit   (structIndex);
        componentsAdd.      bitSet.ClearBit (structIndex);
        return this;   
    }
    
    public EntityBatch AddTag<T>() where T : struct, ITag
    {
        var tagIndex = TagType<T>.TagIndex;
        tagsAdd.    bitSet.SetBit   (tagIndex);
        tagsRemove. bitSet.ClearBit (tagIndex);
        return this;
    }
    
    public EntityBatch AddTags(in Tags tags)
    {
        tagsAdd.    Add     (tags);
        tagsRemove. Remove  (tags);
        return this;
    }
    
    public EntityBatch RemoveTag<T>() where T : struct, ITag
    {
        var tagIndex = TagType<T>.TagIndex;
        tagsRemove. bitSet.SetBit   (tagIndex);
        tagsAdd.    bitSet.ClearBit (tagIndex);
        return this;
    }
    
    public EntityBatch RemoveTags(in Tags tags)
    {
        tagsAdd.    Remove  (tags);
        tagsRemove. Add     (tags);
        return this;
    }
    #endregion
}

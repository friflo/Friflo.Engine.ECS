// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public sealed class CreateBatch
{
#region public properties
    public              int     ComponentCount  => componentsCreate.Count;
    public              int     TagCount        => tagsCreate.Count;
    public   override   string  ToString()      => GetString();
    #endregion

#region internal fields
    [Browse(Never)] private  readonly   BatchComponent[]    batchComponents;    //  8
    [Browse(Never)] private  readonly   ComponentType[]     componentTypes;     //  8
    [Browse(Never)] private  readonly   EntityStoreBase     store;              //  8
    [Browse(Never)] internal            Tags                tagsCreate;         // 32
    [Browse(Never)] internal            ComponentTypes      componentsCreate;   // 32
    #endregion
    
#region general methods
    internal CreateBatch(EntityStoreBase store)
    {
        var schema          = EntityStoreBase.Static.EntitySchema;
        componentTypes      = schema.components;
        batchComponents     = new BatchComponent[schema.maxStructIndex];
        this.store          = store;
    }
    
    private string GetString()
    {
        var hasAdds     = componentsCreate.Count    > 0 || tagsCreate.Count    > 0;
        if (!hasAdds) {
            return "empty";
        }
        var sb = new StringBuilder();
        sb.Append("entity: [");
        foreach (var component in componentsCreate) {
            sb.Append(component.Name);
            sb.Append(", ");
        }
        foreach (var tag in tagsCreate) {
            sb.Append('#');
            sb.Append(tag.Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
    #endregion
    
#region commands
    public Entity CreateEntity()
    {
        var archetype   = store.GetArchetype(componentsCreate, tagsCreate);
        var localStore  = (EntityStore)store;
        ref var node    = ref localStore.CreateEntityInternal(archetype);
        var compIndex   = node.compIndex;
        var components  = batchComponents;
        
        // --- assign component values
        foreach (var heap in archetype.structHeaps) {
            heap.SetBatchComponent(components, compIndex);
        }
        return new Entity(localStore, node.id);
    }

    /// <summary>
    /// Adds an add component command to the <see cref="EntityBatch"/> with the given <paramref name="component"/>.
    /// </summary>
    public CreateBatch Add<T>(in T component) where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        componentsCreate.bitSet.SetBit(structIndex);
        var batchComponent = batchComponents[structIndex] ??= CreateBatchComponent(structIndex);
        ((BatchComponent<T>)batchComponent).value = component;
        return this;   
    }
    
    public CreateBatch Add<T>() where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        componentsCreate.bitSet.SetBit(structIndex);
        var batchComponent = batchComponents[structIndex] ??= CreateBatchComponent(structIndex);
        ((BatchComponent<T>)batchComponent).value = default;
        return this;   
    }
    
    private BatchComponent CreateBatchComponent(int structIndex) {
        return componentTypes[structIndex].CreateBatchComponent();
    }
    
    /// <summary>
    /// Adds an add tag command to the <see cref="EntityBatch"/>.
    /// </summary>
    public CreateBatch AddTag<T>() where T : struct, ITag
    {
        tagsCreate.bitSet.SetBit(TagType<T>.TagIndex);
        return this;
    }
    
    /// <summary>
    /// Adds an add tags command to the <see cref="EntityBatch"/> adding the given <paramref name="tags"/>.
    /// </summary>
    public CreateBatch AddTags(in Tags tags)
    {
        tagsCreate.Add(tags);
        return this;
    }
    #endregion
}

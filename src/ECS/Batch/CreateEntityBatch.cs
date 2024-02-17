// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


/// <summary>
/// A create batch is used to optimize entity creation.<br/>
/// Components and tags are buffered before creating an entity with <see cref="CreateEntity"/>. 
/// </summary>
/// <remarks>
/// Multiple entities can be created using the same batch.<br/>
/// <br/>
/// Creating an entity via a batch stores the entity directly in the target <see cref="Archetype"/><br/>
/// This prevents any structural changes caused when creating an entity in steps using<br/>  
/// <see cref="EntityStore.CreateEntity()"/> an subsequent calls to <see cref="Entity.AddComponent{T}()"/>
/// and <see cref="Entity.AddTag{TTag}()"/>.
/// </remarks>
public sealed class CreateEntityBatch
{
#region public properties
    /// <summary> Return the of components added to the batch.</summary>
    public              int     ComponentCount  => componentsCreate.Count;
    
    /// <summary> Return the of tags added to the batch.</summary>
    public              int     TagCount        => tagsCreate.Count;
    
    public   override   string  ToString()      => GetString();
    #endregion

#region internal fields
    [Browse(Never)] private  readonly   BatchComponent[]    batchComponents;    //  8
    [Browse(Never)] private  readonly   ComponentType[]     componentTypes;     //  8
    [Browse(Never)] private  readonly   EntityStoreBase     store;              //  8
    [Browse(Never)] internal            Archetype           archetype;          //  8
    [Browse(Never)] internal            Tags                tagsCreate;         // 32
    [Browse(Never)] internal            ComponentTypes      componentsCreate;   // 32
    #endregion
    
#region general methods
    internal CreateEntityBatch(EntityStoreBase store)
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
    /// <summary>
    /// Creates an entity with the components and tags added previously using<br/>
    /// <see cref="Add{T}()"/>, <see cref="AddTag{T}"/> or <see cref="AddTags"/>.
    /// </summary>
    public Entity CreateEntity()
    {
        archetype       ??= store.GetArchetype(componentsCreate, tagsCreate);
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
    /// Add the given <paramref name="component"/> that will be added to the entity when calling <see cref="CreateEntity"/>. 
    /// </summary>
    public CreateEntityBatch Add<T>(in T component) where T : struct, IComponent
    {
        archetype       = null;
        var structIndex = StructHeap<T>.StructIndex;
        componentsCreate.bitSet.SetBit(structIndex);
        var batchComponent = batchComponents[structIndex] ??= CreateBatchComponent(structIndex);
        ((BatchComponent<T>)batchComponent).value = component;
        return this;   
    }
    
    /// <summary>
    /// Add a component that will be added to the entity when calling <see cref="CreateEntity"/>. 
    /// </summary>
    public CreateEntityBatch Add<T>() where T : struct, IComponent
    {
        archetype       = null;
        var structIndex = StructHeap<T>.StructIndex;
        componentsCreate.bitSet.SetBit(structIndex);
        var batchComponent = batchComponents[structIndex] ??= CreateBatchComponent(structIndex);
        ((BatchComponent<T>)batchComponent).value = default;
        return this;   
    }
    
    /// <summary>
    /// Get a component by reference previously added to the batch.<br/>
    /// This enables creation of multiple entities using the same batch. 
    /// </summary>
    public ref T Get<T>() where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        if (!componentsCreate.bitSet.Has(structIndex)) throw GetException(structIndex);
        return ref ((BatchComponent<T>)batchComponents[structIndex]).value;
    }
    
    private static InvalidOperationException GetException(int structIndex)
    {
        var componentName = EntityStoreBase.Static.EntitySchema.components[structIndex].Name;
        return new InvalidOperationException($"Get<>() requires a preceding Add<>(). Component: [{componentName}]");
    }
    
    private BatchComponent CreateBatchComponent(int structIndex) {
        return componentTypes[structIndex].CreateBatchComponent();
    }
    
    /// <summary>
    /// Add a tag that will be added to the entity when calling <see cref="CreateEntity"/>. 
    /// </summary>
    public CreateEntityBatch AddTag<T>() where T : struct, ITag
    {
        archetype = null;
        tagsCreate.bitSet.SetBit(TagType<T>.TagIndex);
        return this;
    }
    
    /// <summary>
    /// Adds the given <paramref name="tags"/> that will be added to the entity when calling <see cref="CreateEntity"/>. 
    /// </summary>
    public CreateEntityBatch AddTags(in Tags tags)
    {
        archetype = null;
        tagsCreate.Add(tags);
        return this;
    }
    #endregion
}

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
/// Is thrown if using a batch returned by <see cref="EntityStoreBase.Batch"/> with autoReturn: true<br/>
/// after calling <see cref="CreateEntityBatch.CreateEntity"/>.
/// </summary>
public class BatchAlreadyReturnedException : InvalidOperationException
{
    internal BatchAlreadyReturnedException(string message) : base (message) { }
}

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
    [Browse(Never)] internal            bool                autoReturn;         //  4
    [Browse(Never)] internal            bool                isReturned;         //  4
    [Browse(Never)] private             Archetype           archetype;          //  8
    [Browse(Never)] private             Tags                tagsCreate;         // 32
    [Browse(Never)] private             ComponentTypes      componentsCreate;   // 32
    #endregion
    
#region general methods
    /// <summary>
    /// Creates a batch used to create entities with components and tags added to the batch.<br/>
    /// The created batch instance can be cached.
    /// </summary>
    public CreateEntityBatch(EntityStoreBase store)
    {
        var schema          = EntityStoreBase.Static.EntitySchema;
        componentTypes      = schema.components;
        batchComponents     = new BatchComponent[schema.maxStructIndex];
        this.store          = store;
    }
    
    /// <summary> Clear all components and tags previously added to the batch. </summary>
    public CreateEntityBatch Clear() {
        componentsCreate  = default;
        tagsCreate        = default;
        archetype         = null;
        return this;
    }
    
    private string GetString()
    {
        if (isReturned) {
            return "batch returned";
        }
        var hasAdds = componentsCreate.Count > 0 || tagsCreate.Count > 0;
        if (!hasAdds) {
            return "empty";
        }
        var sb = new StringBuilder();
        sb.Append("add: [");
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
    /// Creates an entity with the components and tags previously added.<br/>
    /// Added batch components and tags are not cleared.
    /// </summary>
    /// <remarks>
    /// Subsequent use of a batch returned by <c>Batch(autoReturn: true)</c> throws <see cref="ECS.BatchAlreadyReturnedException"/>.
    /// </remarks>
    public Entity CreateEntity()
    {
        if (isReturned) throw BatchAlreadyReturnedException();
        archetype       ??= store.GetArchetype(componentsCreate, tagsCreate);
        var localStore  = (EntityStore)store;
        ref var node    = ref localStore.CreateEntityInternal(archetype);
        var compIndex   = node.compIndex;
        var components  = batchComponents;
        
        // --- assign component values
        foreach (var heap in archetype.structHeaps) {
            heap.SetBatchComponent(components, compIndex);
        }
        if (autoReturn) {
            isReturned = true;
            Clear();
            store.ReturnCreateBatch(this);
        }
        return new Entity(localStore, node.id);
    }
    
    /// <summary>
    /// Return the batch instance to its <see cref="EntityStore"/> to prevent memory allocations for future
    /// <see cref="EntityStoreBase.Batch"/> calls.
    /// </summary>
    public void Return() {
        if (isReturned) {
            return;
        }
        isReturned = true;
        Clear();
        store.ReturnCreateBatch(this);
    }
    
    private static BatchAlreadyReturnedException BatchAlreadyReturnedException() {
        return new BatchAlreadyReturnedException("batch already returned");
    }

    /// <summary>
    /// Add the given <paramref name="component"/> that will be added to the entity when calling <see cref="CreateEntity"/>. 
    /// </summary>
    public CreateEntityBatch Add<T>(in T component) where T : struct, IComponent
    {
        if (isReturned) throw BatchAlreadyReturnedException();
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
        if (isReturned) throw BatchAlreadyReturnedException();
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
        if (isReturned) throw BatchAlreadyReturnedException();
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
        if (isReturned) throw BatchAlreadyReturnedException();
        archetype = null;
        tagsCreate.bitSet.SetBit(TagType<T>.TagIndex);
        return this;
    }
    
    /// <summary>
    /// Adds the given <paramref name="tags"/> that will be added to the entity when calling <see cref="CreateEntity"/>. 
    /// </summary>
    public CreateEntityBatch AddTags(in Tags tags)
    {
        if (isReturned) throw BatchAlreadyReturnedException();
        archetype = null;
        tagsCreate.Add(tags);
        return this;
    }
    #endregion
}

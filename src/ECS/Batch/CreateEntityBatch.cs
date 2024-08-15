// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
/// after calling <see cref="CreateEntityBatch.CreateEntity()"/>.
/// </summary>
public class BatchAlreadyReturnedException : InvalidOperationException
{
    internal BatchAlreadyReturnedException(string message) : base (message) { }
}

/// <summary>
/// A create batch is used to optimize entity creation.<br/>
/// Components and tags are buffered before creating an entity with <see cref="CreateEntity()"/>.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#batch---create-entity">Example.</a>
/// </summary>
/// <remarks>
/// Multiple entities can be created using the same batch.<br/>
/// <br/>
/// Creating an entity via a batch stores the entity directly in the target <see cref="Archetype"/><br/>
/// This prevents any structural changes caused when creating an entity in steps using<br/>  
/// <see cref="EntityStore.CreateEntity()"/> a subsequent calls to <see cref="Entity.AddComponent{T}()"/>
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
    public Entity CreateEntity() {
        if (isReturned) throw BatchAlreadyReturnedException();
        var entityStore     = (EntityStore)store;
        var id              = entityStore.NewId(); 
        return CreateEntityInternal(entityStore, id);
    }
    
    /// <summary>
    /// Creates an entity with the specified <paramref name="id"/> and the components and tags previously added.<br/>
    /// Added batch components and tags are not cleared.
    /// </summary>
    /// <remarks>
    /// Subsequent use of a batch returned by <c>Batch(autoReturn: true)</c> throws <see cref="ECS.BatchAlreadyReturnedException"/>.
    /// </remarks>
    public Entity CreateEntity(int id)
    {
        if (isReturned) throw BatchAlreadyReturnedException();
        var entityStore  = (EntityStore)store;
        entityStore.CheckEntityId(id);
        return CreateEntityInternal(entityStore, id);
    }
    
    private Entity CreateEntityInternal(EntityStore entityStore, int id)
    {
        archetype     ??= entityStore.GetArchetype(componentsCreate, tagsCreate);
        var compIndex   = entityStore.CreateEntityInternal(archetype, id, out var revision);
        var components  = batchComponents;
        
        // --- assign component values
        foreach (var heap in archetype.structHeaps) {
            heap.SetBatchComponent(components, compIndex);
        }
        if (autoReturn) {
            isReturned = true;
            Clear();
            entityStore.ReturnCreateBatch(this);
        }
        var entity = new Entity(entityStore, id, revision);
        
        // Send event. See: SEND_EVENT notes
        entityStore.CreateEntityEvent(entity);
        return entity;
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
    /// Add the given <paramref name="component"/> that will be added to the entity when calling <see cref="CreateEntity()"/>. 
    /// </summary>
    public CreateEntityBatch Add<T>(in T component) where T : struct, IComponent
    {
        if (isReturned) throw BatchAlreadyReturnedException();
        archetype       = null;
        var structIndex = StructInfo<T>.Index;
        componentsCreate.bitSet.SetBit(structIndex);
        var batchComponent = batchComponents[structIndex] ??= CreateBatchComponent(structIndex);
        ((BatchComponent<T>)batchComponent).value = component;
        return this;   
    }
    
    /// <summary>
    /// Add a component that will be added to the entity when calling <see cref="CreateEntity()"/>. 
    /// </summary>
    public CreateEntityBatch Add<T>() where T : struct, IComponent
    {
        if (isReturned) throw BatchAlreadyReturnedException();
        archetype       = null;
        var structIndex = StructInfo<T>.Index;
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
        var structIndex = StructInfo<T>.Index;
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
    /// Add a tag that will be added to the entity when calling <see cref="CreateEntity()"/>. 
    /// </summary>
    public CreateEntityBatch AddTag<T>() where T : struct, ITag
    {
        if (isReturned) throw BatchAlreadyReturnedException();
        archetype = null;
        tagsCreate.bitSet.SetBit(TagInfo<T>.Index);
        return this;
    }
    
    /// <summary>
    /// Adds the given <paramref name="tags"/> that will be added to the entity when calling <see cref="CreateEntity()"/>. 
    /// </summary>
    public CreateEntityBatch AddTags(in Tags tags)
    {
        if (isReturned) throw BatchAlreadyReturnedException();
        archetype = null;
        tagsCreate.Add(tags);
        return this;
    }
    
    public CreateEntityBatch Disable() => AddTags(EntityUtils.Disabled);
    #endregion
}

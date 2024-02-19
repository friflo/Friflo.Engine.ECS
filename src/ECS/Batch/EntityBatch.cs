// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public class BatchAlreadyAppliedException : InvalidOperationException
{
    internal BatchAlreadyAppliedException(string message) : base (message) { }
}

internal class BatchComponent { }

internal class BatchComponent<T> : BatchComponent where T : struct, IComponent
{
    internal    T   value;
}

internal enum BatchOwner
{
    Application = 0,
    EntityStore = 1,
    EntityBatch = 2,
}

/// <summary>
/// An entity batch is a container of component and tag commands that can be <see cref="Apply"/>'ed to an entity.<br/>
/// It can be used on a single entity via <see cref="Entity.Batch"/> or as a <b>bulk operation</b> an a set of entities.
/// </summary>
/// <remarks>
/// Its purpose is to optimize add / remove component and tag changes on entities.<br/>
/// The same entity changes can be performed with the <see cref="Entity"/> methods using:<br/>
/// <see cref="Entity.AddComponent{T}()"/>, <see cref="Entity.RemoveComponent{T}()"/>,
/// <see cref="Entity.AddTag{TTag}()"/> or <see cref="Entity.RemoveTag{TTag}()"/>.<br/>
/// Each of this methods may cause a structural change which is a relative costly operation in comparison to others.<br/>
/// Using <see cref="EntityBatch"/> minimize theses structural changes to one or none.<br/>
/// <br/>
/// <b>Bulk operation</b><br/>
/// To perform the same batch on multiple entities you can use <see cref="QueryEntities.ApplyBatch"/> for <br/>
/// - all entities of an <see cref="EntityStore"/> using <see cref="EntityStore.Entities"/>.<br/>
/// - the entities of an <see cref="ArchetypeQuery"/> using <see cref="ArchetypeQuery.Entities"/>.<br/>
/// - or the entities of an <see cref="Archetype"/> using <see cref="Archetype.Entities"/>.
/// </remarks>
public sealed class  EntityBatch
{
#region public properties
    /// <summary>
    /// Return the number of commands stored in the <see cref="EntityBatch"/>.
    /// </summary>
    public              int     CommandCount    => GetCommandCount();
    public   override   string  ToString()      => GetString();
    #endregion

#region internal fields
    [Browse(Never)] internal            BatchComponent[]    batchComponents;    //  8
    [Browse(Never)] private  readonly   ComponentType[]     componentTypes;     //  8
    [Browse(Never)] private  readonly   EntityStoreBase     store;              //  8   - used only if owner == EntityStore
    [Browse(Never)] internal            int                 entityId;           //  4   - used only if owner == EntityStore
    [Browse(Never)] internal            BatchOwner          owner;              //  4
    [Browse(Never)] internal            Tags                tagsAdd;            // 32
    [Browse(Never)] internal            Tags                tagsRemove;         // 32
    [Browse(Never)] internal            ComponentTypes      componentsAdd;      // 32
    [Browse(Never)] internal            ComponentTypes      componentsRemove;   // 32
    #endregion
    
#region general methods
    /// <summary>
    /// Creates a batch that can be applied to a <b>single</b> entity or a set of entities using a <b>bulk operation</b>.<br/>
    /// See <see cref="EntityBatch"/>.
    /// </summary>
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
    
    /*
    internal BatchInUseException BatchInUseException() {
        var entity = new Entity((EntityStore)store, entityId);
        return new BatchInUseException($"Entity.Batch in use - {this}");
    } */
    
    /// <summary>
    /// Clear all commands currently stored in the <see cref="EntityBatch"/>.
    /// </summary>
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
        if (entityId != 0) {
            sb.Append("entity: ");
            sb.Append(entityId);
            sb.Append(" > ");
        }
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
            sb.Append("]");
        }
        if (hasRemoves) {
            if (hasAdds) {
                sb.Append("  ");
            }
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
    /// <summary>
    /// Apply added batch commands to the entity the preceding <see cref="Entity.Batch"/> operates.<br/>
    /// <br/>
    /// Subsequent use of the batch throws <see cref="ECS.BatchAlreadyAppliedException"/>.
    /// </summary>
    public void Apply()
    {
        if (owner == BatchOwner.Application) throw ApplyException();
        if (owner == BatchOwner.EntityStore) throw BatchAlreadyAppliedException();
        store.ApplyBatchTo(this, entityId);
        store.ReturnBatch(this);
        owner = BatchOwner.EntityStore;
        Clear();
    }
    
    private static InvalidOperationException ApplyException() {
        return new InvalidOperationException("Apply() can only be used on a batch using Entity.Batch() - use ApplyTo()");
    }
    
    private static BatchAlreadyAppliedException BatchAlreadyAppliedException() {
        return new BatchAlreadyAppliedException("batch already applied");
    }
    
    /// <summary>
    /// Apply the batch commands to the given <paramref name="entity"/>.<br/>
    /// The stored batch commands are not cleared.
    /// </summary>
    public EntityBatch ApplyTo(Entity entity)
    {
        entity.store.ApplyBatchTo(this, entity.Id);
        return this;
    }
    
    /// <summary>
    /// Adds an add component command to the <see cref="EntityBatch"/> with the given <paramref name="component"/>.
    /// </summary>
    public EntityBatch Add<T>(in T component) where T : struct, IComponent
    {
        if (owner == BatchOwner.EntityStore) throw BatchAlreadyAppliedException();
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
    
    /// <summary> 
    /// Adds a remove component command to the <see cref="EntityBatch"/>.
    /// </summary>
    public EntityBatch Remove<T>() where T : struct, IComponent
    {
        if (owner == BatchOwner.EntityStore) throw BatchAlreadyAppliedException();
        var structIndex = StructHeap<T>.StructIndex;
        componentsRemove.   bitSet.SetBit   (structIndex);
        componentsAdd.      bitSet.ClearBit (structIndex);
        return this;   
    }
    
    /// <summary>
    /// Adds an add tag command to the <see cref="EntityBatch"/>.
    /// </summary>
    public EntityBatch AddTag<T>() where T : struct, ITag
    {
        if (owner == BatchOwner.EntityStore) throw BatchAlreadyAppliedException();
        var tagIndex = TagType<T>.TagIndex;
        tagsAdd.    bitSet.SetBit   (tagIndex);
        tagsRemove. bitSet.ClearBit (tagIndex);
        return this;
    }
    
    /// <summary>
    /// Adds an add tags command to the <see cref="EntityBatch"/> adding the given <paramref name="tags"/>.
    /// </summary>
    public EntityBatch AddTags(in Tags tags)
    {
        if (owner == BatchOwner.EntityStore) throw BatchAlreadyAppliedException();
        tagsAdd.    Add     (tags);
        tagsRemove. Remove  (tags);
        return this;
    }
    
    /// <summary>
    /// Adds a remove tag command to the <see cref="EntityBatch"/>.
    /// </summary>
    public EntityBatch RemoveTag<T>() where T : struct, ITag
    {
        if (owner == BatchOwner.EntityStore) throw BatchAlreadyAppliedException();
        var tagIndex = TagType<T>.TagIndex;
        tagsRemove. bitSet.SetBit   (tagIndex);
        tagsAdd.    bitSet.ClearBit (tagIndex);
        return this;
    }
    
    /// <summary>
    /// Adds a remove tags command to the <see cref="EntityBatch"/> removing the given <paramref name="tags"/>.
    /// </summary>
    public EntityBatch RemoveTags(in Tags tags)
    {
        if (owner == BatchOwner.EntityStore) throw BatchAlreadyAppliedException();
        tagsAdd.    Remove  (tags);
        tagsRemove. Add     (tags);
        return this;
    }
    
    [Obsolete($"Renamed to {nameof(Remove)}()")]
    public EntityBatch RemoveComponent<T>() where T : struct, IComponent => Remove<T>();
    
    [Obsolete($"Renamed to {nameof(Add)}()")]
    public EntityBatch AddComponent<T>(T component) where T : struct, IComponent => Add(component);
    
    #endregion
}

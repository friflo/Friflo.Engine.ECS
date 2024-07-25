// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is thrown if using a batch returned by <see cref="Entity.Batch"/> after calling <see cref="EntityBatch.Apply"/>.
/// </summary>
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
/// Each of these methods may cause a structural change which is a relative costly operation in comparison to others.<br/>
/// Using a batch minimize theses structural changes to one or none.<br/>
/// <br/>
/// <b>Bulk operation</b><br/>
/// To perform a batch on multiple entities you can use <see cref="QueryEntities.ApplyBatch"/> for <br/>
/// - the entities of an <see cref="ArchetypeQuery"/> using <see cref="ArchetypeQuery.Entities"/>.
///   See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#entitybatch---query">Example.</a><br/>
/// - all entities of an <see cref="EntityStore"/> using <see cref="EntityStore.Entities"/>.<br/>
/// - or the entities of an <see cref="Archetype"/> using <see cref="Archetype.Entities"/>.<br/>
/// To perform a batch on entities in an <see cref="EntityList"/> you can use <see cref="EntityList.ApplyBatch"/>.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#entitybatch---entitylist">Example.</a><br/>
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
    /// The created batch instance can be cached.
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
        if (owner == BatchOwner.EntityStore) {
            return "batch applied";
        }
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
        return new InvalidOperationException("Apply() can only be used on a batch returned by Entity.Batch() - use ApplyTo()");
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
        var structIndex = StructInfo<T>.Index;
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
        var structIndex = StructInfo<T>.Index;
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
        var tagIndex = TagInfo<T>.Index;
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
        var tagIndex = TagInfo<T>.Index;
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
    
    /// <summary>
    /// Enables an entity.<br/>
    /// Enabled entities are included in query results.
    /// </summary>
    public EntityBatch Enable()     => RemoveTags(EntityUtils.Disabled);

    /// <summary>
    /// Disables an entity.<br/>
    /// <see cref="Disabled"/> entities are not included query results.
    /// To include them use <see cref="ArchetypeQuery.WithDisabled"/>.
    /// </summary>
    public EntityBatch Disable()    => AddTags(EntityUtils.Disabled);
    

    /// <summary> Obsolete - use renamed method: <see cref="Remove{T}"/> </summary>
    [Obsolete($"Renamed to {nameof(Remove)}()")]
    public EntityBatch RemoveComponent<T>() where T : struct, IComponent => Remove<T>();
    
    /// <summary> Obsolete - use renamed method: <see cref="Add{T}"/> </summary>
    [Obsolete($"Renamed to {nameof(Add)}()")]
    public EntityBatch AddComponent<T>(T component) where T : struct, IComponent => Add(component);
    
    #endregion
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

/// <summary>
/// The type of a <see cref="ComponentChanged"/> event: <see cref="Remove"/>, <see cref="Add"/> or <see cref="Update"/> component.
/// </summary>
public enum ComponentChangedAction
{
    /// <summary> An <see cref="IComponent"/> was removed from an <see cref="Entity"/>. </summary>
    Remove  = 0,
    /// <summary> An <see cref="IComponent"/> was added to an <see cref="Entity"/>. </summary>
    Add     = 1,
    /// <summary> An <see cref="IComponent"/> of an <see cref="Entity"/> was updated. </summary>
    Update  = 2,
}

/// <summary>
/// Is the event for event handlers added to <see cref="Entity.OnComponentChanged"/>,
/// <see cref="EntityStoreBase.OnComponentAdded"/> or <see cref="EntityStoreBase.OnComponentRemoved"/>.<br/>
/// <br/>
/// These events are fired on:
/// <list type="bullet">
///     <item><see cref="Entity.AddComponent{T}()"/></item>
///     <item><see cref="Entity.RemoveComponent{T}()"/></item>
/// </list>
/// </summary>
public readonly struct  ComponentChanged
{
    /// <summary>The <see cref="EntityStore"/> containing the <see cref="Entity"/> that emitted the event.</summary>
    public  readonly    EntityStore             Store;          //  8
    
    /// <summary>The <c>Id</c> of the <see cref="Entity"/> that emitted the event.</summary>
    public  readonly    int                     EntityId;       //  4
    
    /// <summary>The executed entity change: <see cref="ComponentChangedAction.Remove"/>,
    /// <see cref="ComponentChangedAction.Add"/> or <see cref="ComponentChangedAction.Remove"/> component.</summary>
    public  readonly    ComponentChangedAction  Action;         //  4
    
    /// <summary>The <see cref="ECS.ComponentType"/> of the added / removed component.</summary>
    public  readonly    ComponentType           ComponentType; //  8
    
    private readonly    StructHeap              heap;          //  8
    
    // --- properties
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher.</summary>
    public              Entity                  Entity      => new Entity(Store, EntityId);
    
    public override     string                  ToString()  => $"entity: {EntityId} - event > {Action} {ComponentType}";

    internal ComponentChanged(EntityStoreBase store, int entityId, ComponentChangedAction action, int structIndex, StructHeap heap)
    {
        Store           = store as EntityStore; 
        EntityId        = entityId;
        Action          = action;
        ComponentType   = EntityStoreBase.Static.EntitySchema.components[structIndex];
        this.heap       = heap;
    }
    
    /// <summary>
    /// Returns the old component value before executing the <see cref="ComponentChangedAction.Update"/>
    /// or <see cref="ComponentChangedAction.Remove"/> component.
    /// </summary>.
    /// <typeparam name="TComponent"> The component type of the changed component.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// In case the <see cref="Action"/> was an <see cref="ComponentChangedAction.Add"/> component.
    /// </exception>
    public TComponent OldComponent<TComponent>() where TComponent : struct, IComponent
    {
        switch (Action)
        {
            case ComponentChangedAction.Update:
            case ComponentChangedAction.Remove: 
                if (typeof(TComponent) == ComponentType.Type) {
                    return ((StructHeap<TComponent>)heap).componentStash;
                }
                throw new InvalidOperationException($"OldComponent<T>() - expect component Type: {ComponentType.Type.Name}. T: {typeof(TComponent).Name}");
        }
        throw new InvalidOperationException($"OldComponent<T>() - component is newly added. T: {typeof(TComponent).Name}");
    }
}
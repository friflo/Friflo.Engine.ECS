// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
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
    [Browse(Never)]
    public  readonly    int                     EntityId;       //  4
    
    /// <summary>The executed entity change: <see cref="ComponentChangedAction.Remove"/>,
    /// <see cref="ComponentChangedAction.Add"/> or <see cref="ComponentChangedAction.Remove"/> component.</summary>
    public  readonly    ComponentChangedAction  Action;         //  4
    
    /// <summary>The <see cref="ECS.ComponentType"/> of the added / removed component.</summary>
    public  readonly    ComponentType           ComponentType;  //  8
    
    [Browse(Never)]
    private readonly    StructHeap              oldHeap;        //  8
    
    // --- properties
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher.</summary>
    public              Entity                  Entity              => new Entity(Store, EntityId);
    
    public              IComponent              DebugComponent      => GetDebugComponent();
    
    public              IComponent              DebugOldComponent   => GetDebugOldComponent();
    
    public override     string                  ToString()          => $"entity: {EntityId} - event > {Action} {ComponentType}";

    internal ComponentChanged(EntityStoreBase store, int entityId, ComponentChangedAction action, int structIndex, StructHeap oldHeap)
    {
        Store           = store as EntityStore; 
        EntityId        = entityId;
        Action          = action;
        ComponentType   = EntityStoreBase.Static.EntitySchema.components[structIndex];
        this.oldHeap       = oldHeap;
    }
    
    /// <summary>
    /// Returns the old component value before executing the <see cref="ComponentChangedAction.Update"/>
    /// or <see cref="ComponentChangedAction.Remove"/> component.<br/>
    /// <br/>
    /// <b>Note</b>: The <see cref="OldComponent{T}"/> value is only valid within the event handler call.<br/>
    /// A copy of <see cref="ComponentChanged"/> return an invalid value when using it outside the event handler call.<br/>
    /// Instead store the value returned by <see cref="OldComponent{T}"/> whin the handler when using is after the event handler returns.
    /// </summary>.
    /// <typeparam name="T"> The component type of the changed component.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// In case the <see cref="Action"/> was an <see cref="ComponentChangedAction.Add"/> component.
    /// </exception>
    public T OldComponent<T>() where T : struct, IComponent
    {
        switch (Action)
        {
            case ComponentChangedAction.Remove: 
            case ComponentChangedAction.Update:
                if (typeof(T) == ComponentType.Type) {
                    return ((StructHeap<T>)oldHeap).componentStash;
                }
                throw new InvalidOperationException($"OldComponent<T>() - expect component Type: {ComponentType.Type.Name}. T: {typeof(T).Name}");
        }
        throw new InvalidOperationException($"OldComponent<T>() - component is newly added. T: {typeof(T).Name}");
    }
    
    
    private IComponent GetDebugComponent() {
        switch (Action)
        {
            case ComponentChangedAction.Add: 
            case ComponentChangedAction.Update:
                return Entity.archetype.heapMap[ComponentType.StructIndex].GetComponentDebug(Entity.compIndex);
        }
        return null;
    }
    
    private IComponent GetDebugOldComponent() {
        switch (Action)
        {
            case ComponentChangedAction.Remove: 
            case ComponentChangedAction.Update:
                return oldHeap.GetComponentStashDebug();
        }
        return null;
    }
}
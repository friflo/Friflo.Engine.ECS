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
    /// <summary> An <see cref="IComponent"/> of an <see cref="Entity"/> was updated with <see cref="Entity.AddComponent{T}()"/>. </summary>
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
    
    // --- public properties
    /// <summary> Return the current <see cref="IComponent"/> for debugging.<br/>
    /// It has poor performance as is boxes the returned component. </summary>
    /// <remarks> To access the current component use <see cref="Component{T}"/> </remarks>
    [Obsolete($"use {nameof(Component)}<T>() to access the current component")]
    public              IComponent              DebugComponent      => GetDebugComponent();
    
    /// <summary> Return the old <see cref="IComponent"/> for debugging.<br/>
    /// It has poor performance as is boxes the returned component. </summary>
    /// <remarks> To access the old component use <see cref="OldComponent{T}"/> </remarks>
    [Obsolete($"use {nameof(OldComponent)}<T>() to access the old component")]
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
    /// Returns the current component value after executing the <see cref="ComponentChangedAction.Add"/>
    /// or <see cref="ComponentChangedAction.Update"/> component.<br/>
    /// </summary>.
    /// <typeparam name="T"> The component type of the changed component.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// In case the <see cref="Action"/> was <see cref="ComponentChangedAction.Remove"/> component.
    /// </exception>
    public T Component<T>() where T : struct, IComponent
    {
        switch (Action)
        {
            case ComponentChangedAction.Add: 
            case ComponentChangedAction.Update:
                if (typeof(T) == ComponentType.Type) {
                    var entity = Entity;
                    return ((StructHeap<T>)entity.archetype.heapMap[ComponentType.StructIndex]).components[entity.compIndex];
                }
                throw TypeException("Component<T>() - expect component Type: ", typeof(T));
        }
        throw ActionException("Component<T>() - component was removed. T: ", typeof(T));
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
    /// In case the <see cref="Action"/> was <see cref="ComponentChangedAction.Add"/> component.
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
                throw TypeException("OldComponent<T>() - expect component Type: ", typeof(T));
        }
        throw ActionException("OldComponent<T>() - component is newly added. T: ", typeof(T));
    }
    
    
    private IComponent GetDebugComponent() {
        switch (Action)
        {
            case ComponentChangedAction.Add: 
            case ComponentChangedAction.Update:
                var entity = Entity;
                return entity.archetype.heapMap[ComponentType.StructIndex].GetComponentDebug(entity.compIndex);
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
    
    private static InvalidOperationException ActionException(string message, Type type) {
        return new InvalidOperationException (message + type.Name);
    }
    
    private InvalidOperationException TypeException(string message, Type type) {
        return new InvalidOperationException($"{message}{ComponentType.Type.Name}. T: {type.Name}");
    }
}
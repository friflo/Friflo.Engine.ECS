// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

/// <summary>
/// The modification type of a <see cref="ComponentChanged"/> event: <see cref="Remove"/>, <see cref="Add"/> or <see cref="Update"/> component.
/// </summary>
public enum ComponentChangedAction
{
    /// <summary> An <see cref="IComponent"/> is removed from an <see cref="Entity"/>. </summary>
    Remove  = 0,
    /// <summary> An <see cref="IComponent"/> is added to an <see cref="Entity"/>. </summary>
    Add     = 1,
    /// <summary> An <see cref="IComponent"/> of an <see cref="Entity"/> is updated when calling
    /// <see cref="Entity.AddComponent{T}()"/> on an entity already having a component of a specific type. </summary>
    Update  = 2,
}

/// <summary>
/// Is the event for event handlers added to <see cref="Entity.OnComponentChanged"/>,
/// <see cref="EntityStoreBase.OnComponentAdded"/> or <see cref="EntityStoreBase.OnComponentRemoved"/>.
/// </summary>
/// <remarks>
/// These events are fired on:
/// <list type="bullet">
///     <item><see cref="Entity.AddComponent{T}()"/></item>
///     <item><see cref="Entity.RemoveComponent{T}()"/></item>
/// </list>
/// </remarks>
public readonly struct  ComponentChanged
{
#region fields
    /// <summary>The <see cref="EntityStore"/> containing the <see cref="Entity"/> that emitted the event.</summary>
    public  readonly    EntityStore             Store;          //  8
    
    /// <summary>The <c>Id</c> of the <see cref="Entity"/> that emitted the event.</summary>
    [Browse(Never)]
    public  readonly    int                     EntityId;       //  4
    
    /// <summary>The executed entity change: <see cref="ComponentChangedAction.Remove"/>,
    /// <see cref="ComponentChangedAction.Add"/> or <see cref="ComponentChangedAction.Remove"/> component.</summary>
    public  readonly    ComponentChangedAction  Action;         //  4
    
    [Browse(Never)]
    public  readonly    int                     StructIndex;    //  4
    
    [Browse(Never)]
    private readonly    IComponentStash         stash;          //  8

    // use nested class to minimize noise in debugger
    private static class Static {
        internal static readonly ComponentType[] ComponentTypes = EntityStoreBase.Static.EntitySchema.components;
    }
    #endregion
    
#region properties
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher.</summary>
    public              Entity                  Entity              => new Entity(Store, EntityId);
    
    /// <summary>The <see cref="ECS.ComponentType"/> of the added / removed component.</summary>
    [Browse(Never)]
    public              ComponentType           ComponentType       => Static.ComponentTypes[StructIndex];
    
    /// <summary>The <see cref="System.Type"/> of the added / removed component.</summary>
    /// <remarks>
    /// Use the following code snippet to switch on <see cref="Type"/>:
    /// <br/>
    /// <code>
    ///     var type = args.Type;
    ///     switch (true) {
    ///         case true when type == typeof(EntityName):
    ///             break;
    ///         case true when type == typeof(Position):
    ///             break;
    ///     }
    /// </code> 
    /// </remarks>
    public              Type                    Type                => ComponentType.Type;
    
    // --- public properties
    /// <summary> Return the current <see cref="IComponent"/> for debugging.<br/>
    /// <b>Note</b>: It degrades performance as it boxes the returned component. </summary>
    /// <remarks> To access the current component use <see cref="Component{T}"/> </remarks>
    [Obsolete($"use {nameof(Component)}<T>() to access the current component")]
    public              IComponent              DebugComponent      => GetDebugComponent();
    
    /// <summary> Return the old <see cref="IComponent"/> for debugging.<br/>
    /// <b>Note</b>: It degrades performance as it boxes the returned component. </summary>
    /// <remarks> To access the old component use <see cref="OldComponent{T}"/> </remarks>
    [Obsolete($"use {nameof(OldComponent)}<T>() to access the old component")]
    public              IComponent              DebugOldComponent   => GetDebugOldComponent();
    
    public override     string                  ToString()          => $"entity: {EntityId} - event > {Action} {ComponentType}";
    #endregion

#region methods
    internal ComponentChanged(EntityStoreBase store, int entityId, ComponentChangedAction action, int structIndex, IComponentStash stash)
    {
        Store           = store as EntityStore; 
        EntityId        = entityId;
        Action          = action;
        StructIndex     = structIndex;     
        this.stash      = stash;
    }
    
    /// <summary>
    /// Returns the current component value after executing the <see cref="ComponentChangedAction.Add"/>
    /// or <see cref="ComponentChangedAction.Update"/> component.<br/>
    /// </summary>.
    /// <typeparam name="T"> The component type of the changed component.</typeparam>
    /// <exception cref="InvalidOperationException"> In case the <see cref="Action"/> was <see cref="ComponentChangedAction.Remove"/> component. </exception>
    /// <exception cref="ArgumentException"> In case the component is accessed with the wrong generic type. </exception>
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
    /// Returns the old component value before executing <see cref="ComponentChangedAction.Update"/>
    /// or <see cref="ComponentChangedAction.Remove"/> component.<br/> <b>Note</b>: See Remarks for restrictions.
    /// </summary>.
    /// <remarks>
    /// <b>Note</b>:
    /// The <see cref="OldComponent{T}"/> return value is only valid within the event handler call.<br/>
    /// <see cref="ComponentChanged"/> may return an invalid value when calling it outside the event handler scope.<br/>
    /// Instead store the value returned by <see cref="OldComponent{T}"/> within the handler when using it after the event handler returns.<br/>
    /// Reason: For performance there is only one field per component type storing the old component value.<br/> 
    /// </remarks>
    /// <typeparam name="T"> The component type of the changed component.</typeparam>
    /// <exception cref="InvalidOperationException"> In case the <see cref="Action"/> was <see cref="ComponentChangedAction.Add"/> component. </exception>
    /// <exception cref="ArgumentException"> In case the component is accessed with the wrong generic type. </exception>
    public T OldComponent<T>() where T : struct, IComponent
    {
        switch (Action)
        {
            case ComponentChangedAction.Remove: 
            case ComponentChangedAction.Update:
                if (typeof(T) == ComponentType.Type) {
                    return ((IComponentStash<T>)stash).GetStashRef();
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
                return stash.GetStashDebug();
        }
        return null;
    }
    
    private static InvalidOperationException ActionException(string message, Type type) {
        return new InvalidOperationException (message + type.Name);
    }
    
    private ArgumentException TypeException(string message, Type type) {
        return new ArgumentException($"{message}{ComponentType.Name}. T: {type.Name}");
    }
    #endregion
}
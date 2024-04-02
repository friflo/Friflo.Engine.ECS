// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

/// <summary>
/// Is the event for event handlers added to <see cref="Entity.OnChildEntitiesChanged"/> or <see cref="EntityStore.OnChildEntitiesChanged"/>.
/// </summary>
/// <remarks>
/// These events are fired on:
/// <list type="bullet">
///     <item><see cref="Entity.AddChild"/></item>
///     <item><see cref="Entity.InsertChild"/></item>
///     <item><see cref="Entity.RemoveChild"/></item>
/// </list>
/// </remarks>
public readonly struct ChildEntitiesChanged
{
    
    public readonly ChildEntitiesChangedAction  Action;     //  4
    
    /// <summary>The <see cref="EntityStore"/> containing the <see cref="Entity"/> that emitted the event.</summary>
    public readonly EntityStore                 Store;      //  8
    
    /// <summary>The <c>Id</c> of the <see cref="Entity"/> that emitted the event.</summary>
    [Browse(Never)]
    public readonly int                         EntityId;   //  4
    
    /// <summary>The <c>Id</c> of the added / removed child entity</summary>
    [Browse(Never)]
    public readonly int                         ChildId;    //  4
    
    /// <summary>The child position of the added / removed child entity in the parent <see cref="Entity"/>.</summary>
    public readonly int                         ChildIndex; //  4
    
    // --- properties
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher</summary>
    public          Entity                      Entity      => new Entity(Store, EntityId);
    
    /// <summary>The added / removed child entity</summary>
    public          Entity                      Child       => new Entity(Store, ChildId);

    public override string                      ToString()  => $"entity: {EntityId} - event > {Action} Child[{ChildIndex}] = {ChildId}";

    internal ChildEntitiesChanged(
        ChildEntitiesChangedAction  action,
        EntityStore                 store,
        int                         entityId,
        int                         childId,
        int                         childIndex)
    {
        this.Action     = action;
        this.Store      = store;
        this.EntityId   = entityId;
        this.ChildId    = childId;
        this.ChildIndex = childIndex;
    }
}
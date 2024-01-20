// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

public enum ChangedEventAction
{
    Add     = 0,
    Remove  = 1,
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
    public readonly     EntityStore         Store;          //  8
    /// <summary>The <c>Id</c> of the <see cref="Entity"/> that emitted the event.</summary>
    public readonly     int                 EntityId;       //  4
    /// <summary>The executed entity change: Add / Remove component.</summary>
    public readonly     ChangedEventAction  Action;         //  4
    /// <summary>The <see cref="ECS.ComponentType"/> of the added / removed component.</summary>
    public readonly     ComponentType       ComponentType;  //  8
    
    // --- properties
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher.</summary>
    public              Entity              Entity      => new Entity(Store, EntityId);
    
    public override     string              ToString()  => $"entity: {EntityId} - event > {Action} {ComponentType}";

    internal ComponentChanged(EntityStoreBase store, int entityId, ChangedEventAction action, int structIndex)
    {
        this.Store          = store as EntityStore; 
        this.EntityId       = entityId;
        this.Action         = action;
        this.ComponentType  = EntityStoreBase.Static.EntitySchema.components[structIndex];
    }
}
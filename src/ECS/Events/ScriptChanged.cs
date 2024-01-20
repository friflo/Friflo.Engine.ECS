// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

/// <summary>
/// Is the event for event handlers added to <see cref="ECS.Entity.OnScriptChanged"/>,
/// <see cref="EntityStore.OnScriptAdded"/> or <see cref="EntityStore.OnScriptRemoved"/>.<br/>
/// <br/>
/// These events are fired on:
/// <list type="bullet">
///     <item><see cref="Entity.AddScript{TScript}"/></item>
///     <item><see cref="Entity.RemoveScript{T}"/></item>
/// </list>
/// </summary>
public readonly struct  ScriptChanged
{
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher.</summary>
    public readonly     Entity              Entity;     // 16
    /// <summary>The executed entity change: Add / Remove script.</summary>
    public readonly     ChangedEventAction  Action;     //  4
    /// <summary>The <see cref="ECS.ScriptType"/> of the added / removed script.</summary>
    public readonly     ScriptType          ScriptType; //  8
    
    // --- properties
    /// <summary>The <see cref="EntityStore"/> containing the <see cref="Entity"/> that emitted the event.</summary>
    public readonly     EntityStore         Store => Entity.store;
    
    public override     string              ToString() => $"entity: {Entity.Id} - event > {Action} {ScriptType}";

    internal ScriptChanged(Entity entity, ChangedEventAction action, ScriptType scriptType)
    {
        this.Entity         = entity;
        this.Action         = action;
        this.ScriptType     = scriptType;
    }
}
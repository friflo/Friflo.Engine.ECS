// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="Signal{TEvent}"/>'s are used to emit custom events from an entity to custom <see cref="Signal{TEvent}"/> handlers.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#signal">Example.</a>
/// </summary>
/// <remarks>
/// <see cref="Signal{TEvent}"/> handlers are added with <see cref="ECS.Entity.AddSignalHandler{TEvent}"/>.<br/>
/// They are used to implement the <a href="https://en.wikipedia.org/wiki/Observer_pattern">Observer pattern</a>
/// on entity level in the engine.<br/>
/// <br/>
/// It enables decoupling the code used for emitting events from a specific entity (aka subject / publisher)<br/>
/// to multiple subscribers (aka observers) consuming the event by their <see cref="Signal{TEvent}"/> handlers. 
/// </remarks>
/// <typeparam name="TEvent">The event type containing the fields of a custom event.</typeparam>
public readonly struct Signal<TEvent> where TEvent : struct 
{
    /// <summary>The <see cref="EntityStore"/> containing the <see cref="Entity"/> that emitted the <see cref="Event"/>.</summary>
    public readonly     EntityStore Store;
    /// <summary>The id of the <see cref="Entity"/> that emitted the <see cref="Event"/> with <see cref="ECS.Entity.EmitSignal{TEvent}"/>.</summary>
    public readonly     int         EntityId;
    /// <summary>The <see cref="Event"/> containing event specific data passed to <see cref="ECS.Entity.EmitSignal{TEvent}"/>.</summary>
    public readonly     TEvent      Event;
    
    // --- properties
    /// <summary>The <see cref="Entity"/> that emitted the <see cref="Event"/> with <see cref="ECS.Entity.EmitSignal{TEvent}"/> - aka the publisher.</summary>
    public              Entity      Entity => new Entity(Store, EntityId);
    
    internal Signal(EntityStore store, int id, in TEvent ev) {
        Store       = store;
        EntityId    = id;
        Event       = ev;
    }

    // e.g. "entity: 1 - signal > MyEvent"
    public override string ToString() => $"entity: {Entity.Id} - signal > {typeof(TEvent).Name}";
}
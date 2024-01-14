// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

public readonly struct Signal<TEvent> where TEvent : struct 
{
    public readonly Entity     Entity;
    public readonly TEvent     Event;
    
    internal Signal(Entity entity, in TEvent ev) {
        Entity  = entity;
        Event   = ev;
    }

    // "entity: 1 - event > Add Script: [*TestScript1]"
    public override string ToString() => $"entity: {Entity.Id} - signal > {typeof(TEvent).Name}";
}
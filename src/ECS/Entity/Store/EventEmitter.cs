// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal class EventEmitter
{
    private  static             int     _nextEventIndex             = 1;
    
    internal static int NewEventIndex(Type type)
    {
        return _nextEventIndex++;
    }
}

internal class EventEmitter<TEvent> : EventEmitter where TEvent : struct 
{
    internal readonly   Dictionary<int, Action<EventArgs<TEvent>>>  entityEvents; //  8
    
    internal EventEmitter()
    {
        entityEvents = new Dictionary<int, Action<EventArgs<TEvent>>>();
    }
    
    internal static readonly    int     EventIndex  = NewEventIndex(typeof(TEvent));
}


public partial class EntityStore
{
#region custom events
    internal static void EmitEvent<TEvent>(EntityStore store, int entityId, TEvent ev) where TEvent : struct
    {
        var emitter = GetEmitter<TEvent>(store);
        if (!emitter.entityEvents.TryGetValue(entityId, out var handlers)) {
            return;
        }
        var args = new EventArgs<TEvent>(new Entity(entityId, store), ev);
        handlers.Invoke(args);
    }

    internal static void AddEventHandler<TEvent>(EntityStore store, int entityId, Action<EventArgs<TEvent>> handler) where TEvent : struct
    {
        var emitter = GetEmitter<TEvent>(store);
        var events  = emitter.entityEvents;
        if (events.TryGetValue(entityId, out var handlers)) {
            handlers += handler;
            events[entityId] = handlers;
            return;
        }
        events.Add(entityId, handler);
    }
    
    internal static void RemoveEventHandler<TEvent>(EntityStore store, int entityId, Action<EventArgs<TEvent>> handler) where TEvent : struct
    {
        var emitter = GetEmitter<TEvent>(store);
        var events  = emitter.entityEvents;
        if (!events.TryGetValue(entityId, out var handlers)) {
            return;
        }
        handlers -= handler;
        if (handlers == null) {
            events.Remove(entityId);
            return;
        }
        events[entityId] = handler;
    }
    
    private static EventEmitter<TEvent> GetEmitter<TEvent>(EntityStore store) where TEvent : struct
    {
        var emitterIndex    = EventEmitter<TEvent>.EventIndex;
        var emitters        = store.intern.eventEmitters;
        if (emitterIndex < emitters.Length) {
            return (EventEmitter<TEvent>)emitters[emitterIndex];
        }
        emitters = store.intern.eventEmitters = new EventEmitter[emitterIndex + 1];
        var emitter = new EventEmitter<TEvent>();
        emitters[emitterIndex] = emitter;
        return emitter;
    }
    #endregion
}
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal class SignalHandler
{
    private  static             int     _nextEventIndex             = 1;
    
    internal static int NewEventIndex(Type type)
    {
        return _nextEventIndex++;
    }
}

internal class SignalHandler<TEvent> : SignalHandler where TEvent : struct 
{
    internal readonly   Dictionary<int, Action<Signal<TEvent>>>  entityEvents; //  8
    
    internal SignalHandler()
    {
        entityEvents = new Dictionary<int, Action<Signal<TEvent>>>();
    }
    
    internal static readonly    int     EventIndex  = NewEventIndex(typeof(TEvent));
}


public partial class EntityStore
{
#region custom events
    internal static void EmitSignal<TEvent>(EntityStore store, int entityId, TEvent ev) where TEvent : struct
    {
        var signalIndex    = SignalHandler<TEvent>.EventIndex;
        var signalHandlers = store.intern.signalHandlers;
        if (signalIndex >= signalHandlers.Length) {
            return;
        }
        var signalHandler = (SignalHandler<TEvent>)signalHandlers[signalIndex];
        if (!signalHandler.entityEvents.TryGetValue(entityId, out var handlers)) {
            return;
        }
        var signal = new Signal<TEvent>(new Entity(entityId, store), ev);
        handlers.Invoke(signal);
    }

    internal static void AddSignalHandler<TEvent>(EntityStore store, int entityId, Action<Signal<TEvent>> handler) where TEvent : struct
    {
        var signalHandler   = GetSignalHandler<TEvent>(store);
        var entityEvents    = signalHandler.entityEvents;
        if (entityEvents.TryGetValue(entityId, out var handlers)) {
            handlers += handler;
            entityEvents[entityId] = handlers;
            return;
        }
        entityEvents.Add(entityId, handler);
    }
    
    internal static void RemoveSignalHandler<TEvent>(EntityStore store, int entityId, Action<Signal<TEvent>> handler) where TEvent : struct
    {
        var signalHandler   = GetSignalHandler<TEvent>(store);
        var entityEvents    = signalHandler.entityEvents;
        if (!entityEvents.TryGetValue(entityId, out var handlers)) {
            return;
        }
        handlers -= handler;
        if (handlers == null) {
            entityEvents.Remove(entityId);
            return;
        }
        entityEvents[entityId] = handler;
    }
    
    private static SignalHandler<TEvent> GetSignalHandler<TEvent>(EntityStore store) where TEvent : struct
    {
        var signalIndex     = SignalHandler<TEvent>.EventIndex;
        var signalHandlers  = store.intern.signalHandlers;
        if (signalIndex < signalHandlers.Length) {
            var signalHandler = signalHandlers[signalIndex];
            if (signalHandler != null) {
                return (SignalHandler<TEvent>)signalHandlers[signalIndex];
            }
        } else {
            signalHandlers = store.intern.signalHandlers = new SignalHandler[signalIndex + 1];
        }
        var typedHandler = new SignalHandler<TEvent>();
        signalHandlers[signalIndex] = typedHandler;
        return typedHandler;

    }
    #endregion
}
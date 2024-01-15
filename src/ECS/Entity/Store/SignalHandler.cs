// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal abstract class SignalHandler
{
    private  static     int     _nextEventIndex             = 1;
    
    internal abstract   Type        Type { get; }
    internal abstract   Delegate[]  GetEntityEventHandlers(int entityId);
    
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
    
    internal override  Type         Type    => typeof(TEvent);
    internal override  Delegate[]   GetEntityEventHandlers(int entityId)
    {
        if (!entityEvents.TryGetValue(entityId, out var handlers)) {
            return null;
        }
        return handlers.GetInvocationList();
    }
    
    internal static readonly    int     EventIndex  = NewEventIndex(typeof(TEvent));
}


public partial class EntityStore
{
#region custom events
    internal static Action<Signal<TEvent>> GetSignalHandler<TEvent>(EntityStore store, int entityId) where TEvent : struct
    {
        var signalIndex    = SignalHandler<TEvent>.EventIndex;
        var signalHandlers = store.intern.signalHandlers;
        if (signalIndex >= signalHandlers.Length) {
            return null;
        }
        var signalHandler = signalHandlers[signalIndex];
        if (signalHandler == null) {
            return null;
        }
        var typedHandler = (SignalHandler<TEvent>)signalHandler;
        typedHandler.entityEvents.TryGetValue(entityId, out var handlers);
        return handlers;
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
        entityEvents[entityId] = handlers;
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
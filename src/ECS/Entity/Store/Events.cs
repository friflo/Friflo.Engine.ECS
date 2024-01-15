// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStore
{
#region add / remove script events
    private void ScriptChanged(ScriptChanged args)
    {
        if (!intern.entityScriptChanged.TryGetValue(args.Entity.Id, out var handlers)) {
            return;
        }
        handlers.Invoke(args);
    }
    
    internal static void AddScriptChangedHandler(EntityStore store, int entityId, Action<ScriptChanged> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.intern.entityScriptChanged)) {
            store.intern.scriptAdded     += store.ScriptChanged;
            store.intern.scriptRemoved   += store.ScriptChanged;
        }
    }
    
    internal static void RemoveScriptChangedHandler(EntityStore store, int entityId, Action<ScriptChanged> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.intern.entityScriptChanged)) {
            store.intern.scriptAdded     -= store.ScriptChanged;
            store.intern.scriptRemoved   -= store.ScriptChanged;
        }
    }
    #endregion



    
#region add / remove child entity events
    private void ChildEntitiesChanged(ChildEntitiesChanged args)
    {
        if (!intern.entityChildEntitiesChanged.TryGetValue(args.ParentId, out var handlers)) {
            return;
        }
        handlers.Invoke(args);
    }
    
    internal static void AddChildEntitiesChangedHandler   (EntityStore store, int entityId, Action<ChildEntitiesChanged> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.intern.entityChildEntitiesChanged)) {
            store.intern.childEntitiesChanged     += store.ChildEntitiesChanged;
        }
    }
    
    internal static void RemoveChildEntitiesChangedHandler(EntityStore store, int entityId, Action<ChildEntitiesChanged> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.intern.entityChildEntitiesChanged)) {
            store.intern.childEntitiesChanged     -= store.ChildEntitiesChanged;
        }
    }
    #endregion
    
#region subscribed event / signal delegates 
    internal static EventHandlers[] GetEventHandlers(EntityStore store, int entityId)
    {
        var eventDelegates = new List<EventHandlers>();
        AddEventHandlers(eventDelegates, store, entityId);
        
        var entityScriptChanged = store.intern.entityScriptChanged;
        if (entityScriptChanged != null) {
            if (entityScriptChanged.TryGetValue(entityId, out var handlers)) {
                eventDelegates.Add(new EventHandlers(nameof(Entity.OnScriptChanged), handlers.GetInvocationList()));
            }
        }
        var childEntitiesChanged = store.intern.entityChildEntitiesChanged;
        if (childEntitiesChanged != null) {
            if (childEntitiesChanged.TryGetValue(entityId, out var handlers)) {
                eventDelegates.Add(new EventHandlers(nameof(Entity.OnChildEntitiesChanged), handlers.GetInvocationList()));
            }
        }
        foreach (var signalHandler in store.intern.signalHandlers)
        {
            var handlers = signalHandler?.GetEntityEventHandlers(entityId);
            if (handlers != null) {
                var name = $"Signal: {signalHandler.Type.Name}";
                eventDelegates.Add(new EventHandlers(name, handlers));
            }
        }
        return eventDelegates.ToArray();
    }
    #endregion
}

internal readonly struct EventHandlers
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private  readonly   string      name;
    
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private  readonly   Delegate[]  handlers;

    public   override   string      ToString() => $"{name} - Count: {handlers.Length}";

    internal EventHandlers(string name, Delegate[] handlers) {
        this.name       = name;
        this.handlers   = handlers;
    }
}

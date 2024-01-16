// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;

// ReSharper disable UseCollectionExpression
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
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
    internal static DebugEventHandlers GetEventHandlers(EntityStore store, int entityId)
    {
        List<DebugEventHandler> eventHandlers = null;
        AddEventHandlers(ref eventHandlers, store, entityId);
        
        var entityScriptChanged = store.intern.entityScriptChanged;
        if (entityScriptChanged != null) {
            if (entityScriptChanged.TryGetValue(entityId, out var handlers)) {
                var handler = new DebugEventHandler(DebugEntityEventKind.Event, typeof(ScriptChanged), handlers.GetInvocationList());
                eventHandlers ??= new List<DebugEventHandler>();
                eventHandlers.Add(handler);
            }
        }
        var childEntitiesChanged = store.intern.entityChildEntitiesChanged;
        if (childEntitiesChanged != null) {
            if (childEntitiesChanged.TryGetValue(entityId, out var handlers)) {
                var handler = new DebugEventHandler(DebugEntityEventKind.Event, typeof(ChildEntitiesChanged), handlers.GetInvocationList());
                eventHandlers ??= new List<DebugEventHandler>();
                eventHandlers.Add(handler);
            }
        }
        AddSignalHandlers(ref eventHandlers, store, entityId);
        return new DebugEventHandlers(eventHandlers);
    }
    
    private static void AddSignalHandlers (ref List<DebugEventHandler> eventHandlers, EntityStore store, int entityId)
    {
        var list = store.intern.signalHandlers;
        if (list == null) {
            return;
        }
        if (store.nodes[entityId].signalTypeCount == 0) {
            return;
        }
        foreach (var signalHandler in list) {
            var handlers = signalHandler.GetEntityEventHandlers(entityId);
            if (handlers != null) {
                eventHandlers ??= new List<DebugEventHandler>();
                eventHandlers.Add(new DebugEventHandler(DebugEntityEventKind.Signal, signalHandler.Type, handlers));
            }
        }
    }
    #endregion
}


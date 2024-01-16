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
        if (AddEntityHandler(store, entityId, handler, HasEventFlags.ScriptChanged ,ref store.intern.entityScriptChanged)) {
            store.intern.scriptAdded     += store.ScriptChanged;
            store.intern.scriptRemoved   += store.ScriptChanged;
        }
    }
    
    internal static void RemoveScriptChangedHandler(EntityStore store, int entityId, Action<ScriptChanged> handler)
    {
        if (RemoveEntityHandler(store, entityId, handler,  HasEventFlags.ScriptChanged, store.intern.entityScriptChanged)) {
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
        if (AddEntityHandler(store, entityId, handler, HasEventFlags.ChildEntitiesChanged, ref store.intern.entityChildEntitiesChanged)) {
            store.intern.childEntitiesChanged     += store.ChildEntitiesChanged;
        }
    }
    
    internal static void RemoveChildEntitiesChangedHandler(EntityStore store, int entityId, Action<ChildEntitiesChanged> handler)
    {
        if (RemoveEntityHandler(store, entityId, handler, HasEventFlags.ChildEntitiesChanged, store.intern.entityChildEntitiesChanged)) {
            store.intern.childEntitiesChanged     -= store.ChildEntitiesChanged;
        }
    }
    #endregion
    
#region subscribed event / signal delegates 
    internal static DebugEventHandlers GetEventHandlers(EntityStore store, int entityId)
    {
        List<DebugEventHandler> eventHandlers = null;
        var hasEvent = store.nodes[entityId].hasEvent;
        AddEventHandlers(ref eventHandlers, store, entityId, hasEvent);
        
        if ((hasEvent & HasEventFlags.ScriptChanged) != 0) {
            var handlers    = store.intern.entityScriptChanged[entityId];
            var handler     = new DebugEventHandler(DebugEntityEventKind.Event, typeof(ScriptChanged), handlers.GetInvocationList());
            eventHandlers ??= new List<DebugEventHandler>();
            eventHandlers.Add(handler);
        }
        if ((hasEvent & HasEventFlags.ChildEntitiesChanged) != 0) {
            var handlers    = store.intern.entityChildEntitiesChanged[entityId];
            var handler     = new DebugEventHandler(DebugEntityEventKind.Event, typeof(ChildEntitiesChanged), handlers.GetInvocationList());
            eventHandlers ??= new List<DebugEventHandler>();
            eventHandlers.Add(handler);
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
            signalHandler.AddSignalHandler(ref eventHandlers, entityId);
        }
    }
    #endregion
}


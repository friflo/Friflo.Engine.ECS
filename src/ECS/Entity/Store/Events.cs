// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

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
    internal static EntityEventHandlers GetEventHandlers(EntityStore store, int entityId)
    {
        var eventHandlers = new List<EntityEventHandler>();
        AddEventHandlers(eventHandlers, store, entityId);
        
        var entityScriptChanged = store.intern.entityScriptChanged;
        if (entityScriptChanged != null) {
            if (entityScriptChanged.TryGetValue(entityId, out var handlers)) {
                var handler = new EntityEventHandler(EntityEventKind.Event, typeof(ScriptChanged), handlers.GetInvocationList());
                eventHandlers.Add(handler);
            }
        }
        var childEntitiesChanged = store.intern.entityChildEntitiesChanged;
        if (childEntitiesChanged != null) {
            if (childEntitiesChanged.TryGetValue(entityId, out var handlers)) {
                var handler = new EntityEventHandler(EntityEventKind.Event, typeof(ChildEntitiesChanged), handlers.GetInvocationList());
                eventHandlers.Add(handler);
            }
        }
        var list = store.intern.signalHandlers;
        if (list != null) {
            foreach (var signalHandler in store.intern.signalHandlers)
            {
                var handlers = signalHandler.GetEntityEventHandlers(entityId);
                if (handlers != null) {
                    eventHandlers.Add(new EntityEventHandler(EntityEventKind.Signal, signalHandler.Type, handlers));
                }
            }
        }
        return new EntityEventHandlers(eventHandlers);
    }
    #endregion
}

// ReSharper disable InconsistentNaming
public readonly struct EntityEventHandlers
{
    [Browse(Never)]     public          int                     TypeCount       => Array.Length;
    [Browse(Never)]     public          int                     HandlerCount    => GetHandlerCount();
    [Browse(RootHidden)]public readonly EntityEventHandler[]    Array;

                        public override string                  ToString()      => $"event types: {TypeCount}, handlers: {HandlerCount}";

    public EntityEventHandler this[int index] => Array[index];


    internal EntityEventHandlers(List<EntityEventHandler> eventHandlers) {
        Array = eventHandlers.ToArray();    
    }
    
    private int GetHandlerCount() {
        int count = 0;
        foreach (var handler in Array) {
            count += handler.handlers.Length;
        }
        return count;
    }
}

public enum EntityEventKind
{
    Event   = 0,
    Signal  = 1
}

public readonly struct EntityEventHandler
{
    [Browse(Never)]     public   readonly   Type            Type;
    [Browse(Never)]     public   readonly   EntityEventKind Kind;
    [Browse(Never)]     public              int             Count => handlers.Length;
    /// <remarks>
    /// Note! must not be public.<br/>
    /// Otherwise the <see cref="handlers"/> can be called without the event never happened.
    /// </remarks>
    [Browse(RootHidden)]internal readonly   Delegate[]      handlers;

                        public   override   string          ToString() => GetString();

    internal EntityEventHandler(EntityEventKind kind, Type type, Delegate[] handlers) {
        Kind            = kind;
        Type            = type;
        this.handlers   = handlers;
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        if (Kind == EntityEventKind.Signal) {
            sb.Append("Signal: ");
        }
        sb.Append(Type.Name);
        sb.Append(" - Count: ");
        sb.Append(handlers.Length);
        return sb.ToString();
    }
}

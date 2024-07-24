// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Text;
using System.Collections.Generic;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// ReSharper disable InconsistentNaming
/// <summary>
/// Provide the event / signal handlers of an entity using <see cref="Entity"/>.<see cref="Entity.DebugEventHandlers"/>.
/// </summary>
public readonly struct DebugEventHandlers
{
    [Browse(Never)]     public          int                 TypeCount       => Array.Length;
    [Browse(Never)]     public          int                 HandlerCount    => GetHandlerCount();
    [Browse(RootHidden)]public readonly DebugEventHandler[] Array;

                        public override string              ToString()      => GetString();

    public DebugEventHandler this[int index] => Array[index];


    internal DebugEventHandlers(List<DebugEventHandler> eventHandlers) {
        if (eventHandlers == null) {
            Array = System.Array.Empty<DebugEventHandler>();
            return;
        }
        Array = eventHandlers.ToArray();
    }
    
    private int GetHandlerCount() {
        int count = 0;
        foreach (var handler in Array) {
            count += handler.handlers.Length;
        }
        return count;
    }
    
    private string GetString() {
        if (TypeCount == 0) {
            return "event types: 0, handlers: 0";
        }
        return $"event types: {TypeCount}, handlers: {HandlerCount}";
    }
}

/// <summary>
/// Event type of a <see cref="DebugEventHandler"/>: <see cref="Event"/> or <see cref="Signal"/>. 
/// </summary>
public enum DebugEntityEventKind
{
    /// <summary>
    /// Mark event handlers added with:<br/>
    /// <see cref="Entity.OnComponentChanged"/> <br/> <see cref="Entity.OnTagsChanged"/> <br/>
    /// <see cref="Entity.OnScriptChanged"/> <br/> <see cref="Entity.OnChildEntitiesChanged"/>.
    /// </summary>
    Event   = 0,
    /// <summary>Mark signal handlers added with  <see cref="Entity.AddSignalHandler{TEvent}"/>.</summary>
    Signal  = 1
}

/// <summary>
/// Used as item type in <see cref="DebugEventHandlers"/> providing the number of handlers for a specific event <see cref="Type"/>. 
/// </summary>
public readonly struct DebugEventHandler
{
    /// <summary>The <see cref="System.Type"/> used for an event / signal handler.</summary>
    [Browse(Never)]     public   readonly   Type                    Type;
    
    /// <summary>The type of the event handlers: build-in events or custom signals.</summary>
    [Browse(Never)]     public   readonly   DebugEntityEventKind    Kind;
    
    /// <summary>Number of event handlers for a specific event <see cref="Type"/> added to an entity.</summary>
    [Browse(Never)]     public              int                     Count => handlers.Length;
    /// <remarks>
    /// Note! must not be public.<br/>
    /// Otherwise the <see cref="handlers"/> can be called without the event never happened.
    /// </remarks>
    [Browse(RootHidden)]internal readonly   Delegate[]      handlers;

                        public   override   string          ToString() => GetString();

    internal DebugEventHandler(DebugEntityEventKind kind, Type type, Delegate[] handlers) {
        Kind            = kind;
        Type            = type;
        this.handlers   = handlers;
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        if (Kind == DebugEntityEventKind.Signal) {
            sb.Append("Signal: ");
        }
        sb.Append(Type.Name);
        sb.Append(" - Count: ");
        sb.Append(handlers.Length);
        return sb.ToString();
    }
}

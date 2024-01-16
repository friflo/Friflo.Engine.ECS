// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Text;
using System.Collections.Generic;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// ReSharper disable InconsistentNaming
public readonly struct DebugEventHandlers
{
    [Browse(Never)]     public          int                 TypeCount       => Array.Length;
    [Browse(Never)]     public          int                 HandlerCount    => GetHandlerCount();
    [Browse(RootHidden)]public readonly DebugEventHandler[] Array;

                        public override string              ToString()      => $"event types: {TypeCount}, handlers: {HandlerCount}";

    public DebugEventHandler this[int index] => Array[index];


    internal DebugEventHandlers(List<DebugEventHandler> eventHandlers) {
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

public enum DebugEntityEventKind
{
    Event   = 0,
    Signal  = 1
}

public readonly struct DebugEventHandler
{
    [Browse(Never)]     public   readonly   Type                    Type;
    [Browse(Never)]     public   readonly   DebugEntityEventKind    Kind;
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

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="ScriptAddedHandler"/> added to <see cref="EntityStore.ScriptAdded"/>
/// get events on <see cref="Entity.AddScript{T}"/>
/// </summary>
public delegate void   ScriptAddedHandler    (in ScriptEventArgs e);

/// <summary>
/// A <see cref="ScriptRemovedHandler"/> added to <see cref="EntityStore.ScriptRemoved"/>
/// get events on <see cref="Entity.RemoveScript{T}()"/>
/// </summary>
public delegate void   ScriptRemovedHandler  (in ScriptEventArgs e);


public readonly struct  ScriptEventArgs
{
    public readonly     int                 entityId;
    public readonly     ChangedEventType    type; 
    public readonly     ScriptType          scriptType;
    
    public override     string              ToString() => $"entity: {entityId} - {type} {scriptType}";

    internal ScriptEventArgs(int entityId, ChangedEventType type, ScriptType scriptType)
    {
        this.entityId       = entityId;
        this.type           = type;
        this.scriptType     = scriptType;
    }
}
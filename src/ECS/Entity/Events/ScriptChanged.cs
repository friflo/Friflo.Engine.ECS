// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="ScriptChangedHandler"/> added to <see cref="EntityStore.ScriptAdded"/> get events on <see cref="Entity.AddScript{T}"/><br/>
/// <br/>
/// A <see cref="ScriptChangedHandler"/> added to <see cref="EntityStore.ScriptRemoved"/> get events on <see cref="Entity.RemoveScript{T}"/>
/// </summary>
public delegate void   ScriptChangedHandler    (in ScriptChangedArgs e);

public readonly struct  ScriptChangedArgs
{
    /// <remarks>
    /// Use <see cref="EntityStore.GetNodeById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetNodeById(args.entityId).Entity;       </code>
    /// </remarks>
    public readonly     int                 entityId;   //  4
    public readonly     ChangedEventAction  action;     //  4
    public readonly     ScriptType          scriptType; //  8
    
    public override     string              ToString() => $"entity: {entityId} - {action} {scriptType}";

    internal ScriptChangedArgs(int entityId, ChangedEventAction action, ScriptType scriptType)
    {
        this.entityId       = entityId;
        this.action         = action;
        this.scriptType     = scriptType;
    }
}
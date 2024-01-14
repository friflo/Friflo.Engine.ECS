// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct  ScriptChangedArgs
{
    public readonly     Entity              entity;     // 16
    public readonly     ChangedEventAction  action;     //  4
    public readonly     ScriptType          scriptType; //  8
    
    public override     string              ToString() => $"entity: {entity.Id} - event > {action} {scriptType}";

    internal ScriptChangedArgs(Entity entity, ChangedEventAction action, ScriptType scriptType)
    {
        this.entity         = entity;
        this.action         = action;
        this.scriptType     = scriptType;
    }
}
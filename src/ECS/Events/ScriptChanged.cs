// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

public readonly struct  ScriptChanged
{
    public readonly     Entity              Entity;     // 16
    public readonly     ChangedEventAction  Action;     //  4
    public readonly     ScriptType          ScriptType; //  8
    
    public override     string              ToString() => $"entity: {Entity.Id} - event > {Action} {ScriptType}";

    internal ScriptChanged(Entity entity, ChangedEventAction action, ScriptType scriptType)
    {
        this.Entity         = entity;
        this.Action         = action;
        this.ScriptType     = scriptType;
    }
}
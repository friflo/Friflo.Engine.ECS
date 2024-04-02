// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal struct ScriptCommand
{
    internal    byte                scriptIndex;    //  1
    internal    ScriptChangedAction action;         //  1
    internal    int                 entityId;       //  4
    internal    Script              script;         //  8

    public override string ToString() => $"entity: {entityId} - {action} [#{EntityStoreBase.Static.EntitySchema.scripts[scriptIndex].Name}]";
}

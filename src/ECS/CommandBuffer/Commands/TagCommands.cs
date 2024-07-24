// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal enum TagChange : byte
{
    Remove  = 0,
    Add     = 1,
}

internal struct TagCommand
{
    internal    byte        tagIndex;   //  1
    internal    TagChange   change;     //  1
    internal    int         entityId;   //  4

    public override string ToString() => $"entity: {entityId} - {change} [#{EntityStoreBase.Static.EntitySchema.tags[tagIndex].Name}]";
}

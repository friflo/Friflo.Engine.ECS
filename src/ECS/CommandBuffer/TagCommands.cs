// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using static Friflo.Engine.ECS.ComponentChangedAction;

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
}

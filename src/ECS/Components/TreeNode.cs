// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ComponentKey(null)]
public struct TreeNodeComponent : IComponent    // todo should be internal
{
                    public      ReadOnlySpan<int>   ChildIds    => new (childIds,0, childCount);
                    public      int                 ChildCount  => childCount;
    
//  [Browse(Never)] internal    int     parentId;       //  4   0 if entity has no parent
                    internal    int[]   childIds;       //  8
    [Browse(Never)] internal    int     childCount;     //  4   count of child entities
    
    
    public override             string  ToString() => $"count: {childCount}";
}
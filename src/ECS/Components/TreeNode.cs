// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS.Collections;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// TODO make internal, rename: TreeNodeComponent -> TreeNode
[ComponentKey(null)]
public struct TreeNode : IComponent    // todo should be internal
{
    public                      ReadOnlySpan<int>   ChildIds    => GetChildIds();
    public                      int                 ChildCount  => childIds.count;
    public          override    string              ToString()  => $"ChildCount: {childIds.count}";
    
//  [Browse(Never)] internal    int         parentId;   //  4   0 if entity has no parent
                    internal    IdArray     childIds;   //  8
                    internal    IdArrayHeap arrayHeap;  //  8
    
    internal TreeNode(IdArrayHeap arrayHeap) {
        this.arrayHeap = arrayHeap;
    }
    
    /// same as <see cref="IdArrayExtensions.GetIdSpan"/>
    private ReadOnlySpan<int>  GetChildIds()
    {
        var count = childIds.count;
        switch (count) {
            case 0:     return default;
            case 1:     return MemoryMarshal.CreateReadOnlySpan(ref childIds.start, 1);
        }
        var curPoolIndex = IdArrayHeap.PoolIndex(count);
        return new ReadOnlySpan<int>(arrayHeap.GetPool(curPoolIndex).Ids, childIds.start, count);
    }
}
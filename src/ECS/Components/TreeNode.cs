// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// TODO make internal, rename: TreeNodeComponent -> TreeNode
[ComponentKey(null)]
public struct TreeNode : IComponent    // todo should be internal
{
    public                      int                 ChildCount  => childIds.count;
    public          override    string              ToString()  => $"ChildCount: {childIds.count}";
    
//  [Browse(Never)] internal    int         parentId;   //  4   0 if entity has no parent
                    internal    IdArray     childIds;   //  8
                    internal    int[]       dummy;      //  8  todo remove
    
    /// same as <see cref="IdArrayExtensions.GetIdSpan"/>
    public ReadOnlySpan<int>  GetChildIds(EntityStore store)
    {
        var count = childIds.count;
        switch (count) {
            case 0: return default;
            case 1: return store.GetSpanId(childIds.start);
        }
        var curPoolIndex = IdArrayHeap.PoolIndex(count);
        return new ReadOnlySpan<int>(store.extension.hierarchyHeap.GetPool(curPoolIndex).Ids, childIds.start, count);
    }
}
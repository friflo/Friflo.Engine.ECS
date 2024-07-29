// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct StackArray<T>
{
#region properties
    internal        int     Count       => count;
    public override string  ToString()  => $"Count: {count}";
    #endregion

#region fields
    private         T[]     items;  //  8
    private         int     count;  //  4
    #endregion

    internal StackArray(T[] items) {
        this.items = items;
    }
    
    internal bool TryPop(out T item)
    {
        if (count > 0) {
            item = items[--count];
            items[count] = default;
            return true;
        }
        item = default;
        return false;
    }
    
    internal void Push(in T item)
    {
        int curCount    = count;
        var curItems    = items;
        if (curCount == curItems.Length) {
            curItems = ArrayUtils.Resize(ref items, Math.Max(4, 2 * curCount));
        }
        curItems[curCount] = item;
        count = curCount + 1;
    }
    
    internal void Clear() {
        var end         = count;
        var curItems    = items;
        for (int n = 0; n < end; n++) {
            curItems[n] = default;
        }
        count = 0;
    }
}
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct StackArray<T>
{
    internal        int     Count       => count;
    public override string  ToString()  => $"Count: {count}";

    private     T[]     items;
    private     int     count;
    
    internal StackArray(T[] items) {
        this.items = items;
    }
    
    internal bool TryPop(out T item)
    {
        if (count > 0) {
            item = items[--count];
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
}
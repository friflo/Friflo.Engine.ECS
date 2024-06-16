// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal struct IdArray
{
    public              int     Count => count;
    
    internal            int     start;
    internal readonly   int     count;

    public   override   string  ToString() =>$"count: {count}";

    internal IdArray(int start, int count) {
        this.start = start;
        this.count = count;
    }
}


internal static class IdArrayExtensions {

    public static ReadOnlySpan<int> GetIdSpan(this ref IdArray array, IdArrayHeap heap)
    {
        var count = array.count;
        switch (count) {
            case 0:     return default;
            case 1:     return MemoryMarshal.CreateReadOnlySpan(ref array.start, 1);
        }
        var curPoolIndex = IdArrayHeap.PoolIndex(count);
        return new ReadOnlySpan<int>(heap.GetPool(curPoolIndex).Ids, array.start, count);
    }
    
    public static void AddId(this ref IdArray array, int id, IdArrayHeap heap)
    {
        var count = array.count; 
        if (count == 0) {
            array = new IdArray(id, 1);
            return;
        }
        if (count == 1) {
            var pool        = heap.GetPool(1);
            var start       = pool.CreateArrayStart();
            var ids         = pool.Ids;
            ids[start]      = array.start;
            ids[start + 1]  = id;
            array = new IdArray(start, 2);
            return;
        }
        var newCount        = count + 1;
        var curPoolIndex    = IdArrayHeap.PoolIndex(count);
        var newPoolIndex    = IdArrayHeap.PoolIndex(newCount);
        var curPool         = heap.GetPool(curPoolIndex);
        if (newPoolIndex == curPoolIndex) {
            curPool.Ids[array.start + count] = id;
            array = new IdArray(array.start, newCount);
            return;
        }
        curPool.DeleteArrayStart(array.start);
        var curIds      = curPool.Ids;
        var newPool     = heap.GetPool(newPoolIndex);
        var newStart    = newPool.CreateArrayStart();
        var newIds      = newPool.Ids;
        for (int n = 0; n < count; n++) {
            newIds[array.start + n] = curIds[newStart + n]; 
        }
        newIds[newStart + count] = id;
        array = new IdArray(newStart, newCount);
    }
    
    public static void RemoveAt(this ref IdArray array, int index, IdArrayHeap heap)
    {
        var count = array.count;
        if (index >= count) throw new IndexOutOfRangeException();
        if (count == 1) {
            array = default;
            return;
        }
        if (count == 2) {
            var pool = heap.GetPool(1);
            pool.DeleteArrayStart(array.start);
            if (index == 0) {
                array = new IdArray(pool.Ids[array.start + 1], 1);
                return;
            }
            array = new IdArray(pool.Ids[array.start + 0], 1);
            return;
        }
        var newCount        = count - 1;
        var curPoolIndex    = IdArrayHeap.PoolIndex(count);
        var newPoolIndex    = IdArrayHeap.PoolIndex(newCount);
        var curPool         = heap.GetPool(curPoolIndex);
        if (newPoolIndex == curPoolIndex) {
            var ids = curPool.Ids;
            var end = count + array.start;
            for (int n = array.start + index + 1; n < end; n++) {
                ids[n - 1] = ids[n];
            }
            array = new IdArray(array.start, newCount);
            return;
        }
        curPool.DeleteArrayStart(array.start);
        var curIds      = curPool.Ids;
        var newPool     = heap.GetPool(newPoolIndex);
        var start       = newPool.CreateArrayStart();
        var newIds      = newPool.Ids;
        for (int n = 0; n < index; n++) {
            newIds[array.start + n] = curIds[start + n]; 
        }
        for (int n = index + 1; n < count; n++) {
            newIds[array.start + n - 1] = curIds[start + n];    
        }
        array = new IdArray(start, newCount);
    }
} 
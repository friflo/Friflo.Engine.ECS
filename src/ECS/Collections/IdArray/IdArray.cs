// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Collections;

internal struct IdArray
{
    public              int     Count => count;
    /// <summary>
    /// Stores an id in case <see cref="count"/> == 1.<br/>
    /// The start index within <see cref="IdArrayPool.ids"/> if <see cref="count"/> > 1.
    /// </summary>
    internal            int     start;
    /// <summary>
    /// Number of array ids.
    /// </summary>
    internal readonly   int     count;

    public   override   string  ToString() => GetString();

    internal IdArray(int start, int count) {
        this.start = start;
        this.count = count;
    }
    
    private string GetString()
    {
        if (count == 0) {
            return "count: 0";    
        }
        if (count == 1) {
            return $"count: 1  id: {start}";    
        }
        return $"count: {count}  index: {IdArrayHeap.PoolIndex(count)}  start: {start}";
    }
}


internal static class IdArrayExtensions {

    internal static ReadOnlySpan<int> GetSpan(this IdArray array, IdArrayHeap heap, EntityStoreBase store)
    {
        int count = array.count;
        int start = array.start;
        switch (count) {
            case 0:     return default;
            case 1:     return store.GetSpanId(start);
        }
        return new ReadOnlySpan<int>(IdArrayPool.GetIds(count, heap), start, count);
    }
    
    public static void Add(this ref IdArray array, int id, IdArrayHeap heap)
    {
        int count = array.count; 
        if (count == 0) {
            array = new IdArray(id, 1);
            return;
        }
        int curStart = array.start;
        if (count == 1) {
            var pool        = heap.GetOrCreatePool(1);
            int start       = pool.CreateArray(out var ids);
            ids[start]      = curStart;
            ids[start + 1]  = id;
            array = new IdArray(start, 2);
            return;
        }
        int newCount        = count + 1;
        int curPoolIndex    = IdArrayHeap.PoolIndex(count);
        int newPoolIndex    = IdArrayHeap.PoolIndex(newCount);
        var curPool         = IdArrayPool.GetPool(heap, curPoolIndex, out var curIds);
        if (newPoolIndex == curPoolIndex) {
            curIds[curStart + count] = id;
            array = new IdArray(curStart, newCount);
            return;
        }
        curPool.DeleteArray(curStart, out curIds);
        var newPool     = heap.GetOrCreatePool(newPoolIndex);
        int newStart    = newPool.CreateArray(out var newIds);
        
        new ReadOnlySpan<int> (curIds, curStart, count).CopyTo(
        new Span<int>         (newIds, newStart, count));

        newIds[newStart + count] = id;
        array = new IdArray(newStart, newCount);
    }
    
    public static void RemoveAt(this ref IdArray array, int index, IdArrayHeap heap, bool keepOrder = false)
    {
        int count = array.count;
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        if (count == 1) {   // index is 0
            array = default;
            return;
        }
        int curStart = array.start;
        if (count == 2) {   // index is 0 or 1
            var pool = heap.GetPool(1);
            pool.DeleteArray(curStart, out var ids);
            array = new IdArray(ids[curStart + 1 - index], 1);
            return;
        }
        int newCount        = count - 1;
        int curPoolIndex    = IdArrayHeap.PoolIndex(count);
        int newPoolIndex    = IdArrayHeap.PoolIndex(newCount);
        var curPool         = IdArrayPool.GetPool(heap, curPoolIndex, out var curIds);
        int tail            = newCount - index;
        if (newPoolIndex == curPoolIndex) {
            if (keepOrder) {
                // remove id at index
                new ReadOnlySpan<int> (curIds, curStart + index + 1, tail).CopyTo(
                new Span<int>         (curIds, curStart + index,     tail));
            } else {
                // move last id to deleted index
                curIds[curStart + index] = curIds[curStart + count - 1];
            }
            array = new IdArray(curStart, newCount);
            return;
        }
        curPool.DeleteArray(curStart, out curIds);
        var newPool     = heap.GetOrCreatePool(newPoolIndex);
        int newStart    = newPool.CreateArray(out var newIds);
        
        new ReadOnlySpan<int> (curIds, curStart,             index).CopyTo(
        new Span<int>         (newIds, newStart,             index));
        
        new ReadOnlySpan<int> (curIds, curStart + index + 1, tail).CopyTo(
        new Span<int>         (newIds, newStart + index,     tail));
        
        array = new IdArray(newStart, newCount);
    }
    
    public static void SetArray(this ref IdArray array, ReadOnlySpan<int> idSpan, IdArrayHeap heap)
    {
        int count       = array.count;
        int curStart    = array.start;
        if (count > 1 ) {
            int curPoolIndex    = IdArrayHeap.PoolIndex(count);
            var curPool         = heap.GetPool(curPoolIndex);
            curPool.DeleteArray(curStart, out _);
        }
        int newCount = idSpan.Length;
        switch (newCount) {
            case 0:
                array = default;
                return;
            case 1:
                array = new IdArray(idSpan[0], 1);
                return;
        }
        int newPoolIndex    = IdArrayHeap.PoolIndex(newCount);
        var newPool         = heap.GetOrCreatePool(newPoolIndex);
        int newStart        = newPool.CreateArray(out var newIds);
        idSpan.CopyTo(new Span<int>(newIds, newStart, newCount));
        array = new IdArray(newStart, newCount);
    }
    
    public static void InsertAt(this ref IdArray array, int index, int id, IdArrayHeap heap)
    {
        int count = array.count; 
        if (index < 0 || index > count) throw new IndexOutOfRangeException();
        if (count == 0) {
            array = new IdArray(id, 1);
            return;
        }
        int curStart = array.start;
        if (count == 1) {
            var pool        = heap.GetOrCreatePool(1);
            int start       = pool.CreateArray(out var ids);
            if (index == 0) {
                ids[start]      = id;
                ids[start + 1]  = curStart;
            } else {
                ids[start]      = curStart;
                ids[start + 1]  = id;
            }
            array = new IdArray(start, 2);
            return;
        }
        int newCount        = count + 1;
        int curPoolIndex    = IdArrayHeap.PoolIndex(count);
        int newPoolIndex    = IdArrayHeap.PoolIndex(newCount);
        var curPool         = IdArrayPool.GetPool(heap, curPoolIndex, out var curIds);
        int tail            = count - index;
        int curIndex        = curStart + index;
        if (newPoolIndex == curPoolIndex) {
            new ReadOnlySpan<int> (curIds, curIndex,     tail).CopyTo(
            new Span<int>         (curIds, curIndex + 1, tail));
            curIds[curIndex] = id;
            array = new IdArray(curStart, newCount);
            return;
        }
        curPool.DeleteArray(curStart, out curIds);
        var newPool     = heap.GetOrCreatePool(newPoolIndex);
        int newStart    = newPool.CreateArray(out var newIds);
  
        new ReadOnlySpan<int> (curIds, curStart, index).CopyTo(
        new Span<int>         (newIds, newStart, index));
        
        new ReadOnlySpan<int> (curIds, curStart + index,     tail).CopyTo(
        new Span<int>         (newIds, newStart + index + 1, tail));

        newIds[newStart + index] = id;
        array = new IdArray(newStart, newCount);
    }
    
    /*
    internal static void Clear(this ref IdArray array, IdArrayHeap heap)
    {
        int count = array.count;
        if (count <= 1) {   // index is 0
            array = default;
            return;
        }
        var curPoolIndex    = IdArrayHeap.PoolIndex(count);
        var curPool         = heap.GetPool(curPoolIndex);
        curPool.DeleteArray(array.start, out _);
        array = default;
    } */

    internal static void SetAt(this ref IdArray array, int positionIndex, int value, IdArrayHeap heap)
    {
        int count = array.count;
        if (count == 1) {   // index is 0
            array.start = value;
            return;
        }
        var ids = IdArrayPool.GetIds(count, heap);
        ids[array.start + positionIndex] = value;
    }
    
    internal static int GetAt(this IdArray array, int positionIndex, IdArrayHeap heap)
    {
        int count = array.count;
        int start = array.start;
        if (count == 1) {   // index is 0
            return start;
        }
        var ids = IdArrayPool.GetIds(count, heap);
        return ids[start + positionIndex];
    }
} 
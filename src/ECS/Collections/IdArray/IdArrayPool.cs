// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Collections;

internal sealed class IdArrayPool
{
    public              int             Count       => count;
    internal            int             FreeCount   => freeStarts.Count;
    
    private             int[]           ids;
    private             StackArray<int> freeStarts;
    private  readonly   int             arraySize;
    private             int             freeStart;
    private             int             maxStart;
    private             int             count;

    public override string ToString() => $"arraySize: {arraySize} count: {count}";

    internal IdArrayPool(int poolIndex)
    {
        arraySize   = 2 << (poolIndex - 1);
        ids         = Array.Empty<int>();
        freeStarts  = new StackArray<int>(Array.Empty<int>());
    }
    
    internal static int[] GetIds(int count, IdArrayHeap heap)
    {
        return heap.pools[IdArrayHeap.PoolIndex(count)].ids;
    }
    
    internal static IdArrayPool GetPool(IdArrayHeap heap, int index, out int[] ids)
    {
        var pool = heap.pools[index];
        ids = pool.ids;
        return pool;
    }

    /// <summary>
    /// Return the start index within the returned newIds.
    /// </summary>
    internal int CreateArray(out int[] newIds)
    {
        count++;
        if (freeStarts.TryPop(out var start)) {
            newIds = ids;
            return start;
        }
        start       = freeStart;
        freeStart   = start + arraySize;
        if (start < maxStart) {
            newIds = ids;
            return start;
        }
        maxStart    = Math.Max(4 * arraySize, 2 * maxStart);
        ArrayUtils.Resize(ref ids, maxStart);
        newIds = ids;
        return start;
    }
    
    /// <summary>
    /// Delete the array with the passed start index.
    /// </summary>
    internal void DeleteArray(int start, out int[] ids)
    {
        count--;
        ids = this.ids;
        if (count > 0) {
            freeStarts.Push(start);
            return;
        }
        freeStart = 0;
        freeStarts.Clear();
    }
    
}
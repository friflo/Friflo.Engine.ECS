// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Runtime.InteropServices;
using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal sealed class IdArrayHeap
{
    private readonly IdArrayPool[] pools;
    
    internal IdArrayHeap() {
        
        pools = new IdArrayPool[32];
        for (int n = 1; n < 32; n++) {
            pools[n] = new IdArrayPool(n);
        }
    }
    
    internal ReadOnlySpan<int> IdSpan(IdArray array)
    {
        var count = array.count;
        switch (count) {
            case 0:     return default;
            case 1:     return MemoryMarshal.CreateReadOnlySpan(ref array.start, 1);
        }
        var curPoolIndex = PoolIndex(count);
        return new ReadOnlySpan<int>(pools[curPoolIndex].ids, array.start, count);
    }
    
    internal IdArray AddId(IdArray array, int id)
    {
        var count = array.count; 
        if (count == 0) {
            return new IdArray(id, 1);
        }
        if (count == 1) {
            var pool        = pools[1];
            var start       = pool.CreateArrayStart();
            var ids         = pool.ids;
            ids[start]      = array.start;
            ids[start + 1]  = id;
            return new IdArray(start, 2);
        }
        var newCount        = count + 1;
        var curPoolIndex    = PoolIndex(count);
        var newPoolIndex    = PoolIndex(newCount);
        var curPool         = pools[curPoolIndex];
        if (newPoolIndex == curPoolIndex) {
            curPool.ids[array.start + count] = id;
            return new IdArray(array.start, newCount);
        }
        curPool.freeStarts.Push(array.start);
        var curIds      = curPool.ids;
        var newPool     = pools[newPoolIndex];
        var newStart    = newPool.CreateArrayStart();
        var newIds      = newPool.ids;
        for (int n = 0; n < count; n++) {
            newIds[array.start + n] = curIds[newStart + n]; 
        }
        newIds[newStart + count] = id;
        return new IdArray(newStart, newCount);
    }
    
    internal IdArray RemoveAt(IdArray array, int index)
    {
        var count = array.count;
        if (index >= count) throw new IndexOutOfRangeException();
        if (count == 1) {
            return default;
        }
        if (count == 2) {
            var pool = pools[1];
            pool.freeStarts.Push(array.start);
            if (index == 0) {
                return new IdArray(pool.ids[array.start + 1], 1);    
            }
            return new IdArray(pool.ids[array.start + 0], 1);
        }
        var newCount        = count - 1;
        var curPoolIndex    = PoolIndex(count);
        var newPoolIndex    = PoolIndex(newCount);
        var curPool         = pools[curPoolIndex];
        if (newPoolIndex == curPoolIndex) {
            var ids = curPool.ids;
            var end = count + array.start;
            for (int n = array.start + index + 1; n < end; n++) {
                ids[n - 1] = ids[n];
            }
            return new IdArray(array.start, newCount);
        }
        curPool.freeStarts.Push(array.start);
        var curIds      = curPool.ids;
        var newPool     = pools[newPoolIndex];
        var start       = newPool.CreateArrayStart();
        var newIds      = newPool.ids;
        for (int n = 0; n < index; n++) {
            newIds[array.start + n] = curIds[start + n]; 
        }
        for (int n = index + 1; n < count; n++) {
            newIds[array.start + n - 1] = curIds[start + n];    
        }
        return new IdArray(start, newCount);
    }
    

    private static int PoolIndex(int count)
    {
#if NETCOREAPP3_0_OR_GREATER
        return 64 - System.Numerics.BitOperations.LeadingZeroCount((ulong)(count - 1));
#else
        return LeadingZeroCount(count - 1);
#endif
    }
    
    internal static int LeadingZeroCount(int i)
    {
        if (i == 0) return 32;

        int n = 1;

        if ((i >> 16) == 0) { n = n + 16; i = i << 16; }
        if ((i >> 24) == 0) { n = n + 8;  i = i << 8;  }
        if ((i >> 28) == 0) { n = n + 4;  i = i << 4;  }
        if ((i >> 30) == 0) { n = n + 2;  i = i << 2;  }
        n = n - (i >> 31);

        return n;
    }
}
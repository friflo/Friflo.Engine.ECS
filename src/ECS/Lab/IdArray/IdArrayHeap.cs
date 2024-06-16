// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Runtime.InteropServices;
using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class IdArrayHeap
{
    private readonly IdArrayPool[] pools;
    
    internal IdArrayHeap() {
        
        pools = new IdArrayPool[32];
        for (int n = 1; n < 32; n++) {
            pools[n] = new IdArrayPool(n);
        }
    }
    
    internal ReadOnlySpan<int> Ids(IdArray array)
    {
        var count = array.count;
        switch (count) {
            case 0:     return default;
            case 1:     return default; // MemoryMarshal.CreateReadOnlySpan(ref array.start, 1); todo
        }
        var curPoolIndex = PoolIndex(count);
        return new ReadOnlySpan<int>(pools[curPoolIndex].ids, array.start, count);
    }
    
    internal IdArray Add(IdArray array, int id)
    {
        var count = array.count; 
        if (count == 0) {
            return new IdArray(id, 1);
        }
        if (count == 1) {
            var newPool = pools[1];
            var start   = newPool.CreateArrayStart();
            var ids     = newPool.ids;
            ids[start]      = array.start;
            ids[start + 1]  = id;
            return new IdArray(start, 2);
        }
        else
        {
            var newCount        = count + 1;
            var curPoolIndex    = PoolIndex(count);
            var newPoolIndex    = PoolIndex(newCount);
            var curPool         = pools[curPoolIndex];
            if (newPoolIndex == curPoolIndex) {
                curPool.ids[array.start + count] = id;
                return new IdArray(array.start, newCount);
            }
            curPool.freeStarts.Push(array.start);
            var curIds  = curPool.ids;
            var newPool = pools[newPoolIndex];
            var start   = newPool.CreateArrayStart();
            var newIds  = newPool.ids;
            for (int n = 0; n < count; n++) {
                newIds[array.start + n] = curIds[start + n]; 
            }
            newIds[start + count] = id;
            return new IdArray(start, newCount);
        }
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
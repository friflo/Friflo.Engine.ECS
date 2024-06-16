// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class IdArrays
{
    private readonly IdArrayPool[] pools;
    
    internal IdArrays() {
        
        pools = new IdArrayPool[32];
        for (int n = 1; n < 32; n++) {
            pools[n] = new IdArrayPool();
        }
    }
    
    internal IdArray Add(IdArray array, int id)
    {
        var count = array.count; 
        if (count == 0) {
            return new IdArray(id, 1);
        }
        var curPoolIndex    = PoolIndex(count);
        var poolIndex       = PoolIndex(count + 1);
        
        if (poolIndex == curPoolIndex) {
            var curStart        = (2 << curPoolIndex) * array.index;
            pools[curPoolIndex].ids[curStart + count] = id;
            return new IdArray(curPoolIndex, count + 1);
        }
        pools[curPoolIndex].freeList.Push(poolIndex);
        
        var arrayIndex = pools[poolIndex].GetArrayIndex();

        var newStart    = (2 << poolIndex) * arrayIndex;
        
        
        
        return new IdArray(arrayIndex, count + 1);
    }
    

    private static int PoolIndex(int count)
    {
#if NETCOREAPP3_0_OR_GREATER
        return 32 - System.Numerics.BitOperations.LeadingZeroCount((ulong)count);
#else
        return 32 - LeadingZeroCount(count);
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
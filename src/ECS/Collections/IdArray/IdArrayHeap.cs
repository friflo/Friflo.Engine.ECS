// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Diagnostics.CodeAnalysis;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Collections;

internal readonly struct IdArrayHeap
{
#region properties
    public              int             Count               => GetCount();
    public   override   string          ToString()          => pools == null ? "null" : $"count: {Count}";
    #endregion
    
#region fields
    internal readonly   IdArrayPool[]   pools;
    #endregion
    
    public IdArrayHeap() {
        pools = new IdArrayPool[32];
    }

    internal IdArrayPool GetPool        (int index) => pools[index];
    internal IdArrayPool GetOrCreatePool(int index) => pools[index] ??= new IdArrayPool(index);
    
    private int GetCount()
    {
        int count = 0;
        for (int n = 1; n < 32; n++) {
            var pool = pools[n];
            if (pool == null) continue;
            count += pool.Count;
        }
        return count;
    }
    
    public Entities GetEntities(EntityStore store, IdArray array)
    {
        var count = array.count;
        var start = array.start;
        switch (count) {
            case 0: return  new Entities(store);
            case 1: return  new Entities(store, start);
        }
        var ids = IdArrayPool.GetIds(count, this);
        return new Entities(store, ids, start, count);
    }

    internal static int PoolIndex(int count)
    {
#if NETCOREAPP3_0_OR_GREATER
        return 32 - System.Numerics.BitOperations.LeadingZeroCount((uint)(count - 1));
#else
        return 32 - LeadingZeroCount((uint)(count - 1));
#endif
    }
    
    // C# - Fast way of finding most and least significant bit set in a 64-bit integer - Stack Overflow
    // https://stackoverflow.com/questions/31374628/fast-way-of-finding-most-and-least-significant-bit-set-in-a-64-bit-integer
    [ExcludeFromCodeCoverage]
    internal static int LeadingZeroCount(uint i)
    {
        if (i == 0) return 32;
        uint n = 1;

        if (i >> 16 == 0) { n += 16; i <<= 16; }
        if (i >> 24 == 0) { n +=  8; i <<=  8; }
        if (i >> 28 == 0) { n +=  4; i <<=  4; }
        if (i >> 30 == 0) { n +=  2; i <<=  2; }
        n -= i >> 31;
        return (int)n;
    }
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Diagnostics.CodeAnalysis;

// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class IdArrayHeap
{
    public              int             Count               => GetCount();
    public   override   string          ToString()          => $"count: {Count}";
    internal            IdArrayPool     GetPool(int index)  => pools[index] ??= new IdArrayPool(index);
    
    private  readonly   IdArrayPool[]   pools;
    
    internal IdArrayHeap() {
        pools = new IdArrayPool[32];
    }
    
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

    internal static int PoolIndex(int count)
    {
#if NETCOREAPP3_0_OR_GREATER
        return 64 - System.Numerics.BitOperations.LeadingZeroCount((ulong)(count - 1));
#else
        return LeadingZeroCount(count - 1);
#endif
    }
    
    [ExcludeFromCodeCoverage]
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
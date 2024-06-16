// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal sealed class IdArrayPool
{
    public              int             Count   => count;
    internal            int[]           Ids     => ids;
    
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
    
    internal int CreateArrayStart()
    {
        count++;
        if (freeStarts.TryPop(out var start)) {
            return start;
        }
        if (freeStart < maxStart) {
            return freeStart += arraySize;
        }
        maxStart = Math.Max(4 * arraySize, 2 * maxStart);
        ArrayUtils.Resize(ref ids, maxStart);
        return freeStart;
    }
    
    internal void DeleteArrayStart(int start)
    {
        count--;
        freeStarts.Push(start);
    }
    
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal class IdArrayPool
{
    internal            int[]       ids;
    internal readonly   Stack<int>  freeStarts;
    private  readonly   int         arraySize;
    private             int         freeStart;
    private             int         maxStart;

    public override string ToString() => $"arraySize: {arraySize}";

    internal IdArrayPool(int poolIndex)
    {
        arraySize   = 2 << (poolIndex - 1);
        ids         = Array.Empty<int>();
        freeStarts  = new Stack<int>();
    }
    
    internal int CreateArrayStart()
    {
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
    
}
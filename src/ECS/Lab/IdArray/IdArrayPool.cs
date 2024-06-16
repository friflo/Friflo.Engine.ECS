// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal class IdArrayPool
{
    internal            int[]       ids;
    internal readonly   Stack<int>  freeList;
    internal            int         arrayCount;
    internal            int         maxCount;
    
    internal IdArrayPool() {
        ids         = Array.Empty<int>();
        freeList    = new Stack<int>();
    }
    
    internal int GetArrayIndex()
    {
        if (freeList.TryPop(out var poolIndex)) {
            return poolIndex;
        }
        if (arrayCount < maxCount) {
            return arrayCount++;
        }
        return -1;
    }
    
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal readonly struct IdArray
{
    public int Count => count;
    
    internal readonly int start;
    internal readonly int count;
    
    
    internal IdArray(int start, int count) {
        this.start = start;
        this.count = count;
    }
}
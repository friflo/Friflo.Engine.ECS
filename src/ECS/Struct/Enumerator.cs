// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal class StructEnumerator<T> where T : struct
{
    private readonly StructHeap<T> heap;
    
    internal  StructEnumerator(StructHeap<T> heap) {
        this.heap = heap;
    }
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal readonly struct StructChunk<T>
    where T : struct, IStructComponent
{
    // Note: must not contain any other field. Reasons:
    // - to save memory as many StructChunk<T>'s are stored within a StructHeap<T>.chunks[].
    // - to enable maximum efficiency when GC iterate components[] for collection.
    internal readonly   T[]     components;   // 8
    
    public   override   string  ToString() => components == null ? "" : "used";
    
    internal StructChunk (int chunkSize) {
        components  = new T[chunkSize];
    }
}

public readonly struct Chunk<T>
    where T : struct, IStructComponent
{
    public              Span<T> Values => new(values, 0, count);

    private readonly    T[]     values;
    private readonly    int     count;
    
    public Chunk(T[] values, int count) {
        this.values = values;
        this.count  = count;
    }
}


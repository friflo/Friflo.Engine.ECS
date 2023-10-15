// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal readonly struct StructChunk<T>
    where T : struct, IStructComponent
{
    internal readonly   T[]       components;   // 8
    
    public override string ToString() => components == null ? "" : "used";
    
    internal StructChunk (int count) {
        components  = new T[count];
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


// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct Chunk<T>
    where T : struct, IComponent
{
    public              Span<T>     Values      => new(values, 0, length);
    public override     string      ToString()  => $"Length: {length}";

    private readonly    T[]         values;     //  8
    public  readonly    int         length;     //  4
    
    internal Chunk(T[] values, T[] copy, int length) {
        this.length      = length;
        if (copy == null) {
            this.values = values;
        } else {
            Array.Copy(values, copy, length);
            this.values = copy;
        }
    }
}
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct Chunk<T>
    where T : struct
{
    internal readonly   T[]       components;
    public              Span<T>   Values => new (components, 0, 1);
    
    public override string ToString() => components == null ? "" : "used";
    
    internal Chunk (int count) {
        components  = new T[count];
    }
}


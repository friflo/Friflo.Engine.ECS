// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <remarks>
/// <b>Note!</b> Must not contain any other field. Reasons:<br/>
/// - to save memory as many <see cref="StructChunk{T}"/>'s are stored within a StructHeap{T}.chunks<br/>
/// - to enable maximum efficiency when GC iterate StructHeap{T}.chunks" for collection.
/// </remarks>
[Obsolete("replaced by StructHeap<T>.components")]
internal readonly struct StructChunk<T>
    where T : struct, IComponent
{
    // Note! Must not contain any other field. See <remarks>
    internal readonly   T[]     components;   // 8
    
    public   override   string  ToString() => components == null ? "" : "used";
    
    internal StructChunk (int chunkSize) {
        components  = new T[chunkSize];
    }
}

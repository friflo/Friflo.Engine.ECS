// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// public interface IStructComponent { } 

internal sealed class StructHeap<T> : StructHeap where T : struct // , IStructComponent - not using an interface for struct components
{
    // --- internal
    internal    StructChunk<T>[]  chunks;
    
    private StructHeap(int heapIndex, string keyName, int capacity)
        : base (heapIndex, keyName, typeof(T))
    {
        chunks      = new StructChunk<T>[1];
        chunks[0]   = new StructChunk<T>(capacity);
    }
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(heapIndex, keyName, capacity);
    }
    
    internal static StructHeap Create(int capacity) {
        var componentIndex = ComponentIndex;
        if (componentIndex == MissingAttribute) {
            var msg = $"Missing attribute [StructComponent(\"<key>\")] on type: {typeof(T).Namespace}.{typeof(T).Name}";
            throw new InvalidOperationException(msg);
        }
        return new StructHeap<T>(componentIndex, ComponentKey, capacity);
    }
    
    internal override void SetCapacity(int capacity)
    {
        // todo fix this
        for (int n = 0; n < chunks.Length; n++)
        {
            var cur         = chunks[n].components;
            var newChunk    = new StructChunk<T>(capacity);
            chunks[n] = newChunk;
            for (int i = 0; i < cur.Length; i++) {
                newChunk.components[i]= cur[i];    
            }
        }
    }
    
    internal override void MoveComponent(int from, int to)
    {
        chunks[to   / ChunkSize].components[to   % ChunkSize] =
        chunks[from / ChunkSize].components[from % ChunkSize];
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap target, int targetPos)
    {
        var targetHeap = (StructHeap<T>)target;
        targetHeap.chunks[targetPos / ChunkSize].components[targetPos % ChunkSize] =
                   chunks[sourcePos / ChunkSize].components[sourcePos % ChunkSize];
    }
    
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override object GetComponentDebug (int archIndex) {
        return chunks[archIndex / ChunkSize].components[archIndex % ChunkSize];
    }
    
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    int     ComponentIndex  = NewComponentIndex(typeof(T), out ComponentKey);
    
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    string  ComponentKey;
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Fliox.Engine.ECS;

public readonly struct Component<T>
    where T : struct
{
    // --- public properties
    public              Type            Type        => heap.type;
    public              string          KeyName     => heap.keyName;
    public              string          HeapInfo    => heap.GetString();

    // --- internal fields
    private  readonly   StructHeap<T>   heap;
    private  readonly   GameEntity      entity;

    public  override    string          ToString() => GetString();
    
    internal Component (StructHeap<T> heap, GameEntity entity) {
        this.entity = entity;
        this.heap   = heap;
    }
    
    public ref T Value {
        get {
            // ReSharper disable once UnusedVariable
            var heapTemp = entity.archetype.HeapMap[StructHeap<T>.ComponentIndex].heapIndex;  // force NullReferenceException if entity was removed
            return ref heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize];
        }
    }
    
    private string GetString() {
        return $"[{typeof(T).Name}]";
    }
}
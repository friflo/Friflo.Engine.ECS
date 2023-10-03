// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// public interface IStructComponent { } 

internal sealed class StructHeap<T> : StructHeap where T : struct // , IStructComponent - not using an interface for struct components
{
    // --- internal
    internal            StructChunk<T>[]    chunks;
    private  readonly   TypeMapper<T>       typeMapper;
    
    internal StructHeap(int structIndex, string structKey, int capacity, TypeMapper<T> mapper)
        : base (structIndex, structKey, typeof(T))
    {
        typeMapper  = mapper;
        chunks      = new StructChunk<T>[1];
        chunks[0]   = new StructChunk<T>(capacity);
    }
    
    internal override StructHeap CreateHeap(int capacity, TypeStore typeStore) {
        var mapper = typeStore.GetTypeMapper<T>();
        return new StructHeap<T>(structIndex, structKey, capacity, mapper);
    }
    
    internal static StructHeap Create(in ArchetypeConfig config) {
        var structIndex = StructIndex;
        if (structIndex == MissingAttribute) {
            var msg = $"Missing attribute [StructComponent(\"<key>\")] on type: {typeof(T).Namespace}.{typeof(T).Name}";
            throw new InvalidOperationException(msg);
        }
        if (structIndex >= config.maxStructIndex) {
            const string msg = $"number of structs exceed EntityStore.{nameof(EntityStore.maxStructIndex)}";
            throw new InvalidOperationException(msg);
        }
        var mapper = config.typeStore.GetTypeMapper<T>();
        return new StructHeap<T>(structIndex, StructKey, config.capacity, mapper);
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
    internal override object GetComponentDebug (int compIndex) {
        return chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
    }
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        ref var value = ref chunks[compIndex / ChunkSize].components[compIndex % ChunkSize];
        return writer.WriteAsBytesMapper(value, typeMapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        chunks[compIndex / ChunkSize].components[compIndex % ChunkSize]
            = reader.ReadMapper(typeMapper, json);  // todo avoid boxing within typeMapper, T is struct
    }
    
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    int     StructIndex  = NewStructIndex(typeof(T), out StructKey);
    
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    string  StructKey;
}

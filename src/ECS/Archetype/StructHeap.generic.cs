// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal sealed class StructHeap<T> : StructHeap
    where T : struct, IStructComponent
{
    // --- internal fields
    internal            StructChunk<T>[]    chunks;     // 8 - Length: 1, 2, 4, 8
    private  readonly   TypeMapper<T>       typeMapper; // 8
    
    // --- static internal
    internal static readonly    int     StructIndex  = NewStructIndex(typeof(T), out StructKey);
    internal static readonly    string  StructKey;
    
    internal StructHeap(int structIndex, int chunkSize, TypeMapper<T> mapper)
        : base (structIndex)
    {
        typeMapper  = mapper;
        chunks      = new StructChunk<T>[1];
        chunks[0]   = new StructChunk<T>(chunkSize);
    }
    
    public override   string      ToString() {
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(StructType.Name);
        sb.Append("] chunks - Count: ");
        sb.Append(chunks.Length);
#if DEBUG
        sb.Append(", EntityCount: ");
        sb.Append(archetype.EntityCount);
#endif
        return sb.ToString();
    }
    
    internal override Type  StructType => typeof(T);
    
    internal override void SetChunkCapacity(int newChunkCount, int chunkCount, int newChunkLength, int chunkLength)
    {
        // --- set new chunks array if requested. Length values: 1, 2, 4, 8, 16, ...
        if (chunkLength != newChunkLength)
        {
            var newChunks = new StructChunk<T>[newChunkLength];
            for (int n = 0; n < chunkCount; n++) {
                newChunks[n] = chunks[n];
            }
            chunks = newChunks;
        }
        // --- add new chunks if needed
        for (int n = chunkCount; n < newChunkCount; n++) {
            chunks[n] = new StructChunk<T>(ChunkSize);
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
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <remarks>
/// <b>Note:</b> Should not contain any other fields. Reasons:<br/>
/// - to enable maximum efficiency when GC iterate <see cref="Archetype.structHeaps"/> <see cref="Archetype.heapMap"/>
///   for collection.
/// </remarks>
internal sealed class StructHeap<T> : StructHeap
    where T : struct, IComponent
{
    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    internal            T[]                 components;   // 8
    // internal         StructChunk<T>[]    chunks;     // 8 - Length: 1, 2, 4, 8
    private  readonly   TypeMapper<T>       typeMapper; // 8
    
    // --- static internal
    internal static readonly    int     StructIndex  = StructUtils.NewStructIndex(typeof(T), out StructKey);
    internal static readonly    string  StructKey;
    
    internal StructHeap(int structIndex, TypeMapper<T> mapper)
        : base (structIndex)
    {
        typeMapper  = mapper;
        components  = new T[512];
        // chunks      = new StructChunk<T>[1];
        // chunks[0]   = new StructChunk<T>(ChunkSize);
    }
    
    protected override void DebugInfo(out int length) {
        length = components.Length;
        /* count = 0;
        foreach (var chunk in chunks) {
            if (chunk.components != null) {
                count++;
            }
            break;
        }
        length = chunks.Length; */
    }
    
    internal override Type  StructType => typeof(T);
    
    internal override void SetChunkCapacity(int newChunkCount, int chunkCount, int newChunkLength, int chunkLength)
    {
        if (chunkLength != newChunkLength)
        {
            var newLength       = newChunkLength * ChunkSize;
            var newComponents   = new T [newLength];
            var curComponents   = components;
            var curLength       = chunkCount     * ChunkSize;
            for (int i = 0; i < curLength; i++) {
                newComponents[i] = curComponents[i];
            }
            components = newComponents;
        } else {
            // throw new InvalidOperationException("expect different chunk lengths");
        }
        /*
        AssertChunksLength(chunks.Length, chunkLength);
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
            AssertChunkComponentsNull(chunks[n].components);
            chunks[n] = new StructChunk<T>(ChunkSize);
        }
        */
    }
    
    internal override void MoveComponent(int from, int to)
    {
        components[to] = components[from];
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap target, int targetPos)
    {
        var targetHeap = (StructHeap<T>)target;
        targetHeap.components[targetPos] = components[sourcePos];
    }
    
    /// <remarks>
    /// Copying a component using an assignment can only be done for <see cref="ComponentType.blittable"/>
    /// <see cref="ComponentType"/>'s.<br/>
    /// If not <see cref="ComponentType.blittable"/> serialization must be used.
    /// </remarks>
    internal override void CopyComponent(int sourcePos, int targetPos)
    {
        components[targetPos] = components[sourcePos];
    }
    
    
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override IComponent GetComponentDebug (int compIndex) {
        return components[compIndex];
    }
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        ref var value = ref components[compIndex];
        return writer.WriteAsBytesMapper(value, typeMapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        components[compIndex] = reader.ReadMapper(typeMapper, json);  // todo avoid boxing within typeMapper, T is struct
    }
}

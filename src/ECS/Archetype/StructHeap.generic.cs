// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

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
    internal            T[]             components;     //  8
    internal            T               componentStash; //  sizeof(T)
    private  readonly   TypeMapper<T>   typeMapper;     //  8
    
    // --- static internal
    internal static readonly    int     StructIndex  = StructUtils.NewStructIndex(typeof(T));
    
    internal StructHeap(int structIndex, TypeMapper<T> mapper)
        : base (structIndex)
    {
        typeMapper      = mapper;
        components      = new T[ArchetypeUtils.MinCapacity];
    }
    
    internal void StashComponent(int compIndex) {
        componentStash = components[compIndex];
    }
    
    // --- StructHeap
    protected override  int     ComponentsLength    => components.Length;

    internal  override  Type    StructType          => typeof(T);
    
    internal override void ResizeComponents    (int capacity, int count) {
        var newComponents   = new T[capacity];
        var curComponents   = components;
        var source          = new ReadOnlySpan<T>(curComponents, 0, count);
        var target          = new Span<T>(newComponents);
        source.CopyTo(target);
        components = newComponents;
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
    /// Copying a component using an assignment can only be done for <see cref="ComponentType.IsBlittable"/>
    /// <see cref="ComponentType"/>'s.<br/>
    /// If not <see cref="ComponentType.IsBlittable"/> serialization must be used.
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
    internal override IComponent GetComponentStashDebug() => componentStash;
    
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override IComponent GetComponentDebug(int compIndex) => components[compIndex];
    
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        ref var value = ref components[compIndex];
        return writer.WriteAsBytesMapper(value, typeMapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        components[compIndex] = reader.ReadMapper(typeMapper, json);  // todo avoid boxing within typeMapper, T is struct
    }
}

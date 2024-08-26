// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <remarks>
/// <b>Note:</b> Should not contain any other fields. Reasons:<br/>
/// - to enable maximum efficiency when GC iterate <see cref="Archetype.structHeaps"/> <see cref="Archetype.heapMap"/>
///   for collection.
/// </remarks>
internal sealed class StructHeap<T> : StructHeap, IComponentStash<T>
    where T : struct, IComponent
{
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    public override IComponent  GetStashDebug() => componentStash;
    public          ref T       GetStashRef()     => ref componentStash;
    
    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    internal            T[]                 components;     //  8
    internal            T                   componentStash; //  sizeof(T)
    private  readonly   ComponentType<T>    componentType;  //  8
    
    internal StructHeap(int structIndex, ComponentType<T> componentType)
        : base (structIndex)
    {
        this.componentType  = componentType;
        components          = new T[ArchetypeUtils.MinCapacity];
    }
    
    internal override void StashComponent(int compIndex) {
        componentStash = components[compIndex];
    }
    
    internal override  void SetBatchComponent(BatchComponent[] components, int compIndex)
    {
        this.components[compIndex] = ((BatchComponent<T>)components[structIndex]).value;
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
    
    internal  override  void SetComponentDefault (int compIndex) {
        components[compIndex] = default;
    }
    
    internal  override  void SetComponentsDefault (int compIndexStart, int count)
    {
        var componentSpan = new Span<T>(components, compIndexStart, count);
        componentSpan.Clear();
    }
  
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override IComponent GetComponentDebug(int compIndex) => components[compIndex];
    
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        ref var value = ref components[compIndex];
        return writer.WriteAsBytesMapper(value, componentType.TypeMapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        components[compIndex] = reader.ReadMapper(componentType.TypeMapper, json);  // todo avoid boxing within typeMapper, T is struct
    }
}

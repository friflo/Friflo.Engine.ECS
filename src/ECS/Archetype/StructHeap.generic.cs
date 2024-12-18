// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS.Index;
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
internal sealed class StructHeap<T> : StructHeap, IComponentStash<T>
    where T : struct
{
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    public override object      GetStashDebug() => componentStash;
    public          ref T       GetStashRef()     => ref componentStash;
    
    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    internal            T[]                 components;     //  8
    internal            T                   componentStash; //  sizeof(T)
    
    internal StructHeap(int structIndex)
        : base (structIndex)
    {
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
    internal override void CloneComponent(int sourcePos, int targetPos, in CopyContext context)
    {
        var copyValue = CopyValueUtils<T>.CopyValue;
        ref var source = ref components[sourcePos];
        ref var target = ref components[targetPos];
        if (copyValue == null) {
            target = source;
        } else {
            copyValue(source, ref target, context);
        }
        if (!StructInfo<T>.HasIndex) {
            return;
        }
        var targetEntity = context.target;
        StoreIndex.AddIndex(targetEntity.store, targetEntity.Id, source);
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
    internal override object GetComponentDebug(int compIndex) => components[compIndex];
    
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        var mapper = (TypeMapper<T>)writer.TypeCache.GetTypeMapper(typeof(T));
        ref var value = ref components[compIndex];
        return writer.WriteAsBytesMapper(value, mapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        var mapper = (TypeMapper<T>)reader.TypeCache.GetTypeMapper(typeof(T));
        components[compIndex] = reader.ReadMapper(mapper, json);  // todo avoid boxing within typeMapper, T is struct
    }
    
    internal override  void UpdateIndex (Entity entity) {
        StoreIndex.UpdateIndex(entity.store, entity.Id, components[entity.compIndex], this);
    }
    
    internal override  void AddIndex (Entity entity) {
        StoreIndex.AddIndex(entity.store, entity.Id, components[entity.compIndex]);
    }
    
    internal override  void RemoveIndex (Entity entity) {
        StoreIndex.RemoveIndex(entity.store, entity.Id, this);
    }
}

// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS.Index;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable UseNullPropagation
// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <remarks>
/// <b>Note:</b> Should not contain any other fields. Reasons:<br/>
/// - to enable maximum efficiency when GC iterate <see cref="Archetype.structHeaps"/> <see cref="Archetype.heapMap"/>
///   for collection.
/// </remarks>
internal sealed class StructFloatSoA<T> : StructHeap<T>, IComponentStash<T>
    where T : struct
{
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    public override object  GetStashDebug() => componentStash;
    public override ref T   GetStashRef()   => ref componentStash;

    public override T[]     Components      => Unsafe.As<float[], T[]>(ref components); // the ultimate cowboy move

    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    private         float[] components;     //  8
    private         int     stride;         //  4
    
    internal StructFloatSoA(int structIndex)
        : base (structIndex)
    {
        stride      = ArchetypeUtils.MinCapacity;
        components  = new float[ArchetypeUtils.MinCapacity * 3];
    }
    
    internal override ref T GetComponent(int index) {
        throw new NotImplementedException(); // TODO add descriptive message
        // return ref components[index];
    }
    
    internal override void SetComponent(int index, T component) {
        ComponentToSoA(component, components, index, stride);
    }
    
    internal override T GetSoA(int index) {
        return GetComponentFromSoA(components, index, stride);
    }
    
    
    // --- StructHeap
    internal override void StashComponent(int compIndex) {
        componentStash = GetComponentFromSoA(components, compIndex, stride);
    }
    
    internal override  void SetBatchComponent(BatchComponent[] batchComponents, int compIndex)
    {
        ComponentToSoA(((BatchComponent<T>)batchComponents[structIndex]).value, components, compIndex, stride);
    }
    
    // --- StructHeap
    protected override  int     ComponentsLength    => components.Length;

    internal  override  Type    StructType          => typeof(T);
    
    internal override void ResizeComponents    (int capacity, int count) {
        int srcStride   = stride;
        stride          = capacity;
        var dst         = new float[capacity * 3];
        var src         = components;
        new ReadOnlySpan<float>(src, 0,             count).CopyTo(new Span<float>(dst, 0,            count));
        new ReadOnlySpan<float>(src, srcStride,     count).CopyTo(new Span<float>(dst, capacity,     count));
        new ReadOnlySpan<float>(src, srcStride * 2, count).CopyTo(new Span<float>(dst, capacity * 2, count));
        components = dst;
    }
    
    internal override void MoveComponent(int from, int to)
    {
        var localComponents = components;
        var localStride     = stride;
        localComponents[to]                     = localComponents[from];
        localComponents[to + localStride]       = localComponents[from + localStride];
        localComponents[to + localStride * 2]   = localComponents[from + localStride * 2];
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap target, int targetPos)
    {
        var targetHeap = (StructFloatSoA<T>)target;
        targetHeap.components[targetPos] = components[sourcePos];
    }
    
    internal override void CopyComponent(int sourcePos, StructHeap targetHeap, int targetPos, in CopyContext context, long updateIndexTypes)
    {
        var targetSoA       = (StructFloatSoA<T>)targetHeap;
        var src             = components;
        var dst             = targetSoA.components;
        var targetStride    = targetSoA.stride; 
        dst[targetPos]                      = src[sourcePos];
        dst[targetPos + targetStride]       = src[sourcePos + stride];
        dst[targetPos + targetStride * 2]   = src[sourcePos + stride * 2];
    }
    
    private static void AddOrUpdateIndex(in T source, in T target, in Entity targetEntity, StructFloatSoA<T> targetHeap, long updateIndexTypes)
    {
        throw new NotImplementedException();
    /*  if (((1 << StructInfo<T>.Index) & updateIndexTypes) == 0) {
            StoreIndex.AddIndex(targetEntity.store, targetEntity.Id, source);
        } else {
            targetHeap.componentStash = target;
            StoreIndex.UpdateIndex(targetEntity.store, targetEntity.Id, source, targetHeap);
        } */
    }
    
    internal  override  void SetComponentDefault (int compIndex) {
        // TODO Really necessary? 
        ComponentToSoA(default, components, compIndex, stride);
    }
    
    internal  override  void SetComponentsDefault (int compIndexStart, int count)
    {
        // TODO is this necessary?
        new Span<float>(components, compIndexStart,              count).Clear();
        new Span<float>(components, compIndexStart + stride,     count).Clear();
        new Span<float>(components, compIndexStart + stride * 2, count).Clear();
    }
  
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override object GetComponentDebug(int compIndex) => components[compIndex];
    
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        var mapper = (TypeMapper<T>)writer.TypeCache.GetTypeMapper(typeof(T));
        var value = GetComponentFromSoA(components, compIndex, stride);
        return writer.WriteAsBytesMapper(value, mapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        var mapper = (TypeMapper<T>)reader.TypeCache.GetTypeMapper(typeof(T));
        var value = reader.ReadMapper(mapper, json);  // todo avoid boxing within typeMapper, T is struct
        ComponentToSoA(value, components, compIndex, stride);
    }
    
    internal override  void UpdateIndex (Entity entity) {
        throw new NotImplementedException();
        // StoreIndex.UpdateIndex(entity.store, entity.Id, components[entity.compIndex], this);
    }
    
    internal override  void AddIndex (Entity entity) {
        StoreIndex.AddIndex(entity.store, entity.Id, components[entity.compIndex]);
    }
    
    internal override  void RemoveIndex (Entity entity) {
        throw new NotImplementedException();
        // StoreIndex.RemoveIndex(entity.store, entity.Id, this);
    }
    
    internal  override  bool GetComponentMember<TField> (int compIndex, MemberPath memberPath, out TField value, out Exception exception) {
        throw new NotImplementedException();
    /*  var getter = (MemberPathGetter<T, TField>)memberPath.getter;
        try {
            exception = null;
            value = getter(components[compIndex]);
            return true;
        }
        catch (Exception e) {
            exception = e;
            value = default;
            return false;
        } */
    }
    
    internal override  bool SetComponentMember<TField>(Entity entity, MemberPath memberPath, TField value, Delegate onMemberChanged, out Exception exception)
    {
        throw new NotImplementedException();
    /*  var setter          = (MemberPathSetter<T, TField>)memberPath.setter;
        ref var component   = ref components[entity.compIndex];
        var oldValue        = component;
        try {
            exception = null;
            setter(ref component, value);
            if (onMemberChanged != null) {
                ((OnMemberChanged<T>)onMemberChanged)(ref component, entity, memberPath.path, oldValue);
            }
            return true;
        }
        catch (Exception e) {
            exception = e;
            return false;
        } */
    }
    
    // --- Utils
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComponentToSoA(T src, float[] dst, int index, int stride)
    {
        ref float   component   = ref Unsafe.As<T, float>(ref src);  // TODO may not be supported by Unity
        Span<float> span        = MemoryMarshal.CreateSpan(ref component, 3);
        dst[index]              = span[0];
        dst[index + stride]     = span[1];
        dst[index + stride * 2] = span[2];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetComponentFromSoA(float[] src, int index, int stride)
    {
        Span<float> component = stackalloc float[3];
        component[0] = src[index];
        component[1] = src[index + stride];
        component[2] = src[index + stride * 2];
        return Unsafe.As<float, T>(ref component[0]);  // TODO may not be supported by Unity
    }
}

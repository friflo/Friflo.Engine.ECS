// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
internal sealed class StructSoAVector4<T> : StructHeap<T>
    where T : struct
{
    private const   int     LaneCount = 4;
        
    public override T[]     Components      => Unsafe.As<float[], T[]>(ref components); // the ultimate cowboy move

    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    private         float[] components;     //  8
    private         int     stride;         //  4
    
    internal StructSoAVector4(int structIndex)
        : base (structIndex)
    {
        stride      = CalcCapacity(ArchetypeUtils.MinCapacity, SimdInfo<T>.SimdStep);
        components  = new float[stride * LaneCount];
    }
    
    internal override ref T GetComponentRef(int index) {
        throw new InvalidOperationException($"Component '{typeof(T).Name}' is stored as SoA. GetComponent() requires AoS storage. Remove attribute [SoA] or use GetSoA() instead.");
    }
    
    internal override T GetComponentValue(int index) {
        return GetComponentFromSoA (components, index, stride);
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
    protected override  int     ComponentsLength    => components.Length / LaneCount;

    internal  override  Type    StructType          => typeof(T);
    
    internal override void ResizeComponents    (int capacity, int count) {
        int srcStride   = stride;
        stride          = CalcCapacity(capacity, SimdInfo<T>.SimdStep);
        var dst         = new float[stride * LaneCount];
        var src         = components;
        new ReadOnlySpan<float>(src, 0,             count).CopyTo(new Span<float>(dst, 0,          count));
        new ReadOnlySpan<float>(src, srcStride,     count).CopyTo(new Span<float>(dst, stride,     count));
        new ReadOnlySpan<float>(src, srcStride * 2, count).CopyTo(new Span<float>(dst, stride * 2, count));
        new ReadOnlySpan<float>(src, srcStride * 3, count).CopyTo(new Span<float>(dst, stride * 3, count));
        components = dst;
    }
    
    internal override void MoveComponent(int from, int to)
    {
        var localComponents = components;
        var localStride     = stride;
        localComponents[to]                     = localComponents[from];
        localComponents[to + localStride]       = localComponents[from + localStride];
        localComponents[to + localStride * 2]   = localComponents[from + localStride * 2];
        localComponents[to + localStride * 3]   = localComponents[from + localStride * 3];
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap targetHeap, int targetPos)
    {
        var targetSoA       = (StructSoAVector4<T>)targetHeap;
        var src             = components;
        var dst             = targetSoA.components;
        var targetStride    = targetSoA.stride; 
        dst[targetPos]                      = src[sourcePos];
        dst[targetPos + targetStride]       = src[sourcePos + stride];
        dst[targetPos + targetStride * 2]   = src[sourcePos + stride * 2];
        dst[targetPos + targetStride * 3]   = src[sourcePos + stride * 3];
    }
    
    internal override void CopyComponent(int sourcePos, StructHeap targetHeap, int targetPos, in CopyContext context, long updateIndexTypes)
    {
        CopyComponentTo(sourcePos, targetHeap, targetPos);
    }
    
    internal  override  void SetComponentDefault (int compIndex) {
        ComponentToSoA(default, components, compIndex, stride);
    }
    
    internal  override  void SetComponentsDefault (int compIndexStart, int count)
    {
        new Span<float>(components, compIndexStart,              count).Clear();
        new Span<float>(components, compIndexStart + stride,     count).Clear();
        new Span<float>(components, compIndexStart + stride * 2, count).Clear();
        new Span<float>(components, compIndexStart + stride * 3, count).Clear();
    }
  
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override object GetComponentDebug(int compIndex) => GetComponentFromSoA(components, compIndex, stride);
    
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        var mapper = (TypeMapper<T>)writer.TypeCache.GetTypeMapper(typeof(T));
        var value = GetComponentFromSoA(components, compIndex, stride);
        return writer.WriteAsBytesMapper(value, mapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        var mapper = (TypeMapper<T>)reader.TypeCache.GetTypeMapper(typeof(T));
        var value = reader.ReadMapper(mapper, json);
        ComponentToSoA(value, components, compIndex, stride);
    }
    
    internal override  void UpdateIndex (Entity entity) {
        throw new NotSupportedException();
        // StoreIndex.UpdateIndex(entity.store, entity.Id, components[entity.compIndex], this);
    }
    
    internal override  void AddIndex (Entity entity) {
        throw new NotSupportedException();
        // StoreIndex.AddIndex(entity.store, entity.Id, components[entity.compIndex]);
    }
    
    internal override  void RemoveIndex (Entity entity) {
        throw new NotSupportedException();
        // StoreIndex.RemoveIndex(entity.store, entity.Id, this);
    }
    
    internal  override  bool GetComponentMember<TField> (int compIndex, MemberPath memberPath, out TField value, out Exception exception) {
        // var getter = (MemberPathGetter<T, TField>)memberPath.getter;
        try {
            exception = null;
            var component = GetComponentFromSoA(components, compIndex, stride);
            value = Unsafe.As<T, TField>(ref component);
            return true;
        }
        catch (Exception e) {
            exception = e;
            value = default;
            return false;
        }
    }
    
    internal override  bool SetComponentMember<TField>(Entity entity, MemberPath memberPath, TField value, Delegate onMemberChanged, out Exception exception)
    {
        var component   = Unsafe.As<TField,T>(ref value);
        var oldValue    = GetComponentFromSoA(components, entity.compIndex, stride);
        try {
            exception = null;
            ComponentToSoA(component, components, entity.compIndex, stride);
            if (onMemberChanged != null) {
                ((OnMemberChanged<T>)onMemberChanged)(ref component, entity, memberPath.path, oldValue);
            }
            return true;
        }
        catch (Exception e) {
            exception = e;
            return false;
        }
    }
    
    // --- Utils
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComponentToSoA(T src, float[] dst, int index, int stride)
    {
        Span<float> span        = MemoryMarshal.CreateSpan(ref Unsafe.As<T, float>(ref src), LaneCount);
        dst[index]              = span[0];
        dst[index + stride]     = span[1];
        dst[index + stride * 2] = span[2];
        dst[index + stride * 3] = span[3];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetComponentFromSoA(float[] src, int index, int stride)
    {
        T result = default;
        var component   = MemoryMarshal.CreateSpan(ref Unsafe.As<T, float>(ref result), LaneCount);
        component[0] = src[index];
        component[1] = src[index + stride];
        component[2] = src[index + stride * 2];
        component[3] = src[index + stride * 3];
        return result;
    }
}

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
internal sealed class StructSoAFloat<T> : StructHeap<T>
    where T : struct
{
    private const   int     LaneCount = 1;
    
    public override T[]     Components      => Unsafe.As<float[], T[]>(ref components); // the ultimate cowboy move
    
    internal override (T[],int) GetComponents () => (Unsafe.As<float[], T[]>(ref components), simdOffset);

    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    private         float[] components;     //  8
    private         int     simdOffset;     //  8
    
    internal StructSoAFloat(int structIndex)
        : base (structIndex)
    {
        var capacity = CalcCapacity(ArchetypeUtils.MinCapacity, 8);
        components  = AllocateAligned(capacity, out simdOffset);
    }
    
    internal override ref T GetComponentRef(int index) {
        return ref Unsafe.As<float[], T[]>(ref components)[simdOffset + index];
    }
    
    internal override T GetComponentValue(int index) {
        return GetComponentFromSoA (index);
    }
    
    internal override void SetComponent(int index, T component) {
        ComponentToSoA(component, index);
    }
    
    internal override T GetSoA(int index) {
        return Unsafe.As<float[], T[]>(ref components)[simdOffset + index];
    }
    
    
    // --- StructHeap
    internal override void StashComponent(int compIndex) {
        componentStash = GetComponentFromSoA(compIndex);
    }
    
    internal override  void SetBatchComponent(BatchComponent[] batchComponents, int compIndex)
    {
        ComponentToSoA(((BatchComponent<T>)batchComponents[structIndex]).value, compIndex);
    }
    
    // --- StructHeap
    protected override  int     ComponentsLength    => components.Length - simdOffset;

    internal  override  Type    StructType          => typeof(T);
    
    internal override void ResizeComponents (int newCapacity, int count)
    {
        var capacity    = CalcCapacity(newCapacity, 8);
        var oldOffset   = simdOffset;
        var dst         = AllocateAligned(capacity, out simdOffset);
        if (count > 0) {
            new ReadOnlySpan<float>(components, oldOffset, count).CopyTo(new Span<float>(dst, simdOffset, count));
        }
        components = dst;
    }
    
    internal override void MoveComponent(int from, int to)
    {
        var localComponents = components;
        var offest          = simdOffset; 
        localComponents[offest + to] = localComponents[offest + from];
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap targetHeap, int targetPos)
    {
        var targetSoA       = (StructSoAFloat<T>)targetHeap;
        var dst             = targetSoA.components;
        dst[targetSoA.simdOffset + targetPos] = components[simdOffset + sourcePos];
    }
    
    internal override void CopyComponent(int sourcePos, StructHeap targetHeap, int targetPos, in CopyContext context, long updateIndexTypes)
    {
        CopyComponentTo(sourcePos, targetHeap, targetPos);
    }
    
    internal  override  void SetComponentDefault (int compIndex) {
        ComponentToSoA(default, compIndex);
    }
    
    internal  override  void SetComponentsDefault (int compIndexStart, int count)
    {
        new Span<float>(components, simdOffset + compIndexStart, count).Clear();
    }
  
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override object GetComponentDebug(int compIndex) => GetComponentFromSoA(compIndex);
    
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        var mapper = (TypeMapper<T>)writer.TypeCache.GetTypeMapper(typeof(T));
        var value = GetComponentFromSoA(compIndex);
        return writer.WriteAsBytesMapper(value, mapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        var mapper = (TypeMapper<T>)reader.TypeCache.GetTypeMapper(typeof(T));
        var value = reader.ReadMapper(mapper, json);
        ComponentToSoA(value, compIndex);
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
            var component = GetComponentFromSoA(compIndex);
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
        var oldValue    = GetComponentFromSoA(entity.compIndex);
        try {
            exception = null;
            ComponentToSoA(component, entity.compIndex);
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
    private void ComponentToSoA(T src, int index)
    {
        Span<float> span        = MemoryMarshal.CreateSpan(ref Unsafe.As<T, float>(ref src), LaneCount);
        components[simdOffset + index] = span[0];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T GetComponentFromSoA(int index)
    {
        T result = default;
        var component   = MemoryMarshal.CreateSpan(ref Unsafe.As<T, float>(ref result), LaneCount);
        component[0]    = components[simdOffset + index];
        return result;
    }
}

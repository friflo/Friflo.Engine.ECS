// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
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
internal sealed class StructAoSoAVector2<T> : StructHeap<T>
    where T : struct
{
    private const   int     FieldCount = 2;
        
    public override T[]     Components      => throw new NotSupportedException();
    
    internal override (object,int) GetComponents () => (components, simdOffset);

    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    private         float[] components;     //  8
    private         int     simdOffset;     //  8

    
    internal StructAoSoAVector2(int structIndex)
        : base (structIndex)
    {
        var capacity = SimdUtils.CalcCapacity<T>(ArchetypeUtils.MinCapacity);
        components  = AllocateAligned(capacity * FieldCount, out simdOffset);
    }
    
    internal override ref T GetComponentRef(int index) {
        throw new InvalidOperationException($"Component '{typeof(T).Name}' is stored as SoA. GetComponent() requires AoS storage. Remove attribute [SoA] or use GetSoA() instead.");
    }
    
    internal override T GetComponentValue(int index) {
        return GetComponentFromSoA (index);
    }
    
    internal override void SetComponent(int index, T component) {
        ComponentToSoA(component, index);
    }
    
    internal override T GetSoA(int index) {
        return GetComponentFromSoA(index);
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
    protected override  int     ComponentsLength    => (components.Length - simdOffset) / FieldCount;

    internal  override  Type    StructType          => typeof(T);
    
    internal override void ResizeComponents    (int newCapacity, int count)
    {
        var capacity    = SimdUtils.CalcCapacity<T>(newCapacity);
        var oldOffset   = simdOffset;
        var dst         = AllocateAligned(capacity * FieldCount, out simdOffset);

        int tilesToCopy  = (count + 7) >> 3;
        int floatsToCopy = tilesToCopy << 4; // tiles * 16
        
        if (floatsToCopy > 0) {
            new ReadOnlySpan<float>(components, oldOffset, floatsToCopy).CopyTo(new Span<float>(dst, simdOffset, floatsToCopy));
        }
        components = dst;
    }
    
    internal override void MoveComponent(int from, int to)
    {
        ComponentToSoA(GetComponentFromSoA(from), to);
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap targetHeap, int targetPos)
    {
        var targetSoA       = (StructAoSoAVector2<T>)targetHeap;
        targetSoA.ComponentToSoA(GetComponentFromSoA(sourcePos), targetPos);
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
        int i = 0;
        var localComponents = components;
        
        while (i < count && ((compIndexStart + i) & 7) != 0) {
            ComponentToSoA(default, compIndexStart + i++);
        }

        int remaining = count - i;
        int fullTiles = remaining >> 3;

        if (fullTiles > 0) {
            int startTile   = (compIndexStart + i) >> 3;
            int startFloat  = startTile << 4; // startTile * 16
            int totalFloats = fullTiles << 4; // fullTiles * 16
            localComponents.AsSpan(simdOffset + startFloat, totalFloats).Clear();
            i += fullTiles << 3;
        }

        while (i < count) {
            ComponentToSoA(default, compIndexStart + i++);
        }
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
        float[] dst   = components;
        int tileStart = (index >> 3) << 4; // index / 8 * 16
        int lane      = index & 7;
        int baseIdx   = simdOffset + tileStart + lane;

        ref float srcBase = ref Unsafe.As<T, float>(ref src);
        
        dst[baseIdx + 8] = Unsafe.Add(ref srcBase, 1); // Y
        dst[baseIdx]     = srcBase;                    // X
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T GetComponentFromSoA(int index)
    {
        float[] src   = components;
        int tileStart = (index >> 3) << 4;
        int lane      = index & 7;
        int baseIdx   = simdOffset + tileStart + lane;

        T result = default;
        ref float resBase = ref Unsafe.As<T, float>(ref result);

        Unsafe.Add(ref resBase, 1) = src[baseIdx + 8]; // Y
        resBase                    = src[baseIdx];     // X
        return result;
    }
}

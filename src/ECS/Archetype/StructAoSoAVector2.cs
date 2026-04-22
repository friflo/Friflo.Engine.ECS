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
        
    public override T[]     Components      => Unsafe.As<float[], T[]>(ref components); // the ultimate cowboy move

    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    private         float[] components;     //  8
    private         int     simdOffset;     //  8

    
    internal StructAoSoAVector2(int structIndex)
        : base (structIndex)
    {
        var capacity = CalcCapacity(ArchetypeUtils.MinCapacity, SimdUtils.LaneWidth);
        components  = AllocateAligned(capacity * FieldCount, out simdOffset);
    }
    
    internal override ref T GetComponentRef(int index) {
        throw new InvalidOperationException($"Component '{typeof(T).Name}' is stored as SoA. GetComponent() requires AoS storage. Remove attribute [SoA] or use GetSoA() instead.");
    }
    
    internal override T GetComponentValue(int index) {
        return GetComponentFromSoA (components, index);
    }
    
    internal override void SetComponent(int index, T component) {
        ComponentToSoA(component, components, index);
    }
    
    internal override T GetSoA(int index) {
        return GetComponentFromSoA(components, index);
    }
    
    
    // --- StructHeap
    internal override void StashComponent(int compIndex) {
        componentStash = GetComponentFromSoA(components, compIndex);
    }
    
    internal override  void SetBatchComponent(BatchComponent[] batchComponents, int compIndex)
    {
        ComponentToSoA(((BatchComponent<T>)batchComponents[structIndex]).value, components, compIndex);
    }
    
    // --- StructHeap
    protected override  int     ComponentsLength    => components.Length / FieldCount;

    internal  override  Type    StructType          => typeof(T);
    
    internal override void ResizeComponents    (int newCapacity, int count)
    {
        var capacity = CalcCapacity(newCapacity, 8);
        var dst = AllocateAligned(capacity * FieldCount, out simdOffset);

        int tilesToCopy  = (count + 7) >> 3;
        int floatsToCopy = tilesToCopy << 4; // tiles * 16
        
        if (floatsToCopy > 0) {
            new ReadOnlySpan<float>(components, 0, floatsToCopy).CopyTo(new Span<float>(dst, 0, floatsToCopy));
        }
        components = dst;
    }
    
    internal override void MoveComponent(int from, int to)
    {
        ComponentToSoA(GetComponentFromSoA(components, from), components, to);
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap targetHeap, int targetPos)
    {
        var targetSoA       = (StructAoSoAVector2<T>)targetHeap;
        ComponentToSoA(GetComponentFromSoA(components, sourcePos), targetSoA.components, targetPos);
    }
    
    internal override void CopyComponent(int sourcePos, StructHeap targetHeap, int targetPos, in CopyContext context, long updateIndexTypes)
    {
        CopyComponentTo(sourcePos, targetHeap, targetPos);
    }
    
    internal  override  void SetComponentDefault (int compIndex) {
        ComponentToSoA(default, components, compIndex);
    }
    
    internal  override  void SetComponentsDefault (int compIndexStart, int count)
    {
        int i = 0;
        var localComponents = components;
        
        while (i < count && ((compIndexStart + i) & 7) != 0) {
            ComponentToSoA(default, localComponents, compIndexStart + i++);
        }

        int remaining = count - i;
        int fullTiles = remaining >> 3;

        if (fullTiles > 0) {
            int startTile   = (compIndexStart + i) >> 3;
            int startFloat  = startTile << 4; // startTile * 16
            int totalFloats = fullTiles << 4; // fullTiles * 16
            localComponents.AsSpan(startFloat, totalFloats).Clear();
            i += fullTiles << 3;
        }

        while (i < count) {
            ComponentToSoA(default, localComponents, compIndexStart + i++);
        }
    }
  
    /// <summary>
    /// Method only available for debugging. Reasons:<br/>
    /// - it boxes struct values to return them as objects<br/>
    /// - it allows only reading struct values
    /// </summary>
    internal override object GetComponentDebug(int compIndex) => GetComponentFromSoA(components, compIndex);
    
    
    internal override Bytes Write(ObjectWriter writer, int compIndex) {
        var mapper = (TypeMapper<T>)writer.TypeCache.GetTypeMapper(typeof(T));
        var value = GetComponentFromSoA(components, compIndex);
        return writer.WriteAsBytesMapper(value, mapper);
    }
    
    internal override void Read(ObjectReader reader, int compIndex, JsonValue json) {
        var mapper = (TypeMapper<T>)reader.TypeCache.GetTypeMapper(typeof(T));
        var value = reader.ReadMapper(mapper, json);
        ComponentToSoA(value, components, compIndex);
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
            var component = GetComponentFromSoA(components, compIndex);
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
        var oldValue    = GetComponentFromSoA(components, entity.compIndex);
        try {
            exception = null;
            ComponentToSoA(component, components, entity.compIndex);
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
    private static void ComponentToSoA(T src, float[] dst, int index)
    {
        int tileStart = (index >> 3) << 4; // index / 8 * 16
        int lane      = index & 7;
        int baseIdx   = tileStart + lane;

        ref float srcBase = ref Unsafe.As<T, float>(ref src);
        
        dst[baseIdx + 8] = Unsafe.Add(ref srcBase, 1); // Y
        dst[baseIdx]     = srcBase;                    // X
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetComponentFromSoA(float[] src, int index)
    {
        int tileStart = (index >> 3) << 4;
        int lane      = index & 7;
        int baseIdx   = tileStart + lane;

        T result = default;
        ref float resBase = ref Unsafe.As<T, float>(ref result);

        Unsafe.Add(ref resBase, 1) = src[baseIdx + 8]; // Y
        resBase                    = src[baseIdx];     // X
        return result;
    }
}

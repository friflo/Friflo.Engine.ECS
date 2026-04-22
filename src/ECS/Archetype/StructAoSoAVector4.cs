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
internal sealed class StructAoSoAVector4<T> : StructHeap<T>
    where T : struct
{
    private const   int     FieldCount = 4;
        
    public override T[]     Components      => Unsafe.As<float[], T[]>(ref components); // the ultimate cowboy move

    // Note: Should not contain any other field. See class <remarks>
    // --- internal fields
    private         float[] components;     //  8
    private         int     simdOffset;     //  8

    
    internal StructAoSoAVector4(int structIndex)
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
        var capacity = CalcCapacity(newCapacity, SimdUtils.LaneWidth);
        var dst = AllocateAligned(capacity * FieldCount, out simdOffset);

        // Because X, Y, Z, W for 8 entities are packed together,
        // only ONE copy operation needed for the active data.
        int tilesToCopy  = (count + 7) >> 3;        // How many 8-float tiles are currently used
        int floatsToCopy = tilesToCopy << 5;        // tiles * 32
        
        new ReadOnlySpan<float>(components, 0, floatsToCopy).CopyTo(new Span<float>(dst, 0, floatsToCopy));
        components = dst;
    }
    
    internal override void MoveComponent(int from, int to)
    {
        ComponentToSoA(GetComponentFromSoA(components, from), components, to);
    }
    
    internal override void CopyComponentTo(int sourcePos, StructHeap targetHeap, int targetPos)
    {
        var targetSoA       = (StructAoSoAVector4<T>)targetHeap;
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
        // Handle "Head" (Alignment to Tile boundary)
        // Clear entities one by one until we hit a multiple of 8
        while (i < count && ((compIndexStart + i) & 7) != 0) {
            ComponentToSoA(default, localComponents, compIndexStart + i++);
        }
        // Now that we are tile-aligned, we can clear whole 32-float blocks
        int remaining = count - i;
        int fullTiles = remaining >> 3; // count / 8

        if (fullTiles > 0) {
            int stride      = SimdUtils.LaneWidth * FieldCount;
            int startTile   = (compIndexStart + i) >> 3;
            int startFloat  = startTile * stride;
            int totalFloats = fullTiles * stride;
            localComponents.AsSpan(startFloat, totalFloats).Clear();
            i += fullTiles << 3;
        }
        // Handle "Tail" (The Remainder)
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
        int tileIndex = index >> 3;     // entityIndex / 8  - Find which 8-element tile we belong to
        int lane      = index & 7;      // entityIndex % 8  - Find our position within that 8-element tile
        
        // Calculate the start of this specific 8-vector tile (32 floats total for Vector4)
        // Offset = TileIndex * (8 lanes * 4 components)
        int tileStart = tileIndex << 5; // tileIndex * 32

        Span<float> span = MemoryMarshal.CreateSpan(ref Unsafe.As<T, float>(ref src), FieldCount);
        dst[tileStart + lane]          = span[0]; // X
        dst[tileStart + lane + 8]      = span[1]; // Y
        dst[tileStart + lane + 16]     = span[2]; // Z
        dst[tileStart + lane + 24]     = span[3]; // W
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetComponentFromSoA(float[] src, int index)
    {
        // Same math as the setter
        int tileIndex = index >> 3;     // entityIndex / 8
        int lane      = index & 7;      // entityIndex % 8
        int tileStart = tileIndex << 5; // tileIndex * 32 (8 lanes * 4 components)

        T result = default;
        var component = MemoryMarshal.CreateSpan(ref Unsafe.As<T, float>(ref result), FieldCount);

        // Everything is now in a 128-byte window (2 cache lines)
        component[0] = src[tileStart + lane];      // X
        component[1] = src[tileStart + lane + 8];  // Y
        component[2] = src[tileStart + lane + 16]; // Z
        component[3] = src[tileStart + lane + 24]; // W
        return result;
    }
}

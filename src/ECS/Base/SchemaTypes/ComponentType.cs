// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Engine.ECS.SchemaTypeKind;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public abstract class ComponentType : SchemaType
{
    /// <summary>
    /// The index in <see cref="EntitySchema.Components"/>.<br/>
    /// </summary>
    public   readonly   int         StructIndex;    //  4
    public   readonly   bool        IsBlittable;    //  4
    public   readonly   int         StructSize;     //  4
    
    internal abstract   StructHeap  CreateHeap();
    internal abstract   bool                    RemoveEntityComponent  (Entity entity);
    internal abstract   ComponentChangedAction  AddEntityComponent     (Entity entity);
    internal abstract   ComponentChangedAction  AddEntityComponentValue(Entity entity, object value);
    
    protected ComponentType(string componentKey, int structIndex, Type type, int byteSize)
        : base (componentKey, type, Component)
    {
        StructIndex = structIndex;
        IsBlittable = IsBlittableType(type);
        StructSize  = byteSize;
    }
}

internal sealed class ComponentType<T> : ComponentType 
    where T : struct, IComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"Component: [{typeof(T).Name}]";

    internal ComponentType(string componentKey, int structIndex, TypeMapper<T> typeMapper)
        : base(componentKey, structIndex, typeof(T), ByteSize)
    {
        this.typeMapper = typeMapper;
    }
    
    internal override bool RemoveEntityComponent(Entity entity) {
        int archIndex = 0;
        return EntityStoreBase.RemoveComponent<T>(entity.Id, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, StructIndex);
    }
    
    internal override ComponentChangedAction AddEntityComponent(Entity entity) {
        int archIndex = 0;
        return EntityStoreBase.AddComponent<T>(entity.Id, StructIndex, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, default);
    }
    
    internal override ComponentChangedAction AddEntityComponentValue(Entity entity, object value) {
        int archIndex = 0;
        var componentValue = (T)value;
        return EntityStoreBase.AddComponent(entity.Id, StructIndex, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, componentValue);
    }
    
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(StructIndex, typeMapper);
    }
    
    private static              int GetByteSize()   => Unsafe.SizeOf<T>();

    // ReSharper disable StaticMemberInGenericType
    internal static readonly    int ByteSize        = GetByteSize();

    /// <summary>
    /// The returned padding enables using <see cref="Vector128"/>, <see cref="Vector256"/> and Vector512 (512 bits = 64 bytes) operations <br/>
    /// on <see cref="StructHeap{T}"/>.<see cref="StructHeap{T}.components"/>
    /// without the need of an additional for loop to process the elements at the end of a <see cref="Span{T}"/>.
    /// </summary>
    internal static readonly    int PadCount512     = 64 / ByteSize - 1;
    
    /// <summary> 256 bits = 32 bytes </summary>
    internal static readonly    int PadCount256     = 32 / ByteSize - 1;
    
    /// <summary> 128 bits = 16 bytes </summary>
    internal static readonly    int PadCount128     = 16 / ByteSize - 1;
}
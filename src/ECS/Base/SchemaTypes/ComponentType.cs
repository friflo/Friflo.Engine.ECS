// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Engine.ECS.SchemaTypeKind;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public abstract class ComponentType : SchemaType
{
    /// <summary>
    /// The index in <see cref="EntitySchema.Components"/>.<br/>
    /// </summary>
    public   readonly   int         structIndex;    //  4
    public   readonly   bool        blittable;      //  4
    public   readonly   int         byteSize;       //  4
    
    internal abstract   StructHeap  CreateHeap();
    internal abstract   bool        AddEntityComponent     (Entity entity);
    internal abstract   bool        AddEntityComponentValue(Entity entity, object value);
    
    protected ComponentType(string componentKey, int structIndex, Type type, int byteSize)
        : base (componentKey, type, Component)
    {
        this.structIndex    = structIndex;
        blittable           = IsBlittableType(type);
        this.byteSize       = byteSize;
    }
}

internal sealed class ComponentType<T> : ComponentType 
    where T : struct, IComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"component: '{componentKey}' [{typeof(T).Name}]";

    internal ComponentType(string componentKey, int structIndex, TypeMapper<T> typeMapper)
        : base(componentKey, structIndex, typeof(T), ByteSize)
    {
        this.typeMapper = typeMapper;
    }
    
    internal override bool AddEntityComponent(Entity entity) {
        int archIndex = 0;
        return EntityStoreBase.AddComponent<T>(entity.Id, structIndex, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, default);
    }
    
    internal override bool AddEntityComponentValue(Entity entity, object value) {
        int archIndex = 0;
        var componentValue = (T)value;
        return EntityStoreBase.AddComponent(entity.Id, structIndex, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, componentValue);
    }
    
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(structIndex, typeMapper);
    }
    
    /// <summary>
    /// The returned padding enables using <see cref="Vector128"/>, <see cref="Vector256"/> and Vector512 (512 bits = 64 bytes) operations <br/>
    /// on <see cref="StructHeap{T}"/>.<see cref="StructHeap{T}.components"/>
    /// without the need of an additional for loop to process the elements at the end of a <see cref="Span{T}"/>.
    /// </summary>
    internal static int PadCount512     => 64 / ByteSize - 1;
    
    /// <summary> 256 bits = 32 bytes </summary>
    internal static int PadCount256     => 32 / ByteSize - 1;
    
    /// <summary> 128 bits = 16 bytes </summary>
    internal static int PadCount128     => 16 / ByteSize - 1;
    
    private static  int ByteSize        => GetByteSize();
    
    private static  int GetByteSize()   => Unsafe.SizeOf<T>();
}
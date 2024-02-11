// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Engine.ECS.SchemaTypeKind;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide meta data for an <see cref="IComponent"/> struct.
/// </summary>
public abstract class ComponentType : SchemaType
{
    /// <summary> The index in <see cref="EntitySchema"/>.<see cref="EntitySchema.Components"/>. </summary>
    public   readonly   int         StructIndex;    //  4
    /// <summary> Return true if <see cref="IComponent"/>'s of this type can be copied. </summary>
    public   readonly   bool        IsBlittable;    //  4
    /// <summary> The size in bytes of the <see cref="IComponent"/> struct. </summary>
    public   readonly   int         StructSize;     //  4
    
    internal abstract   StructHeap  CreateHeap();
    internal abstract   bool        RemoveEntityComponent  (Entity entity);
    internal abstract   bool        AddEntityComponent     (Entity entity);
    internal abstract   bool        AddEntityComponentValue(Entity entity, object value);
    
    internal abstract   ComponentCommands  CreateComponentCommands();
    
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
    
    internal override bool AddEntityComponent(Entity entity) {
        int archIndex = 0;
        return EntityStoreBase.AddComponent<T>(entity.Id, StructIndex, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, default);
    }
    
    internal override bool AddEntityComponentValue(Entity entity, object value) {
        int archIndex = 0;
        var componentValue = (T)value;
        return EntityStoreBase.AddComponent(entity.Id, StructIndex, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, componentValue);
    }
    
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(StructIndex, typeMapper);
    }
    
    internal override ComponentCommands CreateComponentCommands()
    {
        return new ComponentCommands<T>(StructIndex) {
            componentCommands = new ComponentCommand<T>[8]
        };
    }
    
    private static              int GetByteSize()   => Unsafe.SizeOf<T>();

    // ReSharper disable StaticMemberInGenericType
    internal static readonly    int ByteSize        = GetByteSize();

    /// <summary>
    /// The returned padding enables using Vector128, Vector256 and Vector512 (512 bits = 64 bytes) operations <br/>
    /// on <see cref="StructHeap{T}"/>.<see cref="StructHeap{T}.components"/>
    /// without the need of an additional for loop to process the elements at the end of a <see cref="Span{T}"/>.
    /// </summary>
    internal static readonly    int PadCount512     = 64 / ByteSize - 1;
    
    /// <summary> 256 bits = 32 bytes </summary>
    internal static readonly    int PadCount256     = 32 / ByteSize - 1;
    
    /// <summary> 128 bits = 16 bytes </summary>
    internal static readonly    int PadCount128     = 16 / ByteSize - 1;
    
    /// <summary>
    /// Return the number of components in a <see cref="Chunk{T}"/> as a multiple of 64 bytes.
    /// </summary>
    /// <remarks>
    /// This enables providing <see cref="Chunk{T}"/> components as <see cref="Span{T}"/> of Vector128, Vector256 and Vector512
    /// of https://learn.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.<br/>
    /// See: <see cref="Chunk{T}.AsSpan128{TTo}"/>, <see cref="Chunk{T}.AsSpan256{TTo}"/> and <see cref="Chunk{T}.AsSpan512{TTo}"/>.<br/>
    /// <br/>
    /// It also enables to apply vectorization without a remainder loop.<br/>
    /// <br/>
    /// Aligned naming to Vector512. 512 bits == 64 bytes.
    /// </remarks>
    internal static readonly    int Align512        = GetAlign512();
    
    private static int GetAlign512()
    {
        switch (ByteSize) {
            case  1:    return 64;
            case  2:    return 32;
            case  4:    return 16;
            case  8:    return  8;
            case 16:    return  4;
            case 32:    return  2;
            case 64:    return  1;
            default:    return  0;
        }
    }
}

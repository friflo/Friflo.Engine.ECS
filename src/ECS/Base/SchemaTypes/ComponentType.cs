// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
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
    
    internal abstract   StructHeap  CreateHeap();
    internal abstract   bool        AddEntityComponent(Entity entity);
    
    protected ComponentType(string componentKey, int structIndex, Type type)
        : base (componentKey, type, Component)
    {
        this.structIndex    = structIndex;
        blittable           = IsBlittableType(type);
    }
}

internal sealed class ComponentType<T> : ComponentType 
    where T : struct, IComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"component: '{componentKey}' [{typeof(T).Name}]";

    internal ComponentType(string componentKey, int structIndex, TypeMapper<T> typeMapper)
        : base(componentKey, structIndex, typeof(T))
    {
        this.typeMapper = typeMapper;
    }
    
    internal override bool AddEntityComponent(Entity entity) {
        int archIndex = 0;
        return EntityStoreBase.AddComponent<T>(entity.id, structIndex, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, default);
    }
    
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(structIndex, typeMapper);
    }
}
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Fliox.Engine.ECS.SchemaTypeKind;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class ComponentType : SchemaType
{
    protected ComponentType(string componentKey, int structIndex, Type type)
        : base (componentKey, null, type, Component, 0, structIndex)
    { }
}

internal sealed class ComponentType<T> : ComponentType 
    where T : struct, IComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"component: '{componentKey}' [{typeof(T).Name}]";

    internal ComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, structIndex, typeof(T))
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(structIndex, typeMapper);
    }
}
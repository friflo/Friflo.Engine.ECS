// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class ComponentFactory
{
    internal readonly   string  componentKey;
    internal readonly   int     structIndex;
    internal readonly   long    structHash;
    internal readonly   bool    isStructFactory;
        
    internal abstract   StructHeap  CreateHeap          (int capacity);
    internal abstract   void        ReadClassComponent  (ObjectReader reader, JsonValue json, GameEntity entity);
    
    internal ComponentFactory(string componentKey, bool isStructFactory, int structIndex, long structHash) {
        this.componentKey       = componentKey;
        this.structIndex        = structIndex;
        this.structHash         = structHash;
        this.isStructFactory    = isStructFactory;
    }
}

internal sealed class StructFactory<T> : ComponentFactory 
    where T : struct
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"StructFactory: {typeof(T).Name}";

    internal StructFactory(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, true, structIndex, typeof(T).Handle())
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   void    ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException("operates only on ClassFactory<>");
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(structIndex, componentKey, capacity, typeMapper);   
    }
}

internal sealed class ClassFactory<T> : ComponentFactory 
    where T : ClassComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"ClassFactory: {typeof(T).Name}";
    
    internal ClassFactory(string componentKey, TypeStore typeStore)
        : base(componentKey, false, -1, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   StructHeap  CreateHeap(int capacity)
        => throw new InvalidOperationException("operates only on StructFactory<>");
    
    internal override void ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity) {
        var classComponent = entity.GetClassComponent<T>();
        if (classComponent != null) { 
            reader.ReadToMapper(typeMapper, json, classComponent, true);
            return;
        }
        classComponent = reader.ReadMapper(typeMapper, json);
        entity.AppendClassComponent(classComponent);
    }
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class ComponentType
{
    public   readonly   string  componentKey;
    public   readonly   int     index;
    public   readonly   bool    isStructType;
    public   readonly   long    structHash;
    public   readonly   Type    type;
        
    internal abstract   StructHeap  CreateHeap          (int capacity);
    internal abstract   void        ReadClassComponent  (ObjectReader reader, JsonValue json, GameEntity entity);
    
    internal ComponentType(string componentKey, Type type, bool isStructType, int index, long structHash) {
        this.componentKey   = componentKey;
        this.index          = index;
        this.structHash     = structHash;
        this.isStructType   = isStructType;
        this.type           = type;
    }
}

internal sealed class StructComponentType<T> : ComponentType 
    where T : struct
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"StructFactory: {typeof(T).Name}";

    internal StructComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, typeof(T), true, structIndex, typeof(T).Handle())
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   void    ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException("operates only on ClassFactory<>");
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(index, componentKey, capacity, typeMapper);   
    }
}

internal sealed class ClassComponentType<T> : ComponentType 
    where T : ClassComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"ClassFactory: {typeof(T).Name}";
    
    internal ClassComponentType(string componentKey, int classIndex, TypeStore typeStore)
        : base(componentKey, typeof(T), false, classIndex, 0)
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

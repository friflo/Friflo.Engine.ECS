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
    /// <summary>
    /// If <see cref="isStructType"/> == true  the key assigned in <see cref="StructComponentAttribute"/><br/>
    /// If <see cref="isStructType"/> == false the key assigned in <see cref="ClassComponentAttribute"/>
    /// </summary>
    public   readonly   string  componentKey;
    /// <summary>
    /// If <see cref="isStructType"/> == true  the index in <see cref="ComponentTypes.Structs"/><br/>
    /// If <see cref="isStructType"/> == false the index in <see cref="ComponentTypes.Classes"/>
    /// </summary>
    public   readonly   int     index;
    /// <summary>
    /// true if the type is a struct component<br/>
    /// false if the type is a struct component<br/>
    /// </summary>
    public   readonly   bool    isStructType;
    
    public   readonly   long    structHash;
    /// <summary>
    /// If <see cref="isStructType"/> == true  the type of a struct component attributed with <see cref="StructComponentAttribute"/><br/>
    /// If <see cref="isStructType"/> == false the type of a class  component attributed with <see cref="ClassComponentAttribute"/>
    /// </summary>
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

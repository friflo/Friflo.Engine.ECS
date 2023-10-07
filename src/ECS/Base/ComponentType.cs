// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Fliox.Engine.ECS.ComponentKind;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class ComponentType
{
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/> the key assigned in <see cref="StructComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Class"/>  the key assigned in <see cref="ClassComponentAttribute"/>
    /// </summary>
    public   readonly   string          componentKey;
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/> the index in <see cref="ComponentSchema.Structs"/><br/>
    /// If <see cref="kind"/> == <see cref="Class"/>  the index in <see cref="ComponentSchema.Classes"/>
    /// </summary>
    public   readonly   int             index;
    /// <returns>
    /// <see cref="Struct"/> if the type is a struct component<br/>
    /// <see cref="Class"/>  if the type is a class component<br/>
    /// </returns>
    public   readonly   ComponentKind   kind;
    
    public   readonly   long            structHash;
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/>  the type of a struct component attributed with <see cref="StructComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Class"/> the type of a class  component attributed with <see cref="ClassComponentAttribute"/>
    /// </summary>
    public   readonly   Type            type;
        
    internal abstract   StructHeap  CreateHeap          (int capacity);
    internal abstract   void        ReadClassComponent  (ObjectReader reader, JsonValue json, GameEntity entity);
    
    internal ComponentType(string componentKey, Type type, ComponentKind kind, int index, long structHash) {
        this.componentKey   = componentKey;
        this.index          = index;
        this.structHash     = structHash;
        this.kind           = kind;
        this.type           = type;
    }
}

internal sealed class StructComponentType<T> : ComponentType 
    where T : struct
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"struct component: {typeof(T).Name}";

    internal StructComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, typeof(T), Struct, structIndex, typeof(T).Handle())
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
    public  override    string          ToString() => $"class component: *{typeof(T).Name}";
    
    internal ClassComponentType(string componentKey, int classIndex, TypeStore typeStore)
        : base(componentKey, typeof(T), Class, classIndex, 0)
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

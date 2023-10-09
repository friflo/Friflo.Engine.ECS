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
    /// If <see cref="kind"/> == <see cref="Class"/> the index in <see cref="ComponentSchema.Classes"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             classIndex;
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/> the index in <see cref="ComponentSchema.Structs"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             structIndex;
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Tag"/> the index in <see cref="ComponentSchema.Tags"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             tagIndex;
    /// <returns>
    /// <see cref="Class"/> if the type is a <see cref="ClassComponent"/><br/>
    /// <see cref="Struct"/> if the type is a <see cref="IStructComponent"/><br/>
    /// <see cref="Tag"/> if the type is an <see cref="IEntityTag"/><br/>
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
    
    internal ComponentType(
        string          componentKey,
        Type            type,
        ComponentKind   kind,
        int             classIndex,
        int             structIndex,
        int             tagIndex,
        long            structHash)
    {
        this.componentKey   = componentKey;
        this.classIndex     = classIndex;
        this.structIndex    = structIndex;
        this.tagIndex       = tagIndex;
        this.structHash     = structHash;
        this.kind           = kind;
        this.type           = type;
    }
}

internal sealed class StructComponentType<T> : ComponentType 
    where T : struct, IStructComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"struct component: [{typeof(T).Name}]";

    internal StructComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, typeof(T), Struct, 0, structIndex, 0, typeof(T).Handle())
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   void    ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException("operates only on ClassFactory<>");
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(structIndex, componentKey, capacity, typeMapper);   
    }
}

internal sealed class ClassComponentType<T> : ComponentType 
    where T : ClassComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"class component: [*{typeof(T).Name}]";
    
    internal ClassComponentType(string componentKey, int classIndex, TypeStore typeStore)
        : base(componentKey, typeof(T), Class, classIndex, 0, 0, 0)
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

internal sealed class TagType : ComponentType 
{
    public  override    string  ToString() => $"tag: [#{type.Name}]";
    
    internal TagType(Type type, int tagIndex)
        : base(null, type, Tag, 0, 0, tagIndex, 0)
    { }
    
    internal override   StructHeap  CreateHeap(int capacity)
        => throw new InvalidOperationException("operates only on StructFactory<>");
    
    internal override void ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException("operates only on ClassFactory<>");
}

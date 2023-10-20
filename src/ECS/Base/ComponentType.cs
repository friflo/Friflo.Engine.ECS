// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
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
    public   readonly   string          componentKey;   //  8
    
    public   readonly   string          tagName;        //  8
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Class"/> the index in <see cref="ComponentSchema.Classes"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             classIndex;     //  4
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/> the index in <see cref="ComponentSchema.Structs"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             structIndex;    //  4
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Tag"/> the index in <see cref="ComponentSchema.Tags"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             tagIndex;       //  4
    /// <returns>
    /// <see cref="Class"/> if the type is a <see cref="ClassComponent"/><br/>
    /// <see cref="Struct"/> if the type is a <see cref="IStructComponent"/><br/>
    /// <see cref="Tag"/> if the type is an <see cref="IEntityTag"/><br/>
    /// </returns>
    public   readonly   ComponentKind   kind;           //  4
    
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/>  the type of a struct component attributed with <see cref="StructComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Class"/> the type of a class  component attributed with <see cref="ClassComponentAttribute"/>
    /// </summary>
    public   readonly   Type            type;           //  8
    
    
    internal readonly   Bytes           componentKeyBytes;
        
    internal virtual    StructHeap  CreateHeap          (int chunkSize)
        => throw new InvalidOperationException("operates only on StructComponentType<>");
    
    internal virtual    void        ReadClassComponent  (ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException("operates only on ClassComponentType<>");
    
    internal ComponentType(
        string          componentKey,
        string          tagName,
        Type            type,
        ComponentKind   kind,
        int             classIndex,
        int             structIndex,
        int             tagIndex)
    {
        this.componentKey   = componentKey;
        this.tagName        = tagName;
        this.classIndex     = classIndex;
        this.structIndex    = structIndex;
        this.tagIndex       = tagIndex;
        this.kind           = kind;
        this.type           = type;
        if (this.componentKey != null) {
            componentKeyBytes = new Bytes(componentKey);   
        }
    }
}

internal sealed class StructComponentType<T> : ComponentType 
    where T : struct, IStructComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"struct component: [{typeof(T).Name}]";

    internal StructComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, null, typeof(T), Struct, 0, structIndex, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    internal override StructHeap CreateHeap(int chunkSize) {
        return new StructHeap<T>(structIndex, chunkSize, typeMapper);   
    }
}

internal sealed class ClassComponentType<T> : ComponentType 
    where T : ClassComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"class component: [*{typeof(T).Name}]";
    
    internal ClassComponentType(string componentKey, int classIndex, TypeStore typeStore)
        : base(componentKey, null, typeof(T), Class, classIndex, 0, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override void ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity) {
        var classComponent = entity.GetClassComponent<T>();
        if (classComponent != null) { 
            reader.ReadToMapper(typeMapper, json, classComponent, true);
            return;
        }
        classComponent = reader.ReadMapper(typeMapper, json);
        GameEntityUtils.AppendClassComponent(entity, classComponent);
    }
}

internal sealed class TagType : ComponentType 
{
    public  override    string  ToString() => $"tag: [#{type.Name}]";
    
    internal TagType(Type type, int tagIndex)
        : base(null, type.Name, type, Tag, 0, 0, tagIndex)
    { }
}

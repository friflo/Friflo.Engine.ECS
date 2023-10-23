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
    /// If <see cref="kind"/> == <see cref="Struct"/> the key assigned in <see cref="ComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Behavior"/>  the key assigned in <see cref="BehaviorAttribute"/>
    /// </summary>
    public   readonly   string          componentKey;   //  8
    
    public   readonly   string          tagName;        //  8
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Behavior"/> the index in <see cref="ComponentSchema.Classes"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             behaviorIndex;  //  4
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/> the index in <see cref="ComponentSchema.Structs"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             structIndex;    //  4
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Tag"/> the index in <see cref="ComponentSchema.Tags"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             tagIndex;       //  4
    /// <returns>
    /// <see cref="Behavior"/> if the type is a <see cref="Behavior"/><br/>
    /// <see cref="Struct"/> if the type is a <see cref="IComponent"/><br/>
    /// <see cref="Tag"/> if the type is an <see cref="IEntityTag"/><br/>
    /// </returns>
    public   readonly   ComponentKind   kind;           //  4
    
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Struct"/>  the type of a struct component attributed with <see cref="ComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Behavior"/> the type of a class  component attributed with <see cref="BehaviorAttribute"/>
    /// </summary>
    public   readonly   Type            type;           //  8
    
    
    internal readonly   Bytes           componentKeyBytes;
        
    internal virtual    StructHeap  CreateHeap          ()
        => throw new InvalidOperationException("operates only on StructComponentType<>");
    
    internal virtual    void        ReadBehavior  (ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException($"operates only on BehaviorType<>");
    
    internal ComponentType(
        string          componentKey,
        string          tagName,
        Type            type,
        ComponentKind   kind,
        int             behaviorIndex,
        int             structIndex,
        int             tagIndex)
    {
        this.componentKey   = componentKey;
        this.tagName        = tagName;
        this.behaviorIndex  = behaviorIndex;
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
    where T : struct, IComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"struct component: [{typeof(T).Name}]";

    internal StructComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, null, typeof(T), Struct, 0, structIndex, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(structIndex, typeMapper);
    }
}

internal sealed class BehaviorType<T> : ComponentType 
    where T : Behavior
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"class component: [*{typeof(T).Name}]";
    
    internal BehaviorType(string behaviorKey, int behaviorIndex, TypeStore typeStore)
        : base(behaviorKey, null, typeof(T), ComponentKind.Behavior, behaviorIndex, 0, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override void ReadBehavior(ObjectReader reader, JsonValue json, GameEntity entity) {
        var behavior = entity.GetBehavior<T>();
        if (behavior != null) { 
            reader.ReadToMapper(typeMapper, json, behavior, true);
            return;
        }
        behavior = reader.ReadMapper(typeMapper, json);
        GameEntityUtils.AppendBehavior(entity, behavior);
    }
}

internal sealed class TagType : ComponentType 
{
    public  override    string  ToString() => $"tag: [#{type.Name}]";
    
    internal TagType(Type type, int tagIndex)
        : base(null, type.Name, type, Tag, 0, 0, tagIndex)
    { }
}

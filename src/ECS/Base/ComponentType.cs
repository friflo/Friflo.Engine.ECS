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
    /// If <see cref="kind"/> == <see cref="Component"/> the key assigned in <see cref="ComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Script"/>  the key assigned in <see cref="ScriptAttribute"/>
    /// </summary>
    public   readonly   string          componentKey;   //  8
    
    public   readonly   string          tagName;        //  8
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Script"/> the index in <see cref="ComponentSchema.Scripts"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             behaviorIndex;  //  4
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Component"/> the index in <see cref="ComponentSchema.Components"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             structIndex;    //  4
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Tag"/> the index in <see cref="ComponentSchema.Tags"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             tagIndex;       //  4
    /// <returns>
    /// <see cref="Script"/> if the type is a <see cref="Script"/><br/>
    /// <see cref="Component"/> if the type is a <see cref="IComponent"/><br/>
    /// <see cref="Tag"/> if the type is an <see cref="IEntityTag"/><br/>
    /// </returns>
    public   readonly   ComponentKind   kind;           //  4
    
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Component"/> the type of a component attributed with <see cref="ComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Script"/> the type of a behavior attributed with <see cref="ScriptAttribute"/>
    /// </summary>
    public   readonly   Type            type;           //  8
    
    
    internal readonly   Bytes           componentKeyBytes;
        
    internal virtual    StructHeap  CreateHeap          ()
        => throw new InvalidOperationException("operates only on StructComponentType<>");
    
    internal virtual    void        ReadScript  (ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException($"operates only on ScriptType<>");
    
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
    public  override    string          ToString() => $"component: '{componentKey}' [{typeof(T).Name}]";

    internal StructComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, null, typeof(T), Component, 0, structIndex, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(structIndex, typeMapper);
    }
}

internal sealed class ScriptType<T> : ComponentType 
    where T : Script
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"behavior: '{componentKey}' [*{typeof(T).Name}]";
    
    internal ScriptType(string behaviorKey, int behaviorIndex, TypeStore typeStore)
        : base(behaviorKey, null, typeof(T), ComponentKind.Script, behaviorIndex, 0, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override void ReadScript(ObjectReader reader, JsonValue json, GameEntity entity) {
        var behavior = entity.GetScript<T>();
        if (behavior != null) { 
            reader.ReadToMapper(typeMapper, json, behavior, true);
            return;
        }
        behavior = reader.ReadMapper(typeMapper, json);
        entity.archetype.gameEntityStore.AppendScript(entity, behavior);
    }
}

internal sealed class TagType : ComponentType 
{
    public  override    string  ToString() => $"tag: [#{type.Name}]";
    
    internal TagType(Type type, int tagIndex)
        : base(null, type.Name, type, Tag, 0, 0, tagIndex)
    { }
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Fliox.Engine.ECS.SchemaTypeKind;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class SchemaType
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
    public   readonly   int             scriptIndex;  //  4
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
    public   readonly   SchemaTypeKind  kind;           //  4
    
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Component"/> the type of a component attributed with <see cref="ComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Script"/> the type of a script attributed with <see cref="ScriptAttribute"/>
    /// </summary>
    public   readonly   Type            type;           //  8
    
    
    internal readonly   Bytes           componentKeyBytes;
        
    internal virtual    StructHeap  CreateHeap          ()
        => throw new InvalidOperationException("operates only on StructComponentType<>");
    
    internal virtual    void        ReadScript  (ObjectReader reader, JsonValue json, Entity entity)
        => throw new InvalidOperationException($"operates only on ScriptType<>");
    
    internal SchemaType(
        string          componentKey,
        string          tagName,
        Type            type,
        SchemaTypeKind  kind,
        int             scriptIndex,
        int             structIndex,
        int             tagIndex)
    {
        this.componentKey   = componentKey;
        this.tagName        = tagName;
        this.scriptIndex  = scriptIndex;
        this.structIndex    = structIndex;
        this.tagIndex       = tagIndex;
        this.kind           = kind;
        this.type           = type;
        if (this.componentKey != null) {
            componentKeyBytes = new Bytes(componentKey);   
        }
    }
}

public abstract class ComponentType : SchemaType
{
    protected ComponentType(string componentKey, int structIndex, Type type)
        : base (componentKey, null, type, Component, 0, structIndex, 0)
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

public abstract class ScriptType : SchemaType
{
    protected ScriptType(string scriptKey, int scriptIndex, Type type)
        : base (scriptKey, null, type, SchemaTypeKind.Script, scriptIndex, 0, 0)
    { }
}

internal sealed class ScriptType<T> : ScriptType 
    where T : Script
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"script: '{componentKey}' [*{typeof(T).Name}]";
    
    internal ScriptType(string scriptKey, int scriptIndex, TypeStore typeStore)
        : base(scriptKey, scriptIndex, typeof(T))
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override void ReadScript(ObjectReader reader, JsonValue json, Entity entity) {
        var script = entity.GetScript<T>();
        if (script != null) { 
            reader.ReadToMapper(typeMapper, json, script, true);
            return;
        }
        script = reader.ReadMapper(typeMapper, json);
        entity.archetype.entityStore.AppendScript(entity, script);
    }
}

public sealed class TagType : SchemaType 
{
    public  override    string  ToString() => $"tag: [#{type.Name}]";
    
    internal TagType(Type type, int tagIndex)
        : base(null, type.Name, type, Tag, 0, 0, tagIndex)
    { }
}

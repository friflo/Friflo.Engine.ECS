// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
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
    /// If <see cref="kind"/> == <see cref="Script"/> the index in <see cref="EntitySchema.Scripts"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             scriptIndex;  //  4
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Component"/> the index in <see cref="EntitySchema.Components"/>. Otherwise 0<br/>
    /// </summary>
    public   readonly   int             structIndex;    //  4
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
        int             structIndex)
    {
        this.componentKey   = componentKey;
        this.tagName        = tagName;
        this.scriptIndex    = scriptIndex;
        this.structIndex    = structIndex;
        this.kind           = kind;
        this.type           = type;
        if (this.componentKey != null) {
            componentKeyBytes = new Bytes(componentKey);   
        }
    }
}

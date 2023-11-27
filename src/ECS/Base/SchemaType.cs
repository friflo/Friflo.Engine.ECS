// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Burst;
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
        
    internal SchemaType(
        string          componentKey,
        Type            type,
        SchemaTypeKind  kind)
    {
        this.componentKey   = componentKey;
        this.kind           = kind;
        this.type           = type;
        if (this.componentKey != null) {
            componentKeyBytes = new Bytes(componentKey);   
        }
    }
}

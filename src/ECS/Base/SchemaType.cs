// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
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
    /// <see cref="Tag"/> if the type is an <see cref="ITag"/><br/>
    /// </returns>
    public   readonly   SchemaTypeKind  kind;           //  4
    
    /// <summary>
    /// If <see cref="kind"/> == <see cref="Component"/> the type of a component attributed with <see cref="ComponentAttribute"/><br/>
    /// If <see cref="kind"/> == <see cref="Script"/> the type of a script attributed with <see cref="ScriptAttribute"/>
    /// </summary>
    public   readonly   Type            type;           //  8
    
    public   readonly   string          name;           //  8
    
    
    internal readonly   Bytes           componentKeyBytes;
        
    internal SchemaType(string componentKey, Type type, SchemaTypeKind kind)
    {
        this.componentKey   = componentKey;
        this.kind           = kind;
        this.type           = type;
        name                = type.Name;
        if (componentKey != null) {
            componentKeyBytes = new Bytes(componentKey);   
        }
    }
    
    private static readonly Dictionary<Type, bool> BlittableTypes = new Dictionary<Type, bool>();
    
    static SchemaType()
    {
        var types = BlittableTypes;
        types.Add(typeof(bool),         true);
        types.Add(typeof(char),         true);
        types.Add(typeof(decimal),      true);
        //
        types.Add(typeof(byte),         true);
        types.Add(typeof(short),        true);
        types.Add(typeof(int),          true);
        types.Add(typeof(long),         true);
        //
        types.Add(typeof(sbyte),        true);
        types.Add(typeof(ushort),       true);
        types.Add(typeof(uint),         true);
        types.Add(typeof(ulong),        true);
        //
        types.Add(typeof(float),        true);
        types.Add(typeof(double),       true);
        //
        types.Add(typeof(Guid),         true);
        types.Add(typeof(DateTime),     true);
        types.Add(typeof(BigInteger),   true);
        //
        types.Add(typeof(JsonValue),    true);
        types.Add(typeof(Entity),       true);
        //
        types.Add(typeof(string),       true);
    }
    
    // todo - add test assertion EntityName is a blittable type 
    internal static bool IsBlittableType(Type type)
    {
        if (BlittableTypes.TryGetValue(type, out bool blittable)) {
            return blittable;
        }
        if (type.IsArray) {
            blittable = false;    
        } else if (type.IsClass || type.IsValueType) {
            blittable = AreAllMembersBlittable(type);
        }
        BlittableTypes.Add(type, blittable);
        return blittable;
    }
    
    private const BindingFlags MemberFlags =
        BindingFlags.Public             |
        BindingFlags.NonPublic          |
        BindingFlags.Instance           |
        BindingFlags.FlattenHierarchy;

    private static bool AreAllMembersBlittable(Type type)
    {
        var members = type.GetMembers(MemberFlags);
        foreach (var member in members)
        {
            switch (member) {
                case FieldInfo fieldInfo:
                    var fieldType = fieldInfo.FieldType;
                    if (IsBlittableType(fieldType)) {
                        continue;
                    }
                    return false;
                case PropertyInfo: // propertyInfo:
                    continue;
                /*  var propertyType = propertyInfo.PropertyType;
                    if (IsBlittableType(propertyType)) {
                        continue;
                    }
                    return false; */
            }
        }
        return true;
    }
}

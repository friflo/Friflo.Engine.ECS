// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using static Friflo.Engine.ECS.SchemaTypeKind;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide meta data for <see cref="Script"/> classes and <see cref="IComponent"/> / <see cref="ITag"/> structs. 
/// </summary>
public abstract class SchemaType
{
    /// <summary>
    /// If <see cref="Kind"/> is a <see cref="Component"/> or a <see cref="Script"/>
    /// the key assigned with <see cref="ComponentKeyAttribute"/>.
    /// </summary>
    public   readonly   string          ComponentKey;       //  8
    
    /// <summary>Returns the <see cref="SchemaTypeKind"/> of the type.</summary>
    /// <returns>
    /// <see cref="Script"/> if the type is a <see cref="Script"/><br/>
    /// <see cref="Component"/> if the type is a <see cref="IComponent"/><br/>
    /// <see cref="Tag"/> if the type is an <see cref="ITag"/><br/>
    /// </returns>
    public   readonly   SchemaTypeKind  Kind;               //  1
    
    /// <summary>
    /// If <see cref="Kind"/> == <see cref="Tag"/> the type of a <b>tag</b> struct implementing <see cref="ITag"/>.<br/>
    /// If <see cref="Kind"/> == <see cref="Component"/> the type of a <b>component</b> struct implementing <see cref="IComponent"/>.<br/>
    /// If <see cref="Kind"/> == <see cref="Script"/> the type of a <b>script</b> class extending <see cref="Script"/>.<br/>
    /// </summary>
    public   readonly   Type            Type;               //  8
    
    /// <summary>Returns the <see cref="System.Type"/> name of the struct / class. </summary>
    public   readonly   string          Name;               //  8
    
    internal readonly   Bytes           componentKeyBytes;  // 16
        
    internal SchemaType(string componentKey, Type type, SchemaTypeKind kind)
    {
        ComponentKey    = componentKey;
        Kind            = kind;
        Type            = type;
        Name            = type.Name;
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

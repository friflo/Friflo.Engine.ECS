// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal readonly struct ComponentFieldInfoKey : IEquatable<ComponentFieldInfoKey>
{
    internal readonly   int     structIndex;
    private  readonly   string  path;

    public   override   string  ToString() => GetString();

    public bool Equals(ComponentFieldInfoKey other) {
        return structIndex == other.structIndex && path == other.path;
    }

    public override int GetHashCode() {
        return structIndex ^ path.GetHashCode();
    }

    internal ComponentFieldInfoKey(int structIndex, string  path) {
        this.structIndex    = structIndex;
        this.path           = path;
    }
    
    internal string GetString() {
        return path;
    }
}

public delegate TField MemberGetter<in TComponent, out TField> (TComponent component);
public delegate void   MemberSetter<TComponent, in  TField> (ref TComponent component, TField value);

public sealed class ComponentFieldInfo 
{
    public   readonly   Type                                type;
    
    public   readonly   ComponentType                       componentType;
    
    public   readonly   IEnumerable<CustomAttributeData>    customAttributes;
    
    internal readonly   ComponentFieldInfoKey               infoKey;
    
    public   readonly   string                              path;
    
    /// Type: <see cref="MemberGetter{T, TField}"/>
    public   readonly   object                              getter;
    
    /// Type: <see cref="MemberSetter{T, TField}"/>. Is null if not writeable
    public   readonly   object                              setter;

    
    private static readonly Dictionary<ComponentFieldInfoKey, ComponentFieldInfo> Map = new();

    public override     string          ToString() => infoKey.GetString();

    private ComponentFieldInfo(ComponentType componentType, string path, ComponentFieldInfoKey key, Type type,  IEnumerable<CustomAttributeData> customAttributes, object getter, object setter) {
        this.componentType      = componentType;        
        this.path               = path;
        infoKey                 = key;
        this.type               = type;
        this.customAttributes   = customAttributes;
        this.getter             = getter;
        this.setter             = setter;
    }
    
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty;
    
    /// Calls <see cref="CreateGetter{TComponent,TField}"/> and <see cref="CreateSetter{TComponent,TField}"/> 
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2080", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    public static ComponentFieldInfo Get(ComponentType componentType, string path)
    {
        var key = new ComponentFieldInfoKey(componentType.StructIndex, path);
        if (Map.TryGetValue(key, out var componentFieldInfo)) {
            return componentFieldInfo;
        }
        var pathItems   = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var memberInfos = new MemberInfo[pathItems.Length];
        var type        = componentType.Type;
        bool canWrite   = true;
        IEnumerable<CustomAttributeData> customAttributes = null;
        for (int i = 0; i < pathItems.Length; i++)
        {
            pathItems[i]    = pathItems[i].Trim();
            if (pathItems[i] == "childIds" || pathItems[i] == "ChildIds") {
                int x =111;
            }
            var members = type.GetMember(pathItems[i], Flags);
            var memberInfo  = members[0];
            memberInfos[i] = memberInfo;
            customAttributes = memberInfo.CustomAttributes;
            if (memberInfo is FieldInfo fieldInfo) {
                type = fieldInfo.FieldType;
                if (fieldInfo.IsInitOnly) {
                    canWrite = false;
                }
            } else if (memberInfo is PropertyInfo propertyInfo) {
                type = propertyInfo.PropertyType;
                if (!propertyInfo.CanWrite) {
                    canWrite = false;
                }
            } else {
                throw new InvalidOperationException();
            }
        }
        var typeParams      = new []{ componentType.Type, type };
        
        var getterMethod    = typeof(ComponentFieldInfo).GetMethod("CreateGetter", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(MemberInfo[])], null)!;
        var genericGetter   = getterMethod.MakeGenericMethod(typeParams);
        var getter          = genericGetter.Invoke(null, [memberInfos]);
        
        object setter = null;
        if (canWrite) {
            var setterMethod    = typeof(ComponentFieldInfo).GetMethod("CreateSetter", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(MemberInfo[])], null)!;
            var genericSetter   = setterMethod.MakeGenericMethod(typeParams);
            setter              = genericSetter.Invoke(null, [memberInfos]);
        }
        path = string.Join('.', pathItems);
        var info = new ComponentFieldInfo(componentType, path, key, type, customAttributes, getter, setter);
        Map.Add(key, info);
        return info;
    }
    
    // ReSharper disable UnusedMember.Local
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    private static MemberGetter<TComponent,TField> CreateGetter<TComponent,TField>(MemberInfo[] fields)
    {
        var arg = Expression.Parameter(typeof(TComponent), "component"); // "component" parameter name in MemberGetter<,>
        Expression fieldExpr = arg;
        foreach (var field in fields) {
            fieldExpr = PropertyOrField(fieldExpr, field);
        }
        return Expression.Lambda<MemberGetter<TComponent, TField>>(fieldExpr, arg).Compile();
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    private static MemberSetter<TComponent,TField> CreateSetter<TComponent,TField>(MemberInfo[] fields)
    {
        var arg   = Expression.Parameter(typeof(TComponent).MakeByRefType(), "component"); // "component" parameter name in MemberSetter<,>
        var value = Expression.Parameter(typeof(TField),                     "value");     // "value" parameter name in MemberSetter<,>
        Expression fieldExpr = arg;
        foreach (var field in fields) {
            fieldExpr = PropertyOrField(fieldExpr, field);
        }
        var assign = Expression.Assign(fieldExpr, value);
        return Expression.Lambda<MemberSetter<TComponent, TField>>(assign, arg, value).Compile();
    }
    
    /// <summary>
    /// <see cref="Expression.PropertyOrField"/> does not search for exact names.
    /// E.g. in case of two fields: bla & Bla it prefers the public version.
    /// </summary>
    private static MemberExpression PropertyOrField(Expression expression, MemberInfo memberInfo)
    {
        if (memberInfo is FieldInfo fieldInfo) {
            return Expression.Field(expression, fieldInfo);
        }
        if (memberInfo is PropertyInfo propertyInfo) {
            return Expression.Property(expression, propertyInfo);
        }
        throw new InvalidOperationException("expect field or property");
    }
}

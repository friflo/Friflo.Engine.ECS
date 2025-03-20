// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal readonly struct MemberPathKey : IEquatable<MemberPathKey>
{
    internal readonly   Type    type;
    internal readonly   string  path;

    public   override   string  ToString() => $"{type.Name} {path}";

    public bool Equals(MemberPathKey other) {
        return type == other.type && path == other.path;
    }

    public override int GetHashCode() {
        return type.GetHashCode() ^ path.GetHashCode();
    }

    internal MemberPathKey(Type type, string  path) {
        this.type    = type;
        this.path    = path;
    }
}

public delegate TField MemberPathGetter<in T, out TField> (T root);
public delegate void   MemberPathSetter<T, in  TField> (ref T root, TField value);


public sealed class MemberPath 
{
    private  readonly   MemberPathKey                       key;
    
    internal readonly   int                                 structIndex;

    public   readonly   Type                                memberType;
    
    // ReSharper disable once InconsistentNaming
    public              Type                                declarationType => key.type;
    
    public   readonly   IEnumerable<CustomAttributeData>    customAttributes;
    
    // ReSharper disable once InconsistentNaming
    public              string                              path => key.path;
    
    /// Type: <see cref="MemberPathGetter{T,TField}"/>
    public   readonly   object                              getter;
    
    /// Type: <see cref="MemberPathSetter{T,TField}"/>. Is null if not writeable
    public   readonly   object                              setter;

    public override     string                              ToString() => $"{declarationType.Name} {path} : {memberType.Name}";
    
    private static readonly Dictionary<MemberPathKey, MemberPath> Map = new();


    private MemberPath(MemberPathKey key, int structIndex, Type type, IEnumerable<CustomAttributeData> customAttributes, object getter, object setter) {
        this.key                = key;
        this.memberType         = type;
        this.customAttributes   = customAttributes;
        this.getter             = getter;
        this.setter             = setter;
        this.structIndex        = structIndex;
    }
    
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty;
    
    /// Calls <see cref="CreateGetter{TComponent,TField}"/> and <see cref="CreateSetter{TComponent,TField}"/> 
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2080", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    public static MemberPath Get(Type declarationType, string path)
    {
        path = Regex.Replace(path, @"\s+", "");
        var key = new MemberPathKey(declarationType, path);
        if (Map.TryGetValue(key, out var componentFieldInfo)) {
            return componentFieldInfo;
        }
        var pathItems   = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var memberInfos = new MemberInfo[pathItems.Length];
        var type        = declarationType;
        bool canWrite   = true;
        IEnumerable<CustomAttributeData> customAttributes = null;
        for (int i = 0; i < pathItems.Length; i++)
        {
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
        var typeParams      = new []{ declarationType, type };
        
        var getterMethod    = typeof(MemberPath).GetMethod("CreateGetter", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(MemberInfo[])], null)!;
        var genericGetter   = getterMethod.MakeGenericMethod(typeParams);
        var getter          = genericGetter.Invoke(null, [memberInfos]);
        
        object setter = null;
        if (canWrite) {
            var setterMethod    = typeof(MemberPath).GetMethod("CreateSetter", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(MemberInfo[])], null)!;
            var genericSetter   = setterMethod.MakeGenericMethod(typeParams);
            setter              = genericSetter.Invoke(null, [memberInfos]);
        }
        var structIndex = 0;
        if (EntityStoreBase.Static.EntitySchema.ComponentTypeByType.TryGetValue(declarationType, out var componentType)) {
            structIndex = componentType.StructIndex;
        }
        var memberPath = new MemberPath(key, structIndex, type, customAttributes, getter, setter);
        Map.Add(key, memberPath);
        return memberPath;
    }
    
    // ReSharper disable UnusedMember.Local
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    private static MemberPathGetter<TComponent,TField> CreateGetter<TComponent,TField>(MemberInfo[] fields)
    {
        var arg = Expression.Parameter(typeof(TComponent), "component"); // "component" parameter name in MemberGetter<,>
        Expression fieldExpr = arg;
        foreach (var field in fields) {
            fieldExpr = PropertyOrField(fieldExpr, field);
        }
        return Expression.Lambda<MemberPathGetter<TComponent, TField>>(fieldExpr, arg).Compile();
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    private static MemberPathSetter<TComponent,TField> CreateSetter<TComponent,TField>(MemberInfo[] fields)
    {
        var arg   = Expression.Parameter(typeof(TComponent).MakeByRefType(), "component"); // "component" parameter name in MemberSetter<,>
        var value = Expression.Parameter(typeof(TField),                     "value");     // "value" parameter name in MemberSetter<,>
        Expression fieldExpr = arg;
        foreach (var field in fields) {
            fieldExpr = PropertyOrField(fieldExpr, field);
        }
        var assign = Expression.Assign(fieldExpr, value);
        return Expression.Lambda<MemberPathSetter<TComponent, TField>>(assign, arg, value).Compile();
    }
    
    /// <summary>
    /// <see cref="Expression.PropertyOrField"/> does not search for exact names.
    /// E.g. in case of two fields: bla and Bla it prefers the public version.
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

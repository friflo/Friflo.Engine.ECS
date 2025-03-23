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


/// <summary>
/// Provide the attributes for a specific field / property within a specific <see cref="Type"/>.<br/>
/// It also enables to read / write the value of the field / property.
/// </summary>
public sealed class MemberPath 
{
    private  readonly   MemberPathKey                       key;
    
    internal readonly   int                                 structIndex;

    /// Returns the Type of the field / property.
    public   readonly   Type                                memberType;
    
    // ReSharper disable once InconsistentNaming
    /// Returns the Type containing the field / property.
    public              Type                                declaringType => key.type;
    
    /// Returns the components Type containing the field / property.
    public              ComponentType                       componentType;
    
    /// Returns the custom attributes of the field / property.
    public   readonly   IEnumerable<CustomAttributeData>    customAttributes;
    
    /// Identifies the field / property within its <see cref="declaringType"/>.
    // ReSharper disable once InconsistentNaming
    public              string                              path => key.path;
    
    /// Returns a delegate used to read the value of the field / property.<br/>
    /// Type: <see cref="MemberPathGetter{T,TField}"/>
    public   readonly   object                              getter;
    
    /// Returns a delegate used to set the value of the field / property.<br/>
    /// Type: <see cref="MemberPathSetter{T,TField}"/>. Is null if not writeable
    public   readonly   object                              setter;

    public override     string                              ToString() => GetString();
    
    private static readonly Dictionary<MemberPathKey, MemberPath> Map = new();

    private string GetString() {
        if (path == "") {
            return declaringType.Name;
        }
        return $"{declaringType.Name} {path} : {memberType.Name}";
    }

    private MemberPath(MemberPathKey key, ComponentType componentType, int structIndex, Type type, IEnumerable<CustomAttributeData> customAttributes, object getter, object setter) {
        this.key                = key;
        this.componentType      = componentType;
        this.memberType         = type;
        this.customAttributes   = customAttributes;
        this.getter             = getter;
        this.setter             = setter;
        this.structIndex        = structIndex;
    }
    
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty;
    
    /// <summary>
    /// Returns a <see cref="MemberPath"/> identifying a specific field / property by its <paramref name="path"/>
    /// within the passed <paramref name="type"/>.
    /// </summary>
    /// <remarks>
    /// Internal note: Calls <see cref="CreateGetter{TComponent,TField}"/> and <see cref="CreateSetter{TComponent,TField}"/>
    /// </remarks>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2080", Justification = "Not called for NativeAOT")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Not called for NativeAOT")]
    public static MemberPath Get(Type type, string path)
    {
        var key = new MemberPathKey(type, path);
        if (Map.TryGetValue(key, out var componentFieldInfo)) {
            return componentFieldInfo;
        }
        var pathItems   = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var memberInfos = new MemberInfo[pathItems.Length];
        var memberType  = type;
        bool canWrite   = true;
        IEnumerable<CustomAttributeData> customAttributes = type.CustomAttributes;
        for (int i = 0; i < pathItems.Length; i++)
        {
            var memberName      = pathItems[i];
            var members         = memberType.GetMember(memberName, Flags);
            if (members.Length == 0) {
                throw new InvalidOperationException($"Member '{memberName}' not found in '{type.Name}'");
            }
            var memberInfo      = members[0];
            memberInfos[i]      = memberInfo;
            customAttributes    = memberInfo.CustomAttributes;
            if (memberInfo is FieldInfo fieldInfo) {
                memberType = fieldInfo.FieldType;
                if (fieldInfo.IsInitOnly) {
                    canWrite = false;
                }
            } else if (memberInfo is PropertyInfo propertyInfo) {
                memberType = propertyInfo.PropertyType;
                if (!propertyInfo.CanWrite) {
                    canWrite = false;
                }
            }
            /* else { // not reachable with the given Flags
                // throw new InvalidOperationException($"Member '{memberName}' is not a field or property in '{type.Name}'");
            } */
        }
        var typeParams      = new []{ type, memberType };
        
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
        if (EntityStoreBase.Static.EntitySchema.ComponentTypeByType.TryGetValue(type, out var componentType)) {
            structIndex = componentType.StructIndex;
        }
        var memberPath = new MemberPath(key, componentType, structIndex, memberType, customAttributes, getter, setter);
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

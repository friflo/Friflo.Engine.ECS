// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal readonly struct MemberPathKey : IEquatable<MemberPathKey>
{
    internal readonly   Type    type;
    internal readonly   string  path;
    private  readonly   int     hashCode;

    public   override   string  ToString() => $"{type.Name} {path}";

    public override int GetHashCode() => hashCode;

    public bool Equals(MemberPathKey other) {
        return type == other.type && path == other.path;
    }

    internal MemberPathKey(Type type, string  path) {
        this.type   = type;
        this.path   = path;
        hashCode    = type.TypeHandle.GetHashCode() ^ path.GetHashCode();
    }
}

public delegate TField MemberPathGetter<T, out TField> (in T root);
public delegate void   MemberPathSetter<T, in  TField> (ref T root, TField value);
public delegate void   OnMemberChanged<T>(ref T value, Entity entity, string path, in T old);

/// <summary>
/// Identifies a specific field / property by its <see cref="path"/> within a specific <see cref="Type"/>.<br/>
/// It enables to read and write the field / property using the <see cref="getter"/> and <see cref="setter"/>.
/// </summary>
/// <remarks>
/// <see cref="MemberPath"/> instances are immutable.<br/>
/// Each combination of <see cref="declaringType"/> and <see cref="path"/> has single <see cref="MemberPath"/> instance.
/// </remarks>
public sealed class MemberPath 
{
    private  readonly   MemberPathKey   key;
    
    internal readonly   int             structIndex;

    /// The Type of the field / property.
    public   readonly   Type            memberType;
    
    // ReSharper disable once InconsistentNaming
    /// The Type containing the field / property.
    public              Type            declaringType => key.type;
    
    /// The components Type containing the field / property.
    public              ComponentType   componentType;
    
    /// The <see cref="MemberInfo"/> of the leaf member in the <see cref="path"/>.<br/>
    /// Is null if <see cref="path"/> == ""
    public   readonly   MemberInfo      memberInfo;
    
    /// Identifies the field / property within its <see cref="declaringType"/>.
    // ReSharper disable once InconsistentNaming
    public              string          path => key.path;
    
    /// Name of the leaf member. E.g. path: "value.Length" => name: "Length" 
    public              string          name;
    
    /// A delegate used to read the value of the field / property.<br/>
    /// Type: <see cref="MemberPathGetter{T,TField}"/>. Is null if not readable.
    public   readonly   Delegate        getter;
    
    /// A delegate used to set the value of the field / property.<br/>
    /// Type: <see cref="MemberPathSetter{T,TField}"/>. Is null if not writeable.
    public   readonly   Delegate        setter;

    public override     string          ToString() => GetString();
    
    private static readonly Dictionary<MemberPathKey, MemberPath> Map = new();

    private string GetString() {
        if (path == "") {
            return declaringType.Name;
        }
        return $"{declaringType.Name} {path} : {memberType.Name}";
    }

    private MemberPath(MemberPathKey key, ComponentType componentType, int structIndex, Type type, MemberInfo memberInfo, Delegate getter, Delegate setter) {
        this.key            = key;
        this.componentType  = componentType;
        this.memberType     = type;
        this.memberInfo     = memberInfo;
        this.getter         = getter;
        this.setter         = setter;
        this.structIndex    = structIndex;
        this.name           = memberInfo?.Name ?? type.Name;
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
        bool canRead    = true;
        MemberInfo memberInfo = null;
        var leafIndex = pathItems.Length - 1;
        for (int i = 0; i < pathItems.Length; i++)
        {
            var memberName      = pathItems[i];
            var members         = memberType.GetMember(memberName, Flags);
            if (members.Length == 0) {
                throw new InvalidOperationException($"Member '{memberName}' not found in Type '{type.Name}'");
            }
            memberInfo          = members[0];
            memberInfos[i]      = memberInfo;
            if (memberInfo is FieldInfo fieldInfo) {
                memberType = fieldInfo.FieldType;
                if (fieldInfo.IsInitOnly) {
                    if (i == leafIndex || memberType.IsValueType) {
                        canWrite = false;
                    }
                }
            } else if (memberInfo is PropertyInfo propertyInfo) {
                memberType = propertyInfo.PropertyType;
                if (!propertyInfo.CanWrite) {
                    if (i == leafIndex || memberType.IsValueType) {
                        canWrite = false;
                    }
                }
            }
            if (IsInvalidType(memberType)) {
                canRead  = false;
                canWrite = false;
            }
            /* else { // not reachable with the given Flags
                // throw new InvalidOperationException($"Member '{memberName}' is not a field or property in '{type.Name}'");
            } */
        }
        
        var typeParams  = new []{ type, memberType };
        Delegate getter = null;
        if (canRead) {
            var name 		    = path == "" ? $"get: ({type.Name} => this)" :  $"get: ({type.Name} => {path})";
            var getterMethod    = typeof(MemberPath).GetMethod("CreateGetter", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(string), typeof(MemberInfo[])], null)!;
            var genericGetter   = getterMethod.MakeGenericMethod(typeParams);
            getter              = (Delegate)genericGetter.Invoke(null, [name, memberInfos]);
        }
        Delegate setter = null;
        if (canWrite) {
            var name 		    = path == "" ? $"set: ({type.Name} => this)" :  $"set: ({type.Name} => {path})";
            var setterMethod    = typeof(MemberPath).GetMethod("CreateSetter", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(string), typeof(MemberInfo[])], null)!;
            var genericSetter   = setterMethod.MakeGenericMethod(typeParams);
            setter              = (Delegate)genericSetter.Invoke(null, [name, memberInfos]);
        }
        var structIndex = 0;
        if (EntityStoreBase.Static.EntitySchema.ComponentTypeByType.TryGetValue(type, out var componentType)) {
            structIndex = componentType.StructIndex;
        }
        var memberPath = new MemberPath(key, componentType, structIndex, memberType, memberInfo, getter, setter);
        Map.Add(key, memberPath);
        return memberPath;
    }
    
    // See ThrowIfTypeNeverValidGenericArgument() at:
    // https://github.com/dotnet/runtime/blob/4f5c6938d09e935830492c006aa8381611b65ad8/src/libraries/System.Private.CoreLib/src/System/RuntimeType.cs#L736
    private static bool IsInvalidType(Type type) {
        return  type.IsPointer || type.IsByRef || type == typeof(void) ||
                type.IsByRefLike; // type is a ref struct. E.g. Span<>
    }
    
    // ReSharper disable UnusedMember.Local
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    private static MemberPathGetter<TComponent,TField> CreateGetter<TComponent,TField>(string name, MemberInfo[] fields)
    {
        var arg = Expression.Parameter(typeof(TComponent).MakeByRefType(), "root");     // "root" parameter name in MemberPathGetter<,>
        Expression fieldExpr = arg;
        foreach (var field in fields) {
            fieldExpr = PropertyOrField(fieldExpr, field);
        }
        return Expression.Lambda<MemberPathGetter<TComponent, TField>>(fieldExpr, name, [arg]).Compile();
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Not called for NativeAOT")]
    private static MemberPathSetter<TComponent,TField> CreateSetter<TComponent,TField>(string name, MemberInfo[] fields)
    {
        var arg   = Expression.Parameter(typeof(TComponent).MakeByRefType(), "root");   // "root" parameter name in MemberPathSetter<,>
        var value = Expression.Parameter(typeof(TField),                     "value");  // "value" parameter name in MemberPathSetter<,>
        Expression fieldExpr = arg;
        foreach (var field in fields) {
            fieldExpr = PropertyOrField(fieldExpr, field);
        }
        var assign = Expression.Assign(fieldExpr, value);
        return Expression.Lambda<MemberPathSetter<TComponent, TField>>(assign, name, [arg, value]).Compile();
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

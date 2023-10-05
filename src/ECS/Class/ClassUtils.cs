// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Json.Burst;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class ClassTypeInfo<T> where T : class
{
    internal static readonly    int     ClassIndex  = ClassUtils.NewClassIndex(typeof(T), out ClassKey);
    internal static readonly    string  ClassKey;
}

public static class ClassUtils
{
    internal const              int                                 MissingAttribute            = 0;
    
    private  static             int                                 _nextClassIndex             = 1;
    private  static readonly    Dictionary<Type, Bytes>             ClassComponentBytes         = new Dictionary<Type, Bytes>();
    private  static readonly    Dictionary<Type, string>            ClassComponentKeys          = new Dictionary<Type, string>();
    public   static             IReadOnlyDictionary<Type, string>   RegisteredClassComponentKeys => ClassComponentKeys;

    internal static int NewClassIndex(Type type, out string classKey) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType == typeof(ClassComponentAttribute)) {
                var arg     = attr.ConstructorArguments;
                classKey    = (string) arg[0].Value;
                ClassComponentKeys.Add(type, classKey);
                ClassComponentBytes.Add(type, new Bytes(classKey));
                return _nextClassIndex++;
            }
        }
        classKey = null;
        return MissingAttribute;
    }
    
    internal static string GetClassKey(Type type) {
        return ClassComponentKeys[type];
    }
    
    internal static Bytes GetClassKeyBytes(Type type) {
        return ClassComponentBytes[type];
    }
}
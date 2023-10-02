// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Json.Burst;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class ClassType<T> where T : class
{
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    int     ClassIndex  = ClassUtils.NewClassIndex(typeof(T));
}

public static class ClassUtils
{
    internal const              int                                 MissingAttribute    = 0;
    
    private  static             int                                 _nextClassIndex     = 1;
    private  static readonly    Dictionary<Type, Bytes>             ClassKeysBytes      = new Dictionary<Type, Bytes>();
    private  static readonly    Dictionary<Type, string>            ClassKeys           = new Dictionary<Type, string>();
    public   static             IReadOnlyDictionary<Type, string>   RegisteredClassKeys => ClassKeys;

    internal static int NewClassIndex(Type type) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType == typeof(ClassComponentAttribute)) {
                var arg = attr.ConstructorArguments;
                var key = (string) arg[0].Value;
                ClassKeys.Add(type, key);
                ClassKeysBytes.Add(type, new Bytes(key));
                return _nextClassIndex++;
            }
        }
        return MissingAttribute;
    }
    
    internal static string GetClassKey(Type type) {
        return ClassKeys[type];
    }
    
    internal static Bytes GetClassKeyBytes(Type type) {
        return ClassKeysBytes[type];
    }
}
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class ClassType<T> where T : class
{
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly    int     ComponentIndex  = ClassUtils.NewComponentIndex(typeof(T));
}

public static class ClassUtils
{
    internal const              int                                 MissingAttribute    = 0;
    
    private  static             int                                 _nextComponentIndex = 1;
    private  static readonly    Dictionary<Type, string>            Types               = new Dictionary<Type, string>();
    public   static             IReadOnlyDictionary<Type, string>   RegisteredTypes     => Types;

    internal static int NewComponentIndex(Type type) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType == typeof(ClassComponentAttribute)) {
                var arg = attr.ConstructorArguments;
                var key = (string) arg[0].Value;
                Types.Add(type, key);
                return _nextComponentIndex++;
            }
        }
        return MissingAttribute;
    }
    
    internal static string GetKeyName(Type type) {
        Types.TryGetValue(type, out string key);
        return key;
    }
}
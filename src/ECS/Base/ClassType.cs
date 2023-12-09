// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class ClassType<T>
    where T : Script
{
    internal static readonly    int     ScriptIndex  = ClassUtils.NewClassIndex(typeof(T), out ScriptKey);
    internal static readonly    string  ScriptKey;
}

internal static class ClassUtils
{
    private  static     int     _nextScriptIndex    = 1;

//  private  const      int     MissingAttribute    = 0;
    
    internal static int NewClassIndex(Type type, out string classKey)
    {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(ComponentKeyAttribute)) {
                continue;
            }
            var arg     = attr.ConstructorArguments;
            classKey    = (string) arg[0].Value;
            return _nextScriptIndex++;
        }
        classKey = type.Name;
        return _nextScriptIndex++;
    }
}
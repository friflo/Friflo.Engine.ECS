// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class ClassType<T>
    where T : Behavior
{
    internal static readonly    int     BehaviorIndex  = ClassUtils.NewClassIndex(typeof(T), out BehaviorKey);
    internal static readonly    string  BehaviorKey;
}

internal static class ClassUtils
{
    private  static     int     _nextBehaviorIndex     = 1;

    internal const      int     MissingAttribute    = 0;

    internal static int NewClassIndex(Type type, out string classKey) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(BehaviorAttribute)) {
                continue;
            }
            var arg     = attr.ConstructorArguments;
            classKey    = (string) arg[0].Value;
            return _nextBehaviorIndex++;
        }
        classKey = null;
        return MissingAttribute;
    }
}
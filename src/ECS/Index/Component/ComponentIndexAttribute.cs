// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[AttributeUsage(AttributeTargets.Struct)]
internal sealed class ComponentIndexAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local
    public ComponentIndexAttribute(Type type) { }
    
    internal static Type GetComponentIndex(Type type)
    {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(ComponentIndexAttribute)) {
                continue;
            }
            var arg = attr.ConstructorArguments;
            return (Type) arg[0].Value;
        }
        return null;
    }
}


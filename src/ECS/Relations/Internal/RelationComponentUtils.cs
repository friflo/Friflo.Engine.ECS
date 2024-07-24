// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;

internal static class RelationComponentUtils
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "TODO")] // TODO
    internal static Type GetEntityRelationsType(Type componentType, out Type keyType)
    {
        var interfaces = componentType.GetInterfaces();
        foreach (var i in interfaces)
        {
            if (!i.IsGenericType) continue;
            var genericType = i.GetGenericTypeDefinition();
            if (genericType != typeof(IRelationComponent<>)) {
                continue;
            }
            keyType = i.GenericTypeArguments[0];
            return MakeIndexType(componentType, keyType);
        }
        keyType = null;
        return null;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055", Justification = "TODO")] // TODO
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "TODO")] // TODO
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    private static Type MakeIndexType(Type componentType, Type keyType)
    {
        if (keyType == typeof(Entity)) {
            return typeof(EntityRelationLinks<>).MakeGenericType(new [] { componentType });
        }
        return typeof(EntityRelations<,>).MakeGenericType(new [] { componentType, keyType });
    }
}


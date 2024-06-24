// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal static class IndexedComponentType
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2077", Justification = "TODO")] // TODO
    internal static ComponentIndex CreateComponentIndex(EntityStore store, ComponentType componentType)
    {
        var obj             = Activator.CreateInstance(componentType.IndexType);
        var index           = (ComponentIndex)obj!;
        index.store         = store;
        index.componentType = componentType;
        return index;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "TODO")] // TODO
    internal static Type GetIndexType(Type componentType)
    {
        var interfaces = componentType.GetInterfaces();
        foreach (var i in interfaces)
        {
            if (!i.IsGenericType) continue;
            var genericType = i.GetGenericTypeDefinition();
            if (genericType != typeof(IIndexedComponent<>)) {
                continue;
            }
            var valueType = i.GenericTypeArguments[0];
            return MakeIndexType(valueType, componentType);
        }
        return null;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055", Justification = "TODO")] // TODO
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "TODO")] // TODO
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    private static Type MakeIndexType(Type valueType, Type componentType)
    {
        if (valueType == typeof(Entity)) {
            return typeof(EntityIndex);
        }
        var indexType   = ComponentIndexAttribute.GetComponentIndex(componentType);
        var typeArgs    = new [] { valueType };
        if (indexType != null) {
            return indexType.                MakeGenericType(typeArgs);
        }
        if (valueType.IsClass) {
            return typeof(ValueClassIndex<>).MakeGenericType(typeArgs);
        }
        return typeof(ValueStructIndex<>).   MakeGenericType(typeArgs);
    }
}


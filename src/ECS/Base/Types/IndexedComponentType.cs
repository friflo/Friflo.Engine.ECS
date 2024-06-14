// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal readonly struct IndexedComponentType
{
    internal readonly ComponentType componentType;
    private  readonly MethodInfo    createIndexMethod;
    
    private IndexedComponentType(ComponentType componentType, MethodInfo createIndexMethod) {
        this.componentType      = componentType;
        this.createIndexMethod  = createIndexMethod;
    }
    
    internal InvertedIndex CreateInvertedIndex()
    {
        var index = createIndexMethod.Invoke(null, null);
        return (InvertedIndex)index;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2080", Justification = "TODO")] // TODO
    internal static void AddIndexedComponentType(SchemaTypes schemaTypes, ComponentType componentType)
    {
        var interfaces = componentType.Type.GetInterfaces();
        foreach (var i in interfaces)
        {
            if (!i.IsGenericType) continue;
            var genericType = i.GetGenericTypeDefinition();
            if (genericType != typeof(IIndexedComponent<>)) {
                continue;
            }
            var valueType               = i.GenericTypeArguments[0];
            var createMethod            = CreateCreateDelegate(valueType);
            var indexedComponentType    = new IndexedComponentType(componentType, createMethod);
            schemaTypes.indexedComponents.Add(indexedComponentType);
        }
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    private static MethodInfo CreateCreateDelegate(Type valueType)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method = typeof(IndexedComponentType).GetMethod(nameof(CreateInvertedIndexGeneric), flags);
        return method?.MakeGenericMethod(valueType);
    }
    
    internal static InvertedIndex CreateInvertedIndexGeneric<TValue>()
    {
        return new InvertedIndex<TValue>();
    }
}


// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal readonly struct IndexedComponentType
{
    /// <summary> Not null in case component is indexed. </summary>
    internal readonly ComponentType componentType;  //  8
    
    private  readonly Type          indexType;      //  8
    
    private IndexedComponentType(ComponentType componentType, Type indexType) {
        this.componentType  = componentType;
        this.indexType      = indexType;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2077", Justification = "TODO")] // TODO
    internal ComponentIndex CreateComponentIndex(EntityStore store)
    {
        var obj             = Activator.CreateInstance(indexType);
        var index           = (ComponentIndex)obj!;
        index.store         = store;
        index.componentType = componentType;
        return index;
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
            var indexType               = MakeIndexType(valueType, componentType.Type);
            var indexedComponentType    = new IndexedComponentType(componentType, indexType);
            schemaTypes.indexedComponents.Add(indexedComponentType);
        }
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


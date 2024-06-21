// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal delegate ComponentIndex CreateIndex(Type indexType);

internal readonly struct IndexedComponentType
{
    /// <summary> Not null in case component is indexed. </summary>
    internal readonly ComponentType componentType;  //  8
    
    /// <summary> If generic delegate of <see cref="CreateInvertedIndex{TValue}"/> </summary>
    private  readonly CreateIndex   createIndex;    //  8
    
    private  readonly Type          indexType;      //  8
    
    private IndexedComponentType(ComponentType componentType, CreateIndex createIndex, Type indexType) {
        this.componentType  = componentType;
        this.createIndex    = createIndex;
        this.indexType      = indexType;
    }
    
    internal ComponentIndex CreateComponentIndex(EntityStore store)
    {
        var index   = createIndex(indexType);
        index.store = store;
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
            var createIndex             = MakeCreateIndex(valueType);
            var indexedComponentType    = new IndexedComponentType(componentType, createIndex, indexType);
            schemaTypes.indexedComponents.Add(indexedComponentType);
        }
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055", Justification = "TODO")] // TODO
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "TODO")] // TODO
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    private static Type MakeIndexType(Type valueType, Type componentType)
    {
        if (valueType == typeof(Entity)) {
            return null;
        }
        var indexType   = ComponentIndexAttribute.GetComponentIndex(componentType);
        var typeArgs    = new [] { valueType };
        if (indexType != null) {
            return indexType.                   MakeGenericType(typeArgs);
        }
        if (valueType.IsClass) {
            return typeof(HasValueClassIndex<>).MakeGenericType(typeArgs);
        }
        return typeof(HasValueStructIndex<>).   MakeGenericType(typeArgs);
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    private static CreateIndex MakeCreateIndex(Type valueType)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var methodInfo      = typeof(IndexedComponentType).GetMethod(nameof(CreateInvertedIndex), flags);
        var genericMethod   = methodInfo?.MakeGenericMethod(valueType);
        return (CreateIndex)Delegate.CreateDelegate(typeof(CreateIndex), null, genericMethod!);
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067", Justification = "TODO")]
    internal static ComponentIndex CreateInvertedIndex<TValue>(Type indexType)
    {
        if (indexType != null) {
            return(ComponentIndex)Activator.CreateInstance(indexType);
        }
        if (typeof(TValue) == typeof(Entity)) {
            return new HasEntityIndex();    
        }
        throw new InvalidOperationException("unexpected index type");
    }
}


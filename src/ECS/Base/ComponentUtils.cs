// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class ComponentUtils
{
    internal static ComponentSchema RegisterComponentTypes(TypeStore typeStore)
    {
        var assemblyLoader  = new AssemblyLoader();
        var assemblies      = assemblyLoader.GetEngineDependants();
        Console.WriteLine(assemblyLoader);
        
        var types           = new List<Type>();
        foreach (var assembly in assemblies) {
            AssemblyLoader.AddComponentTypes(types, assembly);
        }
        var structs         = new List<ComponentType>(types.Count);
        var classes         = new List<ComponentType>(types.Count);
        var tags            = new List<ComponentType>(types.Count);
        foreach (var type in types) {
            RegisterComponentType(type, structs, classes, tags, typeStore);
        }
        return new ComponentSchema(assemblies, structs, classes, tags);
    }
    
    internal static void RegisterComponentType(
        Type                type,
        List<ComponentType> structs,
        List<ComponentType> classes,
        List<ComponentType> tags,
        TypeStore           typeStore)
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        
        if (type.IsValueType && typeof(IEntityTag).IsAssignableFrom(type)) {
            var method          = typeof(ComponentUtils).GetMethod(nameof(CreateTagType), flags);
            var genericMethod   = method!.MakeGenericMethod(type);
            var componentType   = (ComponentType)genericMethod.Invoke(null, null);
            tags.Add(componentType);
            return;
        }
        var createParams            = new object[] { typeStore };
        foreach (var attr in type.CustomAttributes)
        {
            var attributeType = attr.AttributeType;
            if (attributeType == typeof(StructComponentAttribute))
            {
                var method          = typeof(ComponentUtils).GetMethod(nameof(CreateStructFactory), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
                structs.Add(componentType);
                return;
            }
            if (attributeType == typeof(ClassComponentAttribute))
            {
                var method          = typeof(ComponentUtils).GetMethod(nameof(CreateClassFactory), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
                classes.Add(componentType);
                return;
            }
        }
        throw new InvalidOperationException($"missing expected attribute. Type: {type}");
    }
    
    internal static ComponentType CreateStructFactory<T>(TypeStore typeStore)
        where T : struct, IStructComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        var structKey   = StructHeap<T>.StructKey;
        return new StructComponentType<T>(structKey, structIndex, typeStore);
    }
    
    internal static ComponentType CreateClassFactory<T>(TypeStore typeStore)
        where T : ClassComponent
    {
        var classIndex  = ClassType<T>.ClassIndex;
        var classKey    = ClassType<T>.ClassKey;
        return new ClassComponentType<T>(classKey, classIndex, typeStore);
    }
    
    internal static ComponentType CreateTagType<T>()
        where T : struct, IEntityTag
    {
        var tagIndex    = TagType<T>.TagIndex;
        return new TagType(typeof(T), tagIndex);
    }
}

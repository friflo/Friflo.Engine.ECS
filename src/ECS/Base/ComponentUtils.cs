// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class ComponentUtils
{
    internal static ComponentSchema RegisterComponentTypes(TypeStore typeStore)
    {
        var assemblyLoader  = new AssemblyLoader();
        var assemblies      = assemblyLoader.GetEngineDependants();
        Console.WriteLine(assemblyLoader);
        
        var structs     = new List<ComponentType>();
        var classes     = new List<ComponentType>();
        var tags        = new List<ComponentType>();
        var dependants  = new List<EngineDependant>();
        foreach (var assembly in assemblies) {
            var types           = AssemblyLoader.AddComponentTypes(assembly);
            var componentTypes  = new List<ComponentType>();
            foreach (var type in types) {
                var componentType = RegisterComponentType(type, structs, classes, tags, typeStore);
                componentTypes.Add(componentType);
            }
            dependants.Add(new EngineDependant (assembly, componentTypes));
        }
        return new ComponentSchema(dependants, structs, classes, tags);
    }
    
    internal static ComponentType RegisterComponentType(
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
            return componentType;
        }
        var createParams            = new object[] { typeStore };
        foreach (var attr in type.CustomAttributes)
        {
            var attributeType = attr.AttributeType;
            if (attributeType == typeof(ComponentAttribute))
            {
                var method          = typeof(ComponentUtils).GetMethod(nameof(CreateStructFactory), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
                structs.Add(componentType);
                return componentType;
            }
            if (attributeType == typeof(BehaviorAttribute))
            {
                var method          = typeof(ComponentUtils).GetMethod(nameof(CreateClassFactory), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var componentType   = (ComponentType)genericMethod.Invoke(null, createParams);
                classes.Add(componentType);
                return componentType;
            }
        }
        throw new InvalidOperationException($"missing expected attribute. Type: {type}");
    }
    
    internal static ComponentType CreateStructFactory<T>(TypeStore typeStore)
        where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        var structKey   = StructHeap<T>.StructKey;
        return new StructComponentType<T>(structKey, structIndex, typeStore);
    }
    
    internal static ComponentType CreateClassFactory<T>(TypeStore typeStore)
        where T : Behavior
    {
        var behaviorIndex   = ClassType<T>.BehaviorIndex;
        var behaviorKey     = ClassType<T>.BehaviorKey;
        return new BehaviorType<T>(behaviorKey, behaviorIndex, typeStore);
    }
    
    internal static ComponentType CreateTagType<T>()
        where T : struct, IEntityTag
    {
        var tagIndex    = TagType<T>.TagIndex;
        return new TagType(typeof(T), tagIndex);
    }
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public class ComponentTypes
{
    public              ReadOnlySpan<ComponentFactory>          Structs => new (structs);
    public              ReadOnlySpan<ComponentFactory>          Classes => new (classes);
    
    private  readonly   ComponentFactory[]                      structs;
    private  readonly   ComponentFactory[]                      classes;
    internal readonly   Dictionary<string, ComponentFactory>    factories;
    
    internal ComponentTypes(List<ComponentFactory> structs, List<ComponentFactory> classes)
    {
        factories       = new Dictionary<string, ComponentFactory>(structs.Count + classes.Count);
        this.structs    = structs.ToArray();
        this.classes    = classes.ToArray();
        foreach (var structFactory in this.structs) {
            factories.Add(structFactory.componentKey, structFactory);
        }
        foreach (var classFactory in this.classes) {
            factories.Add(classFactory.componentKey, classFactory);
        }
    }
}

internal static class ComponentUtils
{
    internal static ComponentTypes RegisterComponentTypes(TypeStore typeStore)
    {
        var types   = GetComponentTypes();
        var structs = new List<ComponentFactory>(types.Count);
        var classes = new List<ComponentFactory>(types.Count);
        foreach (var type in types) {
            RegisterComponentType(type, structs, classes, typeStore);
        }
        return new ComponentTypes(structs, classes);
    }
    
    private static void RegisterComponentType(
        Type                    type,
        List<ComponentFactory>  structs,
        List<ComponentFactory>  classes,
        TypeStore               typeStore)
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var createParams            = new object[] { typeStore };
        foreach (var attr in type.CustomAttributes)
        {
            var attributeType = attr.AttributeType;
            if (attributeType == typeof(StructComponentAttribute))
            {
                var method          = typeof(ComponentUtils).GetMethod(nameof(CreateStructFactory), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var factory         = (ComponentFactory)genericMethod.Invoke(null, createParams);
                structs.Add(factory);
                return;
            }
            if (attributeType == typeof(ClassComponentAttribute))
            {
                var method          = typeof(ComponentUtils).GetMethod(nameof(CreateClassFactory), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var factory         = (ComponentFactory)genericMethod.Invoke(null, createParams);
                classes.Add(factory);
                return;
            }
        }
        throw new InvalidOperationException("missing expected attribute");
    }
    
    internal static ComponentFactory CreateStructFactory<T>(TypeStore typeStore) where T : struct  {
        var structIndex = StructHeap<T>.StructIndex;
        var structKey   = StructHeap<T>.StructKey;
        return new StructFactory<T>(structKey, structIndex, typeStore);
    }
    
    internal static ComponentFactory CreateClassFactory<T>(TypeStore typeStore) where T : ClassComponent  {
        var classKey    = ClassType<T>.ClassKey;
        return new ClassFactory<T>(classKey, typeStore);
    }
    
    // --------------------------- query all struct / class component types ---------------------------
    private static List<Type> GetComponentTypes()
    {
        var componentTypes  = new List<Type>();
        var engineAssembly  = typeof(Utils).Assembly;
        var engineFullName  = engineAssembly.FullName;
        AddComponentTypes(componentTypes, engineAssembly);
        
        var assemblies      = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        { 
            var referencedAssemblies = assembly.GetReferencedAssemblies();
            foreach (var referencedAssembly in referencedAssemblies) {
                if (referencedAssembly.FullName != engineFullName) {
                    continue;
                }
                AddComponentTypes(componentTypes, assembly);
                break;
            }
        }
        return componentTypes;
    }
    
    private static void AddComponentTypes(List<Type> componentTypes, Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types) {
            foreach (var attr in type.CustomAttributes)
            {
                var attributeType = attr.AttributeType;
                if (attributeType == typeof(StructComponentAttribute) ||
                    attributeType == typeof(ClassComponentAttribute))
                {
                    componentTypes.Add(type);
                }
            }
        }
    }
}
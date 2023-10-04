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
    /// <summary>return all struct component types attributed with <see cref="StructComponentAttribute"/></summary>
    public              ReadOnlySpan<ComponentType>          Structs => new (structs);
    /// <summary>return all class component types attributed with <see cref="ClassComponentAttribute"/></summary>
    public              ReadOnlySpan<ComponentType>          Classes => new (classes);
    
    private  readonly   ComponentType[]                      structs;
    private  readonly   ComponentType[]                      classes;
    private  readonly   Dictionary<Type,   ComponentType>    componentTypeByType;
    internal readonly   Dictionary<string, ComponentType>    componentTypeByKey;
    
    internal ComponentTypes(List<ComponentType> structs, List<ComponentType> classes)
    {
        int count           = structs.Count + classes.Count;
        componentTypeByKey  = new Dictionary<string, ComponentType>(count);
        componentTypeByType = new Dictionary<Type,   ComponentType>(count);
        this.structs        = structs.ToArray();
        this.classes        = classes.ToArray();
        foreach (var structType in this.structs) {
            componentTypeByKey. Add(structType.componentKey, structType);
            componentTypeByType.Add(structType.type,         structType);
        }
        foreach (var classType in this.classes) {
            componentTypeByKey. Add(classType.componentKey, classType);
            componentTypeByType.Add(classType.type,         classType);
        }
    }
    
    /// <summary>
    /// return <see cref="ComponentType"/> of a component type attributed with
    /// <see cref="StructComponentAttribute"/> or <see cref="ClassComponentAttribute"/> for the given key
    /// </summary>
    public ComponentType GetComponentTypeByKey(string key) {
        componentTypeByKey.TryGetValue(key, out var result);
        return result;
    }
    
    /// <summary>
    /// return <see cref="ComponentType"/> of a struct attributed with <see cref="StructComponentAttribute"/> for the given key
    /// </summary>
    public ComponentType GetStructComponentType<T>() where T : struct
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// return <see cref="ComponentType"/> of a class attributed with <see cref="ClassComponentAttribute"/> for the given type
    /// </summary>
    public ComponentType GetClassComponentType<T>() where T : ClassComponent
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
}

internal static class ComponentUtils
{
    internal static ComponentTypes RegisterComponentTypes(TypeStore typeStore)
    {
        var types   = GetComponentTypes();
        var structs = new List<ComponentType>(types.Count);
        var classes = new List<ComponentType>(types.Count);
        foreach (var type in types) {
            RegisterComponentType(type, structs, classes, typeStore);
        }
        return new ComponentTypes(structs, classes);
    }
    
    private static void RegisterComponentType(
        Type                type,
        List<ComponentType> structs,
        List<ComponentType> classes,
        TypeStore           typeStore)
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
        throw new InvalidOperationException("missing expected attribute");
    }
    
    internal static ComponentType CreateStructFactory<T>(TypeStore typeStore) where T : struct  {
        var structIndex = StructHeap<T>.StructIndex;
        var structKey   = StructHeap<T>.StructKey;
        return new StructComponentType<T>(structKey, structIndex, typeStore);
    }
    
    internal static ComponentType CreateClassFactory<T>(TypeStore typeStore) where T : ClassComponent  {
        var classKey    = ClassTypeInfo<T>.ClassKey;
        return new ClassComponentType<T>(classKey, typeStore);
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
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

namespace Friflo.Fliox.Engine.ECS;

internal abstract class ComponentFactory
{
    internal readonly   string  componentKey;
    internal readonly   int     structIndex;
    internal readonly   long    structHash;
    internal readonly   bool    isStructFactory;
        
    internal abstract   StructHeap  CreateHeap          (int capacity);
    internal abstract   void        ReadClassComponent  (ObjectReader reader, JsonValue json, GameEntity entity);
    
    internal ComponentFactory(string componentKey, bool isStructFactory, int structIndex, long structHash) {
        this.componentKey       = componentKey;
        this.structIndex        = structIndex;
        this.structHash         = structHash;
        this.isStructFactory    = isStructFactory;
    }
}

internal sealed class StructFactory<T> : ComponentFactory 
    where T : struct
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"StructFactory: {typeof(T).Name}";

    internal StructFactory(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, true, structIndex, typeof(T).Handle())
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   void    ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException("operates only on ClassFactory<>");
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(structIndex, componentKey, capacity, typeMapper);   
    }
}

internal sealed class ClassFactory<T> : ComponentFactory 
    where T : ClassComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"ClassFactory: {typeof(T).Name}";
    
    internal ClassFactory(string componentKey, TypeStore typeStore)
        : base(componentKey, false, -1, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   StructHeap  CreateHeap(int capacity)
        => throw new InvalidOperationException("operates only on StructFactory<>");
    
    internal override void ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity) {
        var classComponent = entity.GetClassComponent<T>();
        if (classComponent != null) { 
            reader.ReadToMapper(typeMapper, json, classComponent, true);
            return;
        }
        classComponent = reader.ReadMapper(typeMapper, json);
        entity.AppendClassComponent(classComponent);
    }
}

internal class ComponentTypes
{
    internal readonly int                                   structComponentCount;
    internal readonly Dictionary<string, ComponentFactory>  factories;
    
    internal ComponentTypes(Dictionary<string, ComponentFactory> factories, int structComponentCount) {
        this.factories              = factories;
        this.structComponentCount   = structComponentCount;
    }
}

internal static class ComponentUtils
{
    internal static ComponentTypes RegisterComponentTypes(TypeStore typeStore)
    {
        var types       = GetComponentTypes();
        var factories   = new Dictionary<string, ComponentFactory>(types.Count);
        var structCount = 0;
        foreach (var type in types) {
            if (RegisterComponentType(type, factories, typeStore)) {
                structCount++;
            }
        }
        return new ComponentTypes(factories, structCount);
    }
    
    private static bool RegisterComponentType(
        Type                                    type,
        Dictionary<string, ComponentFactory>    factories,
        TypeStore                               typeStore)
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
                factories.Add(factory!.componentKey, factory);
                return true;
            }
            if (attributeType == typeof(ClassComponentAttribute))
            {
                var method          = typeof(ComponentUtils).GetMethod(nameof(CreateClassFactory), flags);
                var genericMethod   = method!.MakeGenericMethod(type);
                var factory         = (ComponentFactory)genericMethod.Invoke(null, createParams);
                factories.Add(factory!.componentKey, factory);
            }
        }
        return false;
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
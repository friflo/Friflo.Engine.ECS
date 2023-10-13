// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Json.Fliox.Mapper;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed class ComponentSchema
{
#region public properties
    public   ReadOnlySpan<Assembly>                         Dependencies        => new (dependencies);
    /// <summary>return all struct component types attributed with <see cref="StructComponentAttribute"/></summary>
    /// <remarks>
    /// <see cref="ComponentType.structIndex"/> is equal to the array index<br/>
    /// <see cref="Structs"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ComponentType>                    Structs             => new (structs);
    /// <summary>return all class component types attributed with <see cref="ClassComponentAttribute"/></summary>
    /// <remarks>
    /// <see cref="ComponentType.classIndex"/> is equal to the array index<br/>
    /// <see cref="Classes"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ComponentType>                    Classes             => new (classes);
    /// <summary>return all entity <b>Tag</b>'s - structs extending <see cref="IEntityTag"/></summary>
    /// <remarks>
    /// <see cref="ComponentType.tagIndex"/> is equal to the array index<br/>
    /// <see cref="Tags"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ComponentType>                    Tags                => new (tags);
    
    public   IReadOnlyDictionary<string, ComponentType>     ComponentTypeByKey  => componentTypeByKey;
    public   IReadOnlyDictionary<Type,   ComponentType>     ComponentTypeByType => componentTypeByType;
    public   IReadOnlyDictionary<Type,   ComponentType>     TagTypeByType       => tagTypeByType;

    public   override string                                ToString()          => GetString();

    #endregion
    
#region private fields
    [Browse(Never)] internal readonly   Assembly[]                          dependencies;
    [Browse(Never)] internal readonly   int                                 maxStructIndex;
    [Browse(Never)] private  readonly   ComponentType[]                     structs;
    [Browse(Never)] private  readonly   ComponentType[]                     classes;
    [Browse(Never)] private  readonly   ComponentType[]                     tags;
    [Browse(Never)] private  readonly   Dictionary<string, ComponentType>   componentTypeByKey;
    [Browse(Never)] private  readonly   Dictionary<Type,   ComponentType>   componentTypeByType;
    [Browse(Never)] private  readonly   Dictionary<Type,   ComponentType>   tagTypeByType;
    #endregion
    
#region internal methods
    internal ComponentSchema(
        Assembly[]          dependencies,
        List<ComponentType> structList,
        List<ComponentType> classList,
        List<ComponentType> tagList)
    {
        this.dependencies   = dependencies;
        int count           = structList.Count + classList.Count;
        componentTypeByKey  = new Dictionary<string, ComponentType>(count);
        componentTypeByType = new Dictionary<Type,   ComponentType>(count);
        tagTypeByType       = new Dictionary<Type,   ComponentType>(count);
        maxStructIndex      = structList.Count + 1;
        structs             = new ComponentType[maxStructIndex];
        classes             = new ComponentType[classList.Count + 1];
        tags                = new ComponentType[tagList.Count + 1];
        foreach (var structType in structList) {
            componentTypeByKey. Add(structType.componentKey, structType);
            componentTypeByType.Add(structType.type,         structType);
            structs[structType.structIndex] = structType;
        }
        foreach (var classType in classList) {
            componentTypeByKey. Add(classType.componentKey, classType);
            componentTypeByType.Add(classType.type,         classType);
            classes[classType.classIndex] = classType;
        }
        foreach (var tagType in tagList) {
            tagTypeByType.Add(tagType.type, tagType);
            tags[tagType.tagIndex] = tagType;
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
    public ComponentType GetStructComponentType<T>()
        where T : struct, IStructComponent
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// return <see cref="ComponentType"/> of a class attributed with <see cref="ClassComponentAttribute"/> for the given type
    /// </summary>
    public ComponentType GetClassComponentType<T>()
        where T : ClassComponent
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <remarks>
    /// Ensures <see cref="StructHeap.structIndex"/> and <see cref="StructHeap{T}.StructIndex"/> is less than <see cref="maxStructIndex"/><br/>
    /// to make range check redundant when accessing <see cref="Archetype.heapMap"/>[] using an index.
    /// </remarks>
    internal ComponentType GetStructType(int structIndex, Type type)
    {
        CheckStructIndex(structIndex, type);
        return structs[structIndex];
    }
    
    internal int CheckStructIndex(int structIndex, Type type)
    {
        if (structIndex == StructUtils.MissingAttribute) {
            var msg = $"Missing attribute [StructComponent(\"<key>\")] on type: {type.Namespace}.{type.Name}";
            throw new InvalidOperationException(msg);
        }
        if (structIndex >= maxStructIndex) {
            const string msg = $"number of structs exceed EntityStore.{nameof(maxStructIndex)}";
            throw new InvalidOperationException(msg);
        }
        return structIndex;
    }
    
    internal ComponentType GetTagAt(int index) {
        return tags[index];
    }
    
    internal ComponentType GetStructComponentAt(int index) {
        return structs[index];
    }
    
    private string GetString() {
        return $"struct components: {structs.Length - 1}  class components: {classes.Length - 1}  entity tags: {tags.Length - 1}";
    } 
    #endregion
}

internal static class ComponentUtils
{
    internal static ComponentSchema RegisterComponentTypes(TypeStore typeStore)
    {
        var assemblyLoader  = new AssemblyLoader();
        var assemblies      = assemblyLoader.GetAssemblies();
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
    
    private static void RegisterComponentType(
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
        throw new InvalidOperationException("missing expected attribute");
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

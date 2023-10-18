// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[CLSCompliant(true)]
public sealed class ComponentSchema
{
#region public properties
    /// <summary>List of <see cref="Assembly"/>'s referencing the <b>Fliox.Engine</b> assembly as dependency.</summary>
    public   ReadOnlySpan<Assembly>                         EngineDependants    => new (engineDependants);
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
    [Browse(Never)] private  readonly   Assembly[]                          engineDependants;
    [Browse(Never)] internal readonly   int                                 maxStructIndex;
    [Browse(Never)] internal readonly   ComponentType[]                     structs;
    [Browse(Never)] private  readonly   ComponentType[]                     classes;
    [Browse(Never)] private  readonly   ComponentType[]                     tags;
    [Browse(Never)] internal readonly   Dictionary<string, ComponentType>   componentTypeByKey;
    [Browse(Never)] internal readonly   Dictionary<Type,   ComponentType>   componentTypeByType;
    [Browse(Never)] internal readonly   Dictionary<string, ComponentType>   tagTypeByName;
    [Browse(Never)] private  readonly   Dictionary<Type,   ComponentType>   tagTypeByType;
    #endregion
    
#region internal methods
    internal ComponentSchema(
        Assembly[]          engineDependants,
        List<ComponentType> structList,
        List<ComponentType> classList,
        List<ComponentType> tagList)
    {
        this.engineDependants   = engineDependants;
        int count               = structList.Count + classList.Count;
        componentTypeByKey      = new Dictionary<string, ComponentType>(count);
        componentTypeByType     = new Dictionary<Type,   ComponentType>(count);
        tagTypeByName           = new Dictionary<string, ComponentType>(count);
        tagTypeByType           = new Dictionary<Type,   ComponentType>(count);
        maxStructIndex          = structList.Count + 1;
        structs                 = new ComponentType[maxStructIndex];
        classes                 = new ComponentType[classList.Count + 1];
        tags                    = new ComponentType[tagList.Count + 1];
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
            tagTypeByType.Add(tagType.type,      tagType);
            tagTypeByName.Add(tagType.type.Name, tagType);
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
    internal ComponentType GetStructType(Type structType, int structIndex)
    {
        CheckStructIndex(structType, structIndex);
        return structs[structIndex];
    }
    
    internal int CheckStructIndex(Type structType, int structIndex)
    {
        if (structIndex == StructUtils.MissingAttribute) {
            var msg = $"Missing attribute [StructComponent(\"<key>\")] on type: {structType.Namespace}.{structType.Name}";
            throw new InvalidOperationException(msg);
        }
        if (structIndex >= maxStructIndex) {
            string msg = $"number of structs exceed EntityStore.{nameof(maxStructIndex)}: {maxStructIndex}";
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

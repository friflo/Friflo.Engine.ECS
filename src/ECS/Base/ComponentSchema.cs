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
    /// <summary>return all component types attributed with <see cref="ComponentAttribute"/></summary>
    /// <remarks>
    /// <see cref="ComponentType.structIndex"/> is equal to the array index<br/>
    /// <see cref="Components"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ComponentType>                    Components          => new (components);
    /// <summary>return all <see cref="Behavior"/> types attributed with <see cref="BehaviorAttribute"/></summary>
    /// <remarks>
    /// <see cref="ComponentType.behaviorIndex"/> is equal to the array index<br/>
    /// <see cref="Behaviors"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ComponentType>                    Behaviors           => new (behaviors);
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
    [Browse(Never)] internal readonly   ComponentType[]                     components;
    [Browse(Never)] private  readonly   ComponentType[]                     behaviors;
    [Browse(Never)] private  readonly   ComponentType[]                     tags;
    [Browse(Never)] internal readonly   ComponentType                       unresolvedType;
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
        components              = new ComponentType[maxStructIndex];
        behaviors               = new ComponentType[classList.Count + 1];
        tags                    = new ComponentType[tagList.Count + 1];
        foreach (var structType in structList) {
            componentTypeByKey. Add(structType.componentKey, structType);
            componentTypeByType.Add(structType.type,         structType);
            components[structType.structIndex] = structType;
        }
        unresolvedType = components[StructHeap<Unresolved>.StructIndex];
        foreach (var classType in classList) {
            componentTypeByKey.Add(classType.componentKey, classType);
            componentTypeByType.Add(classType.type,         classType);
            behaviors[classType.behaviorIndex] = classType;
        }
        foreach (var tagType in tagList) {
            tagTypeByType.Add(tagType.type,      tagType);
            tagTypeByName.Add(tagType.type.Name, tagType);
            tags[tagType.tagIndex] = tagType;
        }
    }
    
    /// <summary>
    /// return <see cref="ComponentType"/> of a struct attributed with <see cref="ComponentAttribute"/> for the given key
    /// </summary>
    public ComponentType GetComponentType<T>()
        where T : struct, IComponent
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// return <see cref="ComponentType"/> of a class attributed with <see cref="BehaviorAttribute"/> for the given type
    /// </summary>
    public ComponentType GetBehaviorType<T>()
        where T : Behavior
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// return <see cref="ComponentType"/> of a class attributed with <see cref="BehaviorAttribute"/> for the given type
    /// </summary>
    public ComponentType GetTagType<T>()
        where T : struct, IEntityTag
    {
        tagTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <remarks>
    /// Ensures <see cref="StructHeap.structIndex"/> and <see cref="StructHeap{T}.StructIndex"/> is less than <see cref="maxStructIndex"/><br/>
    /// to make range check redundant when accessing <see cref="Archetype.heapMap"/>[] using an index.
    /// </remarks>
    internal int CheckStructIndex(Type structType, int structIndex)
    {
        if (structIndex == StructInfo.MissingAttribute) {
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
        return components[index];
    }
    
    private string GetString() {
        return $"components: {components.Length - 1}  behaviors: {behaviors.Length - 1}  entity tags: {tags.Length - 1}";
    } 
    #endregion
}

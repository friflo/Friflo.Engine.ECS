// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[CLSCompliant(true)]
public sealed class EntitySchema
{
#region public properties
    /// <summary>List of <see cref="Assembly"/>'s referencing the <b>Fliox.Engine</b> assembly as dependency.</summary>
    public   ReadOnlySpan<EngineDependant>              EngineDependants    => new (engineDependants);
    /// <summary>return all component types attributed with <see cref="ComponentAttribute"/></summary>
    /// <remarks>
    /// <see cref="ComponentType.structIndex"/> is equal to the array index<br/>
    /// <see cref="Components"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ComponentType>                Components          => new (components);
    /// <summary>return all <see cref="Script"/> types attributed with <see cref="ScriptAttribute"/></summary>
    /// <remarks>
    /// <see cref="ScriptType.scriptIndex"/> is equal to the array index<br/>
    /// <see cref="Scripts"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ScriptType>                   Scripts             => new (scripts);
    /// <summary>return all entity <b>Tag</b>'s - structs extending <see cref="IEntityTag"/></summary>
    /// <remarks>
    /// <see cref="TagType.tagIndex"/> is equal to the array index<br/>
    /// <see cref="Tags"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<TagType>                      Tags                => new (tags);
    
    public   IReadOnlyDictionary<string, SchemaType>    SchemaTypeByKey     => schemaTypeByKey;
    public   IReadOnlyDictionary<Type,   ScriptType>    ScriptTypeByType    => scriptTypeByType;
    public   IReadOnlyDictionary<Type,   ComponentType> ComponentTypeByType => componentTypeByType;
    public   IReadOnlyDictionary<Type,   TagType>       TagTypeByType       => tagTypeByType;

    public   override string                            ToString()          => GetString();

    #endregion
    
#region private fields
    [Browse(Never)] private  readonly   EngineDependant[]                   engineDependants;
    [Browse(Never)] internal readonly   int                                 maxStructIndex;
    [Browse(Never)] internal readonly   ComponentType[]                     components;
    [Browse(Never)] private  readonly   ScriptType[]                        scripts;
    [Browse(Never)] private  readonly   TagType[]                           tags;
    [Browse(Never)] internal readonly   ComponentType                       unresolvedType;
    [Browse(Never)] internal readonly   Dictionary<string, SchemaType>      schemaTypeByKey;
    [Browse(Never)] internal readonly   Dictionary<Type,   ScriptType>      scriptTypeByType;
    [Browse(Never)] private  readonly   Dictionary<Type,   ComponentType>   componentTypeByType;
    [Browse(Never)] internal readonly   Dictionary<string, TagType>         tagTypeByName;
    [Browse(Never)] private  readonly   Dictionary<Type,   TagType>         tagTypeByType;
    #endregion
    
#region internal methods
    internal EntitySchema(
        List<EngineDependant>   dependants,
        List<ComponentType>     componentList,
        List<ScriptType>        scriptList,
        List<TagType>           tagList)
    {
        engineDependants        = dependants.ToArray();
        int count               = componentList.Count + scriptList.Count;
        schemaTypeByKey         = new Dictionary<string, SchemaType>(count);
        scriptTypeByType        = new Dictionary<Type,   ScriptType>(count);
        componentTypeByType     = new Dictionary<Type,   ComponentType>();
        tagTypeByName           = new Dictionary<string, TagType>   (count);
        tagTypeByType           = new Dictionary<Type,   TagType>   (count);
        maxStructIndex          = componentList.Count + 1;
        components              = new ComponentType[maxStructIndex];
        scripts                 = new ScriptType[scriptList.Count + 1];
        tags                    = new TagType   [tagList.Count + 1];
        foreach (var componentType in componentList) {
            schemaTypeByKey.    Add(componentType.componentKey, componentType);
            componentTypeByType.Add(componentType.type,         componentType);
            components[componentType.structIndex] = componentType;
        }
        unresolvedType = components[StructHeap<Unresolved>.StructIndex];
        foreach (var scriptType in scriptList) {
            schemaTypeByKey.   Add(scriptType.componentKey,  scriptType);
            scriptTypeByType.  Add(scriptType.type,          scriptType);
            scripts[scriptType.scriptIndex] = scriptType;
        }
        foreach (var tagType in tagList) {
            tagTypeByType.Add(tagType.type, tagType);
            tagTypeByName.Add(tagType.name, tagType);
            tags[tagType.tagIndex] = tagType;
        }
    }
    
    /// <summary>
    /// return <see cref="SchemaType"/> of a struct attributed with <see cref="ComponentAttribute"/> for the given key
    /// </summary>
    public ComponentType GetComponentType<T>()
        where T : struct, IComponent
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// return <see cref="SchemaType"/> of a class attributed with <see cref="ScriptAttribute"/> for the given type
    /// </summary>
    public ScriptType GetScriptType<T>()
        where T : Script
    {
        scriptTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// return <see cref="SchemaType"/> of a class attributed with <see cref="ScriptAttribute"/> for the given type
    /// </summary>
    public TagType GetTagType<T>()
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
    
    internal TagType GetTagAt(int index) {
        return tags[index];
    }
    
    internal ComponentType GetComponentAt(int index) {
        return components[index];
    }
    
    private string GetString() {
        return $"components: {components.Length - 1}  scripts: {scripts.Length - 1}  entity tags: {tags.Length - 1}";
    } 
    #endregion
}

    
public readonly struct EngineDependant
{
                    public  ReadOnlySpan<SchemaType>    Types       => new (types);
                    public              Assembly        Assembly    => assembly;
    
    [Browse(Never)] private readonly    Assembly        assembly;
    [Browse(Never)] private readonly    SchemaType[]    types;

    public override                     string          ToString()  => assembly.ManifestModule.Name;

    internal EngineDependant(Assembly assembly, List<SchemaType> types) {
        this.assembly   = assembly;
        this.types      = types.ToArray();
    }
}

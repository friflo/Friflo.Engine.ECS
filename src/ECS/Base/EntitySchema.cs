// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide type information about all <see cref="ITag"/>, <see cref="IComponent"/> and <see cref="Script"/> types
/// available in the application.
/// </summary>
[CLSCompliant(true)]
public sealed class EntitySchema
{
#region public properties
    /// <summary> List of <see cref="Assembly"/>'s referencing the <b>Fliox.Engine</b> assembly as dependency. </summary>
    public   ReadOnlySpan<EngineDependant>              EngineDependants    => new (engineDependants);
    /// <summary> Return all <b>component</b> types - structs implementing <see cref="IComponent"/>. </summary>
    /// <remarks>
    /// <see cref="ComponentType.StructIndex"/> is equal to the array index<br/>
    /// <see cref="Components"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ComponentType>                Components          => new (components);
    /// <summary> Return all <see cref="Script"/> types - classes extending <see cref="Script"/></summary>
    /// <remarks>
    /// <see cref="ScriptType.ScriptIndex"/> is equal to the array index<br/>
    /// <see cref="Scripts"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ScriptType>                   Scripts             => new (scripts);
    /// <summary> Return all <b>Tag</b> types - structs implementing <see cref="ITag"/>. </summary>
    /// <remarks>
    /// <see cref="TagType.TagIndex"/> is equal to the array index<br/>
    /// <see cref="Tags"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<TagType>                      Tags                => new (tags);
    
    // --- lookup: components / scripts
    /// <summary> A map to lookup <see cref="ComponentType"/>'s and <see cref="ScriptType"/>'s by <see cref="SchemaType.ComponentKey"/>. </summary>
    public   IReadOnlyDictionary<string, SchemaType>    SchemaTypeByKey     => schemaTypeByKey;
    
    /// <summary> A map to lookup <see cref="ScriptType"/>'s by <see cref="System.Type"/>. </summary>
    public   IReadOnlyDictionary<Type,   ScriptType>    ScriptTypeByType    => scriptTypeByType;
    
    /// <summary> A map to lookup <see cref="ComponentType"/>'s by <see cref="System.Type"/>. </summary>
    public   IReadOnlyDictionary<Type,   ComponentType> ComponentTypeByType => componentTypeByType;
    
    // --- lookup: tags
    /// <summary> A map to lookup <see cref="TagType"/>'s by <see cref="TagType.TagName"/>. </summary>
    public   IReadOnlyDictionary<string, TagType>       TagTypeByName       => tagTypeByName;
    
    /// <summary> A map to lookup <see cref="TagType"/>'s by <see cref="System.Type"/>. </summary>
    public   IReadOnlyDictionary<Type,   TagType>       TagTypeByType       => tagTypeByType;

    public   override string                            ToString()          => GetString();

    #endregion
    
#region private fields
    [Browse(Never)] private  readonly   EngineDependant[]                   engineDependants;
    [Browse(Never)] internal readonly   int                                 maxStructIndex;
    [Browse(Never)] internal readonly   ComponentType[]                     components;
    [Browse(Never)] internal readonly   ScriptType[]                        scripts;
    [Browse(Never)] internal readonly   TagType[]                           tags;
    [Browse(Never)] internal readonly   ComponentType                       unresolvedType;
    // --- lookup: component / script
    [Browse(Never)] internal readonly   Dictionary<string, SchemaType>      schemaTypeByKey;
    [Browse(Never)] internal readonly   Dictionary<Type,   ScriptType>      scriptTypeByType;
    [Browse(Never)] private  readonly   Dictionary<Type,   ComponentType>   componentTypeByType;
    // --- lookup: tags
    [Browse(Never)] internal readonly   Dictionary<string, TagType>         tagTypeByName;
    [Browse(Never)] private  readonly   Dictionary<Type,   TagType>         tagTypeByType;
    #endregion
    
#region internal methods
    internal EntitySchema(List<EngineDependant> dependants, SchemaTypes schemaTypes)
    {
        var componentList   = schemaTypes.components;
        var scriptList      = schemaTypes.scripts;
        var tagList         = schemaTypes.tags;
        
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

        // --- Solved workaround. But leave it here for record. SHOULD_USE_ADD
        // Commented methods should use Dictionary<,>.Add()
        // But doing so will throw the exception below in Avalonia Designer
        //     System.ArgumentException: An item with the same key has already been added.
        // => so for now use Dictionary<,> index operator
        foreach (var componentType in componentList) {
            schemaTypeByKey.    Add (componentType.ComponentKey,    componentType); // SHOULD_USE_ADD
            componentTypeByType.Add (componentType.Type,            componentType);
            components              [componentType.StructIndex] =   componentType;
        }
        unresolvedType = componentTypeByType[typeof(Unresolved)];
        foreach (var scriptType in scriptList) {
            schemaTypeByKey. Add    (scriptType.ComponentKey,       scriptType);    // SHOULD_USE_ADD
            scriptTypeByType.Add    (scriptType.Type,               scriptType);
            scripts                 [scriptType.ScriptIndex] =      scriptType;
        }
        foreach (var tagType in tagList) {
            tagTypeByName.Add       (tagType.TagName,               tagType);       // SHOULD_USE_ADD
            tagTypeByType.Add       (tagType.Type,                  tagType);
            tags                    [tagType.TagIndex] =            tagType;
        }
    }
    
    /// <summary>
    /// Return the <see cref="ComponentType"/> of a struct implementing <see cref="IComponent"/>.
    /// </summary>
    public ComponentType GetComponentType<T>()
        where T : struct, IComponent
    {
        componentTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// Return the <see cref="ScriptType"/> of a class extending <see cref="Script"/>.
    /// </summary>
    public ScriptType GetScriptType<T>()
        where T : Script
    {
        scriptTypeByType.TryGetValue(typeof(T), out var result);
        return result;
    }
    
    /// <summary>
    /// Return the <see cref="TagType"/> of a struct implementing <see cref="ITag"/>.
    /// </summary>
    public TagType GetTagType<T>()
        where T : struct, ITag
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
        if (structIndex >= maxStructIndex) {
            string msg = $"number of component types exceed EntityStore.{nameof(maxStructIndex)}: {maxStructIndex}";
            throw new InvalidOperationException(msg);
        }
        return structIndex;
    }
    
    internal TagType GetTagAt(int index) {
        return tags[index];
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

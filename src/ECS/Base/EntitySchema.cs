// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
    [Browse(Never)] internal readonly   int                                 maxIndexedStructIndex; // :)
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
    // --- component type masks
    [Browse(Never)] internal readonly   ComponentTypes                      relationTypes;
    [Browse(Never)] internal readonly   ComponentTypes                      indexTypes;
    [Browse(Never)] internal readonly   ComponentTypes                      linkComponentTypes;
    [Browse(Never)] internal readonly   ComponentTypes                      linkRelationTypes;
    #endregion
    
#region internal methods
    internal EntitySchema(EngineDependant[] dependants, SchemaTypes schemaTypes)
    {
        var componentList   = schemaTypes.components;
        var scriptList      = schemaTypes.scripts;
        var tagList         = schemaTypes.tags;
        
        maxIndexedStructIndex   = schemaTypes.indexCount + 1;
        engineDependants        = dependants;
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
            var key = componentType.ComponentKey;
            if (key != null) {
                if (!schemaTypeByKey.TryAdd(key, componentType)) {
                    DuplicateComponentKey(componentType);
                }
            }
            componentTypeByType.Add (componentType.Type,            componentType);
            components              [componentType.StructIndex] =   componentType;
            if (componentType.RelationType != null) {
                relationTypes.Add(new ComponentTypes(componentType));
                if (componentType.RelationKeyType == typeof(Entity)) {
                    linkRelationTypes.Add(new ComponentTypes(componentType));
                }
            }
            if (componentType.IndexType != null) {
                indexTypes.Add(new ComponentTypes(componentType));
                if (componentType.IndexValueType == typeof(Entity)) {
                    linkComponentTypes.Add(new ComponentTypes(componentType));
                }
            }
        }
        unresolvedType = componentTypeByType[typeof(Unresolved)];

        foreach (var scriptType in scriptList) {
            var key = scriptType.ComponentKey;
            if (!schemaTypeByKey.   TryAdd(key,                     scriptType)) {
                DuplicateComponentKey(scriptType);
            } 
            scriptTypeByType.Add    (scriptType.Type,               scriptType);
            scripts                 [scriptType.ScriptIndex] =      scriptType;
        }
        foreach (var tagType in tagList) {
            var name = tagType.TagName;
            if (!tagTypeByName.     TryAdd(name,                    tagType)) {
                DuplicateTagName(tagType);
            }
            tagTypeByType.Add       (tagType.Type,                  tagType);
            tags                    [tagType.TagIndex] =            tagType;
        }
    }
    
    private static void DuplicateComponentKey(SchemaType schemaType)
    {
        var msg = $"warning: Duplicate component name: '{schemaType.ComponentKey}' for: {schemaType.Type.FullName}. Add unique [ComponentKey()] attribute.";
        Console.WriteLine(msg);
    }
    
    private static void DuplicateTagName(TagType tagType)
    {
        var msg = $"warning: Duplicate tag name: '{tagType.TagName}' for: {tagType.Type.FullName}. Add unique [TagName()] attribute.";
        Console.WriteLine(msg);
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
        where T : Script, new()
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
    /// Ensures <see cref="StructHeap.structIndex"/> and <see cref="StructInfo{T}.Index"/> is less than <see cref="maxStructIndex"/><br/>
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
    
    private string GetString() {
        return $"components: {components.Length - 1}  scripts: {scripts.Length - 1}  entity tags: {tags.Length - 1}";
    } 
    #endregion
}

    
public readonly struct EngineDependant
{
                    public  ReadOnlySpan<SchemaType>    Types           => new (types);
                    public              Assembly        Assembly        => assembly;
                    public              string          AssemblyName    => assembly.GetName().Name;
    
    [Browse(Never)] private readonly    Assembly        assembly;
    [Browse(Never)] private readonly    SchemaType[]    types;

    public override                     string          ToString()  => AssemblyName;

    internal EngineDependant(Assembly assembly, List<SchemaType> types) {
        this.assembly   = assembly;
        this.types      = types.ToArray();
    }
}

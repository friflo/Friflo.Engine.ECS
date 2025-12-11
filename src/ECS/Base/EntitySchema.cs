// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS.Index;
using Friflo.Engine.ECS.Relations;
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
    public   ReadOnlySpan<ComponentType>                Components          => new (components, 0, maxStructIndex);
    /// <summary> Return all <see cref="Script"/> types - classes extending <see cref="Script"/></summary>
    /// <remarks>
    /// <see cref="ScriptType.ScriptIndex"/> is equal to the array index<br/>
    /// <see cref="Scripts"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<ScriptType>                   Scripts             => new (scripts, 0, maxScriptIndex);
    /// <summary> Return all <b>Tag</b> types - structs implementing <see cref="ITag"/>. </summary>
    /// <remarks>
    /// <see cref="TagType.TagIndex"/> is equal to the array index<br/>
    /// <see cref="Tags"/>[0] is always null
    /// </remarks>
    public   ReadOnlySpan<TagType>                      Tags                => new (tags, 0, maxTagIndex);
    
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
    
    public   ComponentTypes                             ComponentTypes      => componentTypes;
    
    public   ComponentTypes                             RelationTypes       => relationTypes;
    
    public   override string                            ToString()          => GetString();

    #endregion
    
#region private fields
    [Browse(Never)] private             EngineDependant[]                   engineDependants;
    [Browse(Never)] internal            int                                 maxStructIndex;
    [Browse(Never)] internal            int                                 maxIndexedStructIndex; // :)
    [Browse(Never)] internal            int                                 maxScriptIndex;
    [Browse(Never)] internal            int                                 maxTagIndex;
    [Browse(Never)] internal            ComponentType[]                     components;
    [Browse(Never)] internal            ScriptType[]                        scripts;
    [Browse(Never)] internal            TagType[]                           tags;
    [Browse(Never)] internal readonly   ComponentType                       unresolvedType;
    // --- lookup: component / script
    [Browse(Never)] internal readonly   Dictionary<string, SchemaType>      schemaTypeByKey;
    [Browse(Never)] internal readonly   Dictionary<Type,   ScriptType>      scriptTypeByType;
    [Browse(Never)] private  readonly   Dictionary<Type,   ComponentType>   componentTypeByType;
    // --- lookup: tags
    [Browse(Never)] internal readonly   Dictionary<string, TagType>         tagTypeByName;
    [Browse(Never)] private  readonly   Dictionary<Type,   TagType>         tagTypeByType;
    // --- component type masks
    [Browse(Never)] internal            ComponentTypes                      componentTypes;
    [Browse(Never)] internal            ComponentTypes                      relationTypes;
    [Browse(Never)] internal            ComponentTypes                      indexTypes;
    [Browse(Never)] internal            ComponentTypes                      linkComponentTypes;
    [Browse(Never)] internal            ComponentTypes                      linkRelationTypes;
    // --- for dynamic registration
    [Browse(Never)] private  readonly   object                              schemaLock = new();
    [Browse(Never)] internal const      int                                 InitialCapacity = 256; // Pre-allocate space for dynamic types
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
        
        // Use larger initial capacity to support dynamic registration
        var componentCapacity   = Math.Max(componentList.Count + 1, InitialCapacity);
        var scriptCapacity      = Math.Max(scriptList.Count + 1, InitialCapacity);
        var tagCapacity         = Math.Max(tagList.Count + 1, InitialCapacity);
        
        maxStructIndex          = componentList.Count + 1;
        maxScriptIndex          = scriptList.Count + 1;
        maxTagIndex             = tagList.Count + 1;
        components              = new ComponentType[componentCapacity];
        scripts                 = new ScriptType[scriptCapacity];
        tags                    = new TagType[tagCapacity];

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
            RuntimeTypeRegistry.MarkTypeRegistered(componentType.Type);
            if (componentType.RelationType == null) {
                componentTypes.Add(new ComponentTypes(componentType));
            } else {
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
            RuntimeTypeRegistry.MarkTypeRegistered(scriptType.Type);
        }
        foreach (var tagType in tagList) {
            var name = tagType.TagName;
            if (!tagTypeByName.     TryAdd(name,                    tagType)) {
                DuplicateTagName(tagType);
            }
            tagTypeByType.Add       (tagType.Type,                  tagType);
            tags                    [tagType.TagIndex] =            tagType;
            RuntimeTypeRegistry.MarkTypeRegistered(tagType.Type);
        }
        CreateNameSortIndexes();
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
    /// Adds a type to the schema at runtime. Called by <see cref="RuntimeTypeRegistry"/> when registering
    /// types after the schema has been created.
    /// </summary>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Dynamic registration uses reflection")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Dynamic registration uses reflection")]
    internal void AddTypeRuntime(Type type, SchemaTypeKind kind)
    {
        lock (schemaLock)
        {
            switch (kind)
            {
                case SchemaTypeKind.Component:
                    if (componentTypeByType.ContainsKey(type))
                        return; // Already registered
                    AddComponentTypeRuntime(type);
                    break;
                case SchemaTypeKind.Tag:
                    if (tagTypeByType.ContainsKey(type))
                        return; // Already registered
                    AddTagTypeRuntime(type);
                    break;
                case SchemaTypeKind.Script:
                    if (scriptTypeByType.ContainsKey(type))
                        return; // Already registered
                    AddScriptTypeRuntime(type);
                    break;
            }
        }
    }
    
    private const BindingFlags Flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Dynamic registration uses reflection")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Dynamic registration uses reflection")]
    private void AddComponentTypeRuntime(Type type)
    {
        var structIndex = maxStructIndex;
        
        // Ensure array capacity
        if (structIndex >= components.Length)
        {
            var newCapacity = components.Length * 2;
            var newComponents = new ComponentType[newCapacity];
            Array.Copy(components, newComponents, components.Length);
            components = newComponents;
            
            // Update the static DefaultHeapMap as well
            EntityStoreBase.Static.ResizeDefaultHeapMap(newCapacity);
        }
        
        // Determine if this is a relation type
        var relationType = RelationTypeUtils.GetEntityRelationsType(type, out Type keyType);
        var indexType = ComponentIndexUtils.GetIndexType(type, out var indexValueType);
        
        ComponentType componentType;
        if (relationType != null)
        {
            // It's a relation type
            var createParams = new object[] { structIndex, relationType, keyType };
            var method = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateRelationType), Flags);
            var genericMethod = method!.MakeGenericMethod(type);
            componentType = (ComponentType)genericMethod.Invoke(null, createParams);
        }
        else
        {
            // Regular component
            var createParams = new object[] { structIndex, indexType, indexValueType };
            var method = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateComponentType), Flags);
            var genericMethod = method!.MakeGenericMethod(type);
            componentType = (ComponentType)genericMethod.Invoke(null, createParams);
        }
        
        // Add to schema
        var key = componentType.ComponentKey;
        if (key != null)
        {
            if (!schemaTypeByKey.TryAdd(key, componentType))
            {
                DuplicateComponentKey(componentType);
            }
        }
        componentTypeByType.Add(componentType.Type, componentType);
        components[structIndex] = componentType;
        maxStructIndex++;
        
        // Update component type masks
        if (componentType.RelationType == null)
        {
            componentTypes.Add(new ComponentTypes(componentType));
        }
        else
        {
            relationTypes.Add(new ComponentTypes(componentType));
            if (componentType.RelationKeyType == typeof(Entity))
            {
                linkRelationTypes.Add(new ComponentTypes(componentType));
            }
        }
        if (componentType.IndexType != null)
        {
            indexTypes.Add(new ComponentTypes(componentType));
            if (componentType.IndexValueType == typeof(Entity))
            {
                linkComponentTypes.Add(new ComponentTypes(componentType));
            }
        }
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Dynamic registration uses reflection")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Dynamic registration uses reflection")]
    private void AddTagTypeRuntime(Type type)
    {
        var tagIndex = maxTagIndex;
        
        // Ensure array capacity
        if (tagIndex >= tags.Length)
        {
            var newCapacity = tags.Length * 2;
            var newTags = new TagType[newCapacity];
            Array.Copy(tags, newTags, tags.Length);
            tags = newTags;
        }
        
        var createParams = new object[] { tagIndex };
        var method = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateTagType), Flags);
        var genericMethod = method!.MakeGenericMethod(type);
        var tagType = (TagType)genericMethod.Invoke(null, createParams);
        
        var name = tagType.TagName;
        if (!tagTypeByName.TryAdd(name, tagType))
        {
            DuplicateTagName(tagType);
        }
        tagTypeByType.Add(tagType.Type, tagType);
        tags[tagIndex] = tagType;
        maxTagIndex++;
    }
    
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Dynamic registration uses reflection")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Dynamic registration uses reflection")]
    private void AddScriptTypeRuntime(Type type)
    {
        var scriptIndex = maxScriptIndex;
        
        // Ensure array capacity
        if (scriptIndex >= scripts.Length)
        {
            var newCapacity = scripts.Length * 2;
            var newScripts = new ScriptType[newCapacity];
            Array.Copy(scripts, newScripts, scripts.Length);
            scripts = newScripts;
        }
        
        var createParams = new object[] { scriptIndex };
        var method = typeof(SchemaUtils).GetMethod(nameof(SchemaUtils.CreateScriptType), Flags);
        var genericMethod = method!.MakeGenericMethod(type);
        var scriptType = (ScriptType)genericMethod.Invoke(null, createParams);
        
        var key = scriptType.ComponentKey;
        if (!schemaTypeByKey.TryAdd(key, scriptType))
        {
            DuplicateComponentKey(scriptType);
        }
        scriptTypeByType.Add(scriptType.Type, scriptType);
        scripts[scriptIndex] = scriptType;
        maxScriptIndex++;
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
    
    public ComponentType GetRelationType<T>()
        where T : struct, IRelation
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
        return $"components: {maxStructIndex - 1}  scripts: {maxScriptIndex - 1}  entity tags: {maxTagIndex - 1}";
    }
    
    private void CreateNameSortIndexes()
    {
        // --- ComponentType
        var componentArray  = components;
        var entries         = new SortIndexEntry[maxStructIndex - 1];
        for (int i = 1; i < maxStructIndex; i++) {
            entries[i - 1] = new SortIndexEntry { index = i, name = componentArray[i].Name };
        }
        Array.Sort(entries);
        for (int i = 0; i < entries.Length; i++) {
            componentArray[entries[i].index].nameSortOrder = i;
        }
        // --- TagType
        var tagsArray   = tags;
        entries         = new SortIndexEntry[maxTagIndex - 1];
        for (int i = 1; i < maxTagIndex; i++) {
            entries[i - 1] = new SortIndexEntry { index = i, name = tagsArray[i].Name };
        }
        Array.Sort(entries);
        for (int i = 0; i < entries.Length; i++) {
            tagsArray[entries[i].index].nameSortOrder = i;
        }
    }
    
    struct SortIndexEntry : IComparable<SortIndexEntry>
    {
        internal int        index;
        internal string     name;

        public override string ToString() => $"{name} index: {index}";

        public int CompareTo(SortIndexEntry other) {
            return string.Compare(name, other.name, StringComparison.Ordinal);
        }
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

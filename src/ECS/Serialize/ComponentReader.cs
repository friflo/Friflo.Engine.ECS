// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Utils;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
// ReSharper disable InlineTemporaryVariable
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Friflo.Engine.ECS.Serialize;

/// <summary>
/// Create all components / scripts for an entity from <see cref="JsonValue"/> used as <see cref="DataEntity.components"/>
/// </summary>
internal sealed class ComponentReader
{
    internal readonly   ObjectReader                            componentReader;
    private readonly    MapperContextEntityStore                mapperContextStore;
    private readonly    Dictionary<string, SchemaType>          schemaTypeByKey;
    private readonly    Dictionary<Type,   ScriptType>          scriptTypeByType;
    private readonly    Dictionary<string, TagType>             tagTypeByName;
    private readonly    ComponentType                           unresolvedType;
    private readonly    HashSet<ScriptType>                     scriptTypes;
    private readonly    ArchetypeKey                            searchKey;
    private readonly    List<string>                            unresolvedTagList;
    private readonly    HashSet<string>                         unresolvedTagSet;
    private readonly    List<UnresolvedComponent>               unresolvedComponentList;
    private readonly    Dictionary<string, UnresolvedComponent> unresolvedComponentMap;
    private readonly    Dictionary<BytesHash, RawKey>           rawKeyCache;
    private             Utf8JsonParser                          parser;
    private             RawComponent[]                          components;
    private             int                                     componentCount;
    private             RawRelation[]                           relations;
    private             int                                     relationCount;
    
    
    internal ComponentReader(TypeStore typeStore) {
        components              = new RawComponent[1];
        relations               = Array.Empty<RawRelation>();
        componentReader         = new ObjectReader(typeStore) { ErrorHandler = ObjectReader.NoThrow };
        mapperContextStore      = new MapperContextEntityStore();
        componentReader.SetMapperContext(mapperContextStore);
        var schema              = EntityStoreBase.Static.EntitySchema;
        unresolvedType          = schema.unresolvedType;
        schemaTypeByKey         = schema.schemaTypeByKey;
        scriptTypeByType        = schema.scriptTypeByType;
        tagTypeByName           = schema.tagTypeByName;
        scriptTypes             = new HashSet<ScriptType>(); // Cannot use Script. User code may override Equals() or GetHashCode()
        searchKey               = new ArchetypeKey();
        unresolvedTagList       = new List<string>();
        unresolvedTagSet        = new HashSet<string>();
        unresolvedComponentList = new List<UnresolvedComponent>();
        unresolvedComponentMap  = new Dictionary<string, UnresolvedComponent>();
        rawKeyCache             = new Dictionary<BytesHash, RawKey>(BytesHash.Equality);
    }
    
    internal string Read(DataEntity dataEntity, Entity entity, EntityStoreBase store, in ConvertOptions options)
    {
        mapperContextStore.store = (EntityStore)store;
        componentCount = 0;
        relationCount  = 0;
        var error = ReadRaw(dataEntity, entity);
        if (error != null) {
            return error;
        }
        SetEntityArchetype(dataEntity, entity, store, options);
        return ReadComponents(entity);
    }
    
    private string ReadRaw (DataEntity dataEntity, Entity entity)
    {
        parser.InitParser(dataEntity.components);
        var ev = parser.NextEvent();
        switch (ev)
        {
            case JsonEvent.Error:
                var error = parser.error.GetMessage();
                return $"{error}. id: {entity.Id}";
            case JsonEvent.ValueNull:
                break;
            case JsonEvent.ObjectStart:
                error = ReadRawComponents(entity);
                if (error != null) {
                    // could support also scalar types in the future: string, number or boolean
                    return error;
                }
                break;
            default:
                return $"expect 'components' == object, array or null. id: {entity.Id}. was: {ev}";
        }
        return null;
    }
    
    private string ReadComponents(Entity entity)
    {
        unresolvedComponentList.Clear();
        scriptTypes.Clear();
        foreach (var script in entity.Scripts) {
            var scriptType = scriptTypeByType[script.GetType()];
            scriptTypes.Add(scriptType);
        }
        for (int n = 0; n < componentCount; n++)
        {
            ref var component = ref components[n];
            string error = null;
            switch (component.type) {
                case RawComponentType.Object:
                    error = ReadComponent(entity, component);
                    break;
                case RawComponentType.Array:
                    error = ReadRelations(entity, component);
                    break;
            }
            if (error != null) {
                return error;
            }
        }
        // --- remove missing scripts from entity
        foreach (var scriptType in scriptTypes) {
            EntityUtils.RemoveEntityScript(entity, scriptType);
        }
        // --- add unresolved components
        if (unresolvedComponentList.Count > 0 ) {
            AddUnresolvedComponents(entity);
        }
        return null;
    }
    
    private string ReadComponent(Entity entity, in RawComponent component)
    {
        var json        = new JsonValue(parser.GetInputBytes(component.start - 1, component.end));
        var schemaType  = component.rawKey.schemaType;
        if (schemaType == unresolvedType) {
            unresolvedComponentList.Add(new UnresolvedComponent(component.rawKey.key, json));
            return null;
        }
        switch (schemaType.Kind) {
            case SchemaTypeKind.Script:
                // --- read script
                var scriptType = (ScriptType)schemaType;
                scriptTypes.Remove(scriptType);
                scriptType.ReadScript(componentReader, json, entity);
                break;
            case SchemaTypeKind.Component:
                var componentType   = (ComponentType)schemaType;
                var heap            = entity.archetype.heapMap[componentType.StructIndex]; // no range or null check required
                // --- read & change component
                heap.Read(componentReader, entity.compIndex, json);
                break;
        }
        if (componentReader.Error.ErrSet) {
            return ReadComponentError(component);
        }
        return null;
    }
    
    private string ReadRelations(Entity entity, in RawComponent component)
    {
        var relationType  = (ComponentType)component.rawKey.schemaType;
        for (int index = component.start; index < component.end; index++) {
            var relation = relations[index];
            var json     = new JsonValue(parser.GetInputBytes(relation.start - 1, relation.end));
            relationType.ReadRelation(this, entity, json);
            if (componentReader.Error.ErrSet) {
                return ReadComponentError(component);
            }
        }
        return null;
    }
    
    private string ReadComponentError(in RawComponent component) {
        return $"'components[{component.rawKey.key}]' - {componentReader.Error.GetMessage()}";
    }
    
    private void AddUnresolvedComponents(Entity entity)
    {
        ref var unresolved          = ref entity.GetComponent<Unresolved>();
        var componentList           = unresolvedComponentList;
        var unresolvedComponents    = unresolved.components;
        if (unresolvedComponents == null) {
            unresolved.components = new UnresolvedComponent[componentList.Count];
            componentList.CopyTo(unresolved.components);
            return;
        }
        var map = unresolvedComponentMap;
        map.Clear();
        foreach (var component in unresolvedComponents) {
            map[component.key] = component;
        }
        foreach (var component in componentList) {
            map[component.key] = component;
        }
        if (unresolvedComponents.Length != map.Count) {
            unresolvedComponents = unresolved.components= new UnresolvedComponent[map.Count];
        }
        int n = 0;
        foreach (var pair in map) {
            unresolvedComponents[n++] = pair.Value;
        }
    }
    
    /// <summary>
    /// Ensures the given entity present / moved to an <see cref="Archetype"/> that contains all components 
    /// within the current JSON payload.
    /// </summary>
    private void SetEntityArchetype(DataEntity dataEntity, Entity entity, EntityStoreBase store, in ConvertOptions options)
    {
        var key = searchKey;
        key.Clear();
        GetComponentTypes(ref key.componentTypes);
        var tags = dataEntity.tags;
        unresolvedTagList.Clear();
        if (tags?.Count > 0) {
            AddTags(tags, key);
        }
        var curType = entity.archetype;
        if (options.preserve) {
            Preserve(key, curType, options);
        }
        key.CalculateHashCode();
        
        // --- use / create Archetype with present components to eliminate structural changes for every individual component Read()
        var newType = FindArchetype(key, store);
        if (curType != newType)
        {
            ref var node    = ref entity.store.nodes[entity.Id];
            node.archetype  = newType;
            node.compIndex  = Archetype.MoveEntityTo(curType, entity.Id, node.compIndex, newType);
        }
        if (unresolvedTagList.Count > 0) {
            AddUnresolvedTags(entity);
        }
    }
    
    private void AddUnresolvedTags(Entity entity)
    {
        ref var unresolved = ref entity.GetComponent<Unresolved>();
        var tags    = unresolved.tags;
        var tagList = unresolvedTagList;
        if (tags == null) {
            tags = unresolved.tags = new string[tagList.Count];
            int n = 0;
            foreach (var tag in tagList) {
                tags[n++] = tag;
            }
            return;
        }
        var set = unresolvedTagSet;
        set.Clear();
        foreach (var tag in tags) {
            set.Add(tag);   
        }
        foreach (var tag in tagList) {
            set.Add(tag);   
        }
        if (tags.Length != set.Count) {
            tags = unresolved.tags = new string[set.Count];
        }
        int i = 0;
        foreach (var tag in set) {
            tags[i++] = tag;
        }
    }
    
    private void GetComponentTypes(ref ComponentTypes componentTypes)
    {
        var count = componentCount;
        for (int n = 0; n < count; n++)
        {
            ref var component   = ref components[n];
            var schemaType      = component.rawKey.schemaType;
            if (schemaType == unresolvedType) {
                // case: unresolved component
                componentTypes.bitSet.SetBit(unresolvedType.StructIndex);
                continue;
            }
            if (schemaType.Kind == SchemaTypeKind.Component)
            {
                var componentType = (ComponentType)schemaType;
                componentTypes.bitSet.SetBit(componentType.StructIndex);
            }                
        }
    }
    
    /// <summary> Preserve components and tags present on passed <paramref name="type"/>. </summary>
    private static void Preserve(ArchetypeKey key, Archetype type, in ConvertOptions options)
    {
        key.componentTypes.bitSet.Add(BitSet.Intersect(type.componentTypes.bitSet, options.preserveComponents.bitSet));
        key.tags.          bitSet.Add(BitSet.Intersect(type.tags.          bitSet, options.preserveTags.      bitSet));
    }
    
    private static Archetype FindArchetype(ArchetypeKey searchKey, EntityStoreBase store)
    {
        if (store.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config          = EntityStoreBase.GetArchetypeConfig(store);
        var newArchetype    = Archetype.CreateWithComponentTypes(config, searchKey.componentTypes, searchKey.tags);
        EntityStoreBase.AddArchetype(store, newArchetype);
        return newArchetype;
    }
    
    private string ReadRawComponents(Entity entity)
    {
        var ev = parser.NextEvent();
        while (true) {
            switch (ev) {
                case JsonEvent.ObjectStart:
                    var rawKey  = ToRawKey(parser.key);
                    var start   = parser.Position;
                    parser.SkipTree();
                    if (componentCount == components.Length) {
                        ArrayUtils.Resize(ref components, 2 * componentCount);
                    }
                    components[componentCount++] = new RawComponent(RawComponentType.Object, rawKey, start, parser.Position);
                    ev = parser.NextEvent();
                    break;
                case JsonEvent.ObjectEnd:
                    return null;
                case JsonEvent.ArrayStart:
                    return ReadRawRelations(entity);
                default:
                    return $"'components' member must be object or array. was {ev}. id: {entity.Id}, component: '{parser.key}'";
            }
        }
    }
    
    private string ReadRawRelations(Entity entity)
    {
        var rawKey          = ToRawKey(parser.key);
        var startRelation   = relationCount;
        var ev = parser.NextEvent();
        while (true) {
            switch (ev) {
                case JsonEvent.ObjectStart:
                    var start   = parser.Position;
                    parser.SkipTree();
                    if (relationCount == relations.Length) {
                        ArrayUtils.Resize(ref relations, Math.Max(4, 2 * relationCount));
                    }
                    relations[relationCount++] = new RawRelation(start, parser.Position);
                    ev = parser.NextEvent();
                    if (ev == JsonEvent.ArrayEnd) {
                        if (componentCount == components.Length) {
                            ArrayUtils.Resize(ref components, 2 * componentCount);
                        }
                        components[componentCount++] = new RawComponent(RawComponentType.Array, rawKey, startRelation, relationCount);
                        return null;
                    }
                    break;
                case JsonEvent.ArrayEnd:
                    return null;
                default:
                    return $"'components' member expect array of objects. was {ev}. id: {entity.Id}, component: '{parser.key}'";
            }
        }
    }
    
    private RawKey ToRawKey(in Bytes keyBytes)
    {
        var keyHash = new BytesHash(keyBytes);
        if (rawKeyCache.TryGetValue(keyHash, out RawKey rawKey)) {
            return rawKey;
        }
        var key = keyBytes.AsString();
        if (schemaTypeByKey.TryGetValue(key, out var schemaType)) {
            rawKey  = new RawKey(key, schemaType);
        } else {
            rawKey  = new RawKey(key, unresolvedType);
        }
        var bytesCopy = new Bytes(keyBytes);    // must create copy - given key Bytes will be mutated
        rawKeyCache.Add(new BytesHash(bytesCopy), rawKey);
        return rawKey;
    }
    
    private void AddTags(List<string> tagList, ArchetypeKey archetypeKey)
    {
        foreach (var tag in tagList) {
            if (!tagTypeByName.TryGetValue(tag, out var tagType)) {
                archetypeKey.componentTypes.bitSet.SetBit(unresolvedType.StructIndex);
                unresolvedTagList.Add(tag);
                continue;
            }
            archetypeKey.tags.bitSet.SetBit(tagType.TagIndex);
        }
    }
}

internal readonly struct RawKey
{
    internal  readonly  string      key;            // never null
    internal  readonly  SchemaType  schemaType;     // never null

    public    override  string      ToString() => $"{key} - {schemaType.Name}";
    
    internal RawKey(string key, SchemaType schemaType) {
        this.key        = key;
        this.schemaType = schemaType;
    }
}

internal enum RawComponentType
{
    Object,
    Array
}

internal readonly struct RawComponent
{
    internal  readonly  RawComponentType    type;
    internal  readonly  RawKey              rawKey;
    internal  readonly  int                 start;
    internal  readonly  int                 end;

    public    override  string      ToString() => rawKey.ToString();
    
    internal RawComponent(RawComponentType type, in RawKey rawKey, int start, int end) {
        this.type   = type;
        this.rawKey = rawKey;
        this.start  = start;
        this.end    = end;
    }
}

internal readonly struct RawRelation
{
    internal  readonly  int         start;
    internal  readonly  int         end;
    
    internal RawRelation(int start, int end) {
        this.start  = start;
        this.end    = end;
    }
}
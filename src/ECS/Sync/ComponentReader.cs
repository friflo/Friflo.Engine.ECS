// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Friflo.Fliox.Engine.ECS.Sync;

/// <summary>
/// Create all components / scripts for an entity from <see cref="JsonValue"/> used as <see cref="DataEntity.components"/>
/// </summary>
internal sealed class ComponentReader
{
    private readonly    ObjectReader                            componentReader;
    private readonly    Dictionary<string, ComponentType>       componentTypeByKey;
    private readonly    Dictionary<string, ComponentType>       tagTypeByName;
    private readonly    ComponentType                           unresolvedType;
    private readonly    List<ComponentType>                     structTypes;
    private readonly    ArchetypeKey                            searchKey;
    private readonly    List<string>                            unresolvedTagList;
    private readonly    HashSet<string>                         unresolvedTagSet;
    private readonly    List<UnresolvedComponent>               unresolvedComponentList;
    private readonly    Dictionary<string, UnresolvedComponent> unresolvedComponentMap;
    private             Utf8JsonParser                          parser;
    private             Bytes                                   buffer;
    private             RawComponent[]                          components;
    private             int                                     componentCount;
    
    
    internal ComponentReader() {
        buffer                  = new Bytes(128);
        components              = new RawComponent[1];
        componentReader         = new ObjectReader(EntityStore.Static.TypeStore);
        var schema              = EntityStore.Static.ComponentSchema;
        unresolvedType          = schema.unresolvedType;
        componentTypeByKey      = schema.componentTypeByKey;
        tagTypeByName           = schema.tagTypeByName;
        structTypes             = new List<ComponentType>();
        searchKey               = new ArchetypeKey();
        unresolvedTagList       = new List<string>();
        unresolvedTagSet        = new HashSet<string>();
        unresolvedComponentList = new List<UnresolvedComponent>();
        unresolvedComponentMap  = new Dictionary<string, UnresolvedComponent>();
    }
    
    internal string Read(DataEntity dataEntity, GameEntity entity, EntityStore store)
    {
        componentCount      = 0;
        var hasTags         = dataEntity.tags?.Count > 0;
        var hasComponents   = !dataEntity.components.IsNull();
        if (!hasComponents && !hasTags) {
            return null;
        }
        var error = ReadRaw(dataEntity, entity);
        if (error != null) {
            return error;
        }
        SetEntityArchetype(dataEntity, entity, store);
        ReadComponents(entity);
        return null;
    }
    
    private string ReadRaw (DataEntity dataEntity, GameEntity entity)
    {
        parser.InitParser(dataEntity.components);
        var ev = parser.NextEvent();
        switch (ev)
        {
            case JsonEvent.Error:
                var error = parser.error.GetMessage();
                return $"{error}. id: {entity.id}";
            case JsonEvent.ValueNull:
                break;
            case JsonEvent.ObjectStart:
                ev = ReadRawComponents();
                if (ev != JsonEvent.ObjectEnd) {
                    // could support also scalar types in future: string, number or boolean
                    return $"component must be an object. was {ev}. id: {entity.id}, component: '{parser.key}'";
                }
                break;
            default:
                return $"expect 'components' == object or null. id: {entity.id}. was: {ev}";
        }
        return null;
    }
    
    private void ReadComponents(GameEntity entity)
    {
        unresolvedComponentList.Clear();
        for (int n = 0; n < componentCount; n++)
        {
            var component = components[n];
            buffer.Clear();
            var json = new JsonValue(parser.GetInputBytes(component.start - 1, component.end));
            var type = component.type;
            if (type == unresolvedType) {
                unresolvedComponentList.Add(new UnresolvedComponent(component.key, json));
                continue;
            }
            switch (type.kind) {
                case ComponentKind.Script:
                    // --- read script
                    component.type.ReadScript(componentReader, json, entity);
                    break;
                case ComponentKind.Component:
                    var heap = entity.archetype.heapMap[component.type.structIndex]; // no range or null check required
                    // --- read & change component
                    heap.Read(componentReader, entity.compIndex, json);
                    break;
            }
        }
        if (unresolvedComponentList.Count > 0 ) {
            AddUnresolvedComponents(entity);
        }
    }
    
    private void AddUnresolvedComponents(GameEntity entity)
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
    private void SetEntityArchetype(DataEntity dataEntity, GameEntity entity, EntityStore store)
    {
        searchKey.Clear();
        var hasStructComponent  = GetStructComponents(ref searchKey.structs);
        var tags                = dataEntity.tags;
        var hasTags             = tags?.Count > 0;
        if (!hasStructComponent && !hasTags) {
            return; // early out in absence of components and tags
        }
        unresolvedTagList.Clear();
        if (hasTags) {
            AddTags(tags, searchKey);
        }
        searchKey.CalculateHashCode();
        // --- use / create Archetype with present components to eliminate structural changes for every individual component Read()
        var newArchetype = FindArchetype(searchKey, store);
        
        var curArchetype = entity.archetype;
        if (curArchetype != newArchetype)
        {
            entity.archetype = newArchetype;
            if (curArchetype == store.defaultArchetype) {
                entity.compIndex = newArchetype.AddEntity(entity.id);
            } else {
                entity.compIndex = curArchetype.MoveEntityTo(entity.id, entity.compIndex, newArchetype);
            }
        }
        if (unresolvedTagList.Count > 0) {
            AddUnresolvedTags(entity);
        }
    }
    
    private void AddUnresolvedTags(GameEntity entity)
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
    
    private bool GetStructComponents(ref ArchetypeStructs structs)
    {
        var hasStructComponent  = false;
        var count               = componentCount;
        for (int n = 0; n < count; n++)
        {
            ref var component   = ref components[n];
            componentTypeByKey.TryGetValue(component.key, out var type);
            if (type == null) {
                // case: unresolved component
                hasStructComponent = true;
                structs.SetBit(unresolvedType.structIndex);
                component.type = unresolvedType;
                continue;
            }
            component.type = type;
            if (type.kind == ComponentKind.Component) {
                hasStructComponent = true;
                structs.SetBit(type.structIndex);
            }                
        }
        return hasStructComponent;
    }
    
    private Archetype FindArchetype(ArchetypeKey searchKey, EntityStore store)
    {
        if (store.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config = store.GetArchetypeConfig();
        structTypes.Clear();
        for (int n = 0; n < componentCount; n++) {
            ref var component = ref components[n];
            if (component.type.kind == ComponentKind.Component) {
                structTypes.Add(component.type);
            }
        }
        if (unresolvedTagList.Count > 0) {
            structTypes.Add(unresolvedType);
        }
        var newArchetype = Archetype.CreateWithStructTypes(config, structTypes, searchKey.tags);
        store.AddArchetype(newArchetype);
        return newArchetype;
    }
    
    private JsonEvent ReadRawComponents()
    {
        var ev = parser.NextEvent();
        while (true) {
            switch (ev) {
                case JsonEvent.ObjectStart:
                    var key     = parser.key.AsString();  // todo remove string allocation
                    var start   = parser.Position;
                    parser.SkipTree();
                    if (componentCount == components.Length) {
                        Utils.Resize(ref components, 2 * componentCount);
                    }
                    components[componentCount++] = new RawComponent(key, start, parser.Position);
                    ev = parser.NextEvent();
                    if (ev == JsonEvent.ObjectEnd) {
                        return JsonEvent.ObjectEnd;
                    }
                    break;
                case JsonEvent.ObjectEnd:
                    return JsonEvent.ObjectEnd;
                default:
                    return ev;
            }
        }
    }
    
    private void AddTags(List<string> tagList, ArchetypeKey archetypeKey)
    {
        foreach (var tag in tagList) {
            if (!tagTypeByName.TryGetValue(tag, out var tagType)) {
                archetypeKey.structs.SetBit(unresolvedType.structIndex);
                unresolvedTagList.Add(tag);
                continue;
            }
            archetypeKey.tags.SetBit(tagType.tagIndex);
        }
    }
}

internal struct RawComponent
{
    internal  readonly  string          key;
    internal  readonly  int             start;
    internal  readonly  int             end;
    /// <summary>Is set when looking up components in <see cref="ComponentSchema.componentTypeByKey"/></summary>
    internal            ComponentType   type; 

    public    override  string          ToString() => key;
    
    internal RawComponent(string key, int start, int end) {
        this.key    = key;
        this.start  = start;
        this.end    = end;
    }
}
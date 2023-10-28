// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using static Friflo.Fliox.Engine.ECS.ComponentKind;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Friflo.Fliox.Engine.ECS.Sync;

/// <summary>
/// Create all components / behaviors for an entity from <see cref="JsonValue"/> used as <see cref="DataEntity.components"/>
/// </summary>
internal sealed class ComponentReader
{
    private readonly    ObjectReader                        componentReader;
    private readonly    Dictionary<string, ComponentType>   componentTypeByKey;
    private readonly    Dictionary<string, ComponentType>   tagTypeByName;
    private readonly    List<ComponentType>                 structTypes;
    private readonly    ArchetypeKey                        searchKey;
    private             Utf8JsonParser                      parser;
    private             Bytes                               buffer;
    private             RawComponent[]                      components;
    private             int                                 componentCount;
    
    
    internal ComponentReader() {
        buffer              = new Bytes(128);
        components          = new RawComponent[1];
        componentReader     = new ObjectReader(EntityStore.Static.TypeStore);
        var schema          = EntityStore.Static.ComponentSchema;
        componentTypeByKey  = schema.componentTypeByKey;
        tagTypeByName       = schema.tagTypeByName;
        structTypes         = new List<ComponentType>();
        searchKey           = new ArchetypeKey();
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
        for (int n = 0; n < componentCount; n++)
        {
            var component = components[n];
            buffer.Clear();
            var json = new JsonValue(parser.GetInputBytes(component.start - 1, component.end));
            var type = component.type;
            switch (type.kind) {
                case ComponentKind.Behavior:
                    // --- read behavior
                    component.type.ReadBehavior(componentReader, json, entity);
                    break;
                case Component:
                    var heap = entity.archetype.heapMap[component.type.structIndex]; // no range or null check required
                    // --- read & change component
                    heap.Read(componentReader, entity.compIndex, json);
                    break;
            }
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
        var hasTags = tags?.Count > 0;
        if (!hasStructComponent && !hasTags) {
            return; // early out in absence of components and tags
        }
        if (hasTags) {
            AddTags(tags, ref searchKey.tags);
        }
        searchKey.CalculateHashCode();
        // --- use / create Archetype with present components to eliminate structural changes for every individual component Read()
        var newArchetype = FindArchetype(searchKey, store);
        
        var curArchetype = entity.archetype;
        if (curArchetype == newArchetype) {
            return;
        }
        entity.archetype = newArchetype;
        if (curArchetype == store.defaultArchetype) {
            entity.compIndex = newArchetype.AddEntity(entity.id);
        } else {
            entity.compIndex = curArchetype.MoveEntityTo(entity.id, entity.compIndex, newArchetype);
        }
    }
    
    private bool GetStructComponents(ref ArchetypeStructs structs)
    {
        var hasStructComponent  = false;
        var count               = componentCount;
        for (int n = 0; n < count; n++)
        {
            ref var component   = ref components[n];
            var type            = componentTypeByKey[component.key];
            component.type      = type;
            if (type.kind != Component) {
                continue;
            }
            hasStructComponent = true;
            structs.SetBit(type.structIndex);
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
            if (component.type.kind == Component) {
                structTypes.Add(component.type);
            }
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
    
    private void AddTags(List<string> tagList, ref Tags tags)
    {
        foreach (var tag in tagList) {
            var tagType = tagTypeByName[tag];
            tags.SetBit(tagType.tagIndex);
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
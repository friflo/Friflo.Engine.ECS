// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using static Friflo.Fliox.Engine.ECS.ComponentKind;

namespace Friflo.Fliox.Engine.Client;

/// <summary>
/// Create all class / struct components for an entity from <see cref="JsonValue"/> used as <see cref="DataNode.components"/>
/// </summary>
internal sealed class ComponentReader
{
    private readonly    ObjectReader                        componentReader;
    private readonly    Dictionary<string, ComponentType>   componentTypeByKey;
    private readonly    List<ComponentType>                 structTypes;
    private readonly    ArchetypeKey                        searchKey;
    private             Utf8JsonParser                      parser;
    private             Bytes                               buffer;
    private             RawComponent[]                      components;
    private             int                                 componentCount;
    
    internal static readonly ComponentReader Instance = new ComponentReader();
    
    private ComponentReader() {
        buffer              = new Bytes(128);
        components          = new RawComponent[1];
        componentReader     = new ObjectReader(EntityStore.Static.TypeStore);
        componentTypeByKey  = EntityStore.Static.ComponentSchema.componentTypeByKey;
        structTypes         = new List<ComponentType>();
        searchKey           = new ArchetypeKey();
    }
    
    internal string Read(DataNode dataNode, GameEntity entity, EntityStore store)
    {
        if (dataNode.components.IsNull()) {
            return null;
        }
        parser.InitParser(dataNode.components);
        var ev = parser.NextEvent();
        if (ev == JsonEvent.Error) {
            var error = parser.error.GetMessage();
            return $"{error}. id: {entity.id}";
        }
        if (ev != JsonEvent.ObjectStart) {
            return $"expect 'components' == object or null. id: {entity.id}";
        }
        ev = ReadRawComponents();
        if (ev != JsonEvent.ObjectEnd) {
            // could support also scalar types in future: string, number or boolean
            return $"component must be an object. was {ev}. id: {entity.id}, component: '{parser.key}'";
        }
        SetEntityArchetype(dataNode, entity, store);
        ReadComponents(entity);
        return null;
    }
    
    private void ReadComponents(GameEntity entity)
    {
        for (int n = 0; n < componentCount; n++)
        {
            var component = components[n];
            buffer.Clear();
            parser.AppendInputSlice(ref buffer, component.start - 1, component.end);
            var json = new JsonValue(buffer);
            var type = component.type;
            switch (type.kind) {
                case Class:
                    // --- read class component
                    component.type.ReadClassComponent(componentReader, json, entity);
                    break;
                case Struct:
                    var heap = entity.archetype.heapMap[component.type.structIndex]; // no range or null check required
                    // --- read & change struct component
                    heap.Read(componentReader, entity.compIndex, json);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Ensures the given entity present / moved to an <see cref="Archetype"/> that contains all struct components 
    /// within the current JSON payload.
    /// </summary>
    private void SetEntityArchetype(DataNode dataNode, GameEntity entity, EntityStore store)
    {
        bool hasStructComponent = false;
        searchKey.Clear();
        var count = componentCount;
        for (int n = 0; n < count; n++)
        {
            ref var component   = ref components[n];
            var type            = componentTypeByKey[component.key];
            component.type      = type;
            if (type.kind != Struct) {
                continue;
            }
            hasStructComponent = true;
            searchKey.structs.SetBit(type.structIndex);
        }
        var tags = dataNode.tags;
        if (!hasStructComponent && (tags == null || tags.Count == 0)) {
            return; // early out in absence of struct components 
        }
        // --- use / create Archetype with present components to eliminate structural changes for every individual component Read()
        var curArchetype = entity.archetype;
        searchKey.CalculateHashCode();
        Archetype newArchetype;
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            newArchetype = archetypeKey.archetype;
        } else {
            var config = store.GetArchetypeConfig();
            structTypes.Clear();
            for (int n = 0; n < count; n++) {
                ref var component = ref components[n];
                if (component.type.kind == Struct) {
                    structTypes.Add(component.type);
                }
            }
            newArchetype = Archetype.CreateWithStructTypes(config, structTypes, curArchetype.tags);
            store.AddArchetype(newArchetype);
        }
        if (curArchetype == newArchetype) {
            return;
        }
        if (curArchetype == store.defaultArchetype) {
            newArchetype.AddEntity(entity.id);
        } else {
            curArchetype.MoveEntityTo(entity.id, entity.compIndex, newArchetype);
        }
        entity.archetype = newArchetype;
    }
    
    private JsonEvent ReadRawComponents()
    {
        componentCount = 0;
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
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

namespace Friflo.Fliox.Engine.Client;

/// <summary>
/// Create all class / struct components for an entity from <see cref="JsonValue"/> used as <see cref="DataNode.components"/>
/// </summary>
internal sealed class ComponentReader
{
    private readonly    ObjectReader                        componentReader;
    private readonly    Dictionary<string, ComponentType>   componentTypes;
    private             SignatureTypes                      structTypes;
    private             Utf8JsonParser                      parser;
    private             Bytes                               buffer;
    private             RawComponent[]                      components;
    private             int                                 componentCount;
    
    internal static readonly ComponentReader Instance = new ComponentReader();
    
    private ComponentReader() {
        buffer          = new Bytes(128);
        components      = new RawComponent[1];
        componentReader = new ObjectReader(EntityStore.Static.TypeStore);
        componentTypes  = new Dictionary<string, ComponentType>(EntityStore.Static.ComponentTypes.ComponentTypeByKey);
    }
    
    internal void Read(JsonValue value, GameEntity entity, EntityStore store)
    {
        if (value.IsNull()) {
            return;
        }
        parser.InitParser(value);
        var ev = parser.NextEvent();
        if (ev == JsonEvent.ValueNull) {
            return;
        }
        if (ev != JsonEvent.ObjectStart) {
            throw new InvalidOperationException("expect object or null");
        }
        ReadRawComponents();
        PrepareReadComponents(entity, store);

        // --- read class / struct components
        var updater = store.gameEntityUpdater;
        for (int n = 0; n < componentCount; n++)
        {
            var component = components[n];
            buffer.Clear();
            parser.AppendInputSlice(ref buffer, component.start - 1, component.end);
            var json = new JsonValue(buffer);
            if (!component.type.isStructType) {
                component.type.ReadClassComponent(componentReader, json, entity);
                continue;
            }
            store.ReadStructComponent(componentReader, json, entity.id, ref entity.archetype, ref entity.compIndex, component.type, updater);
        }
    }
    
    private void PrepareReadComponents(GameEntity entity, EntityStore store)
    {
        long archetypeHash = 0;
        var count = componentCount;
        for (int n = 0; n < count; n++)
        {
            ref var component   = ref components[n];
            var type            = componentTypes[component.key];
            archetypeHash      ^= type.structHash;
            component.type      = type;
        }
        // --- use / create Archetype with present components to avoid structural changes
        if (!store.TryGetArchetype(archetypeHash, out var newArchetype))
        {
            var config  = store.GetArchetypeConfig();
            structTypes.Clear();
            for (int n = 0; n < count; n++) {
                ref var component   = ref components[n];
                if (!component.type.isStructType) {
                    continue;
                }
                structTypes.Add(component.type);
            }
            newArchetype    = Archetype.CreateWithStructTypes(config, structTypes);
            store.AddArchetype(newArchetype);
        }
        if (entity.archetype == newArchetype) {
            return;
        }
        if (entity.archetype == store.defaultArchetype) {
            newArchetype.AddEntity(entity.id);
        } else {
            entity.archetype.MoveEntityTo(entity.id, entity.compIndex, newArchetype, store.gameEntityUpdater);
        }
        entity.archetype = newArchetype;
    }
    
    private void ReadRawComponents()
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
                    components[componentCount++] = new RawComponent { key = key, start = start, end = parser.Position };
                    ev = parser.NextEvent();
                    if (ev == JsonEvent.ObjectEnd) {
                        return;
                    }
                    break;
                case JsonEvent.ObjectEnd:
                    return;
                default:
                    throw new InvalidOperationException($"expect object. was: {ev}");
            }
        }
    }
}

internal struct RawComponent
{
    internal        string          key;
    internal        ComponentType   type;
    internal        int             start;
    internal        int             end;

    public override string          ToString() => key;
}
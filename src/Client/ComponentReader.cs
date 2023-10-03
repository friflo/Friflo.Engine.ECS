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
    private readonly    ObjectReader        componentReader;
    private             Utf8JsonParser      parser;
    private             Bytes               buffer;
    private readonly    List<RawComponent>  components;
    
    internal static readonly ComponentReader Instance = new ComponentReader();
    
    private ComponentReader() {
        buffer          = new Bytes(128);
        components      = new List<RawComponent>();
        componentReader = new ObjectReader(EntityStore.Static.TypeStore);
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
        components.Clear();
        ReadRawComponents();
        foreach (var component in components) {
            buffer.Clear();
            parser.AppendInputSlice(ref buffer, component.startIndex - 1, component.endIndex);
            var json    = new JsonValue(buffer);
            var factory = store.factories[component.key];
            store.ReadComponent(componentReader, json, entity.id, ref entity.archetype, ref entity.compIndex, factory, store.gameEntityUpdater);
        }
    }
    
    private void ReadRawComponents()
    {
        var ev = parser.NextEvent();
        while (true) {
            switch (ev) {
                case JsonEvent.ObjectStart:
                    var key         = parser.key.AsString();  // todo remove heap cause by string creation
                    var component   = new RawComponent { key = key, startIndex = parser.Position };
                    parser.SkipTree();
                    component.endIndex = parser.Position;
                    components.Add(component);
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
    internal string key;
    internal int    startIndex;
    internal int    endIndex;
}
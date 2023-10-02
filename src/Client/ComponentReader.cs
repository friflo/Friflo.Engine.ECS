// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
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
    private readonly    ObjectReader    componentReader;
    private             Utf8JsonParser  parser;
    private             Bytes           keyBuffer;
    private             Bytes           buffer;
    
    internal static readonly ComponentReader Instance = new ComponentReader();
    
    private ComponentReader() {
        keyBuffer       = new Bytes(16);
        buffer          = new Bytes(128);
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
        parser.NextEvent();
        while (true) {
            ev = parser.Event;
            switch (ev) {
                case JsonEvent.ObjectStart:
                    keyBuffer.Clear();
                    keyBuffer.AppendBytes(parser.key);
                    var key     = keyBuffer.AsSpan();
                    var start   = parser.Position;
                    parser.SkipTree();
                    ReadComponent(key, start, entity, store);
                    break;
                case JsonEvent.ObjectEnd:
                    return;
                default:
                    throw new InvalidOperationException($"expect object. was: {ev}");
            }
        }
    }
    
    private void ReadComponent(ReadOnlySpan<byte> keySpan, int start, GameEntity entity, EntityStore store)
    {
        parser.AppendInputSlice(ref buffer, start - 1, parser.Position);
        var json    = new JsonValue(buffer);
        var key     = Encoding.UTF8.GetString(keySpan); // todo remove heap allocation. Currently required for lookup
        var factory = store.factories[key];
        store.ReadComponent(componentReader, json, entity.id, ref entity.archetype, ref entity.compIndex, factory, store.gameEntityUpdater);
    }
}
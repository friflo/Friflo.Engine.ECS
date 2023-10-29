// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

namespace Friflo.Fliox.Engine.ECS.Sync;

/// <summary>
/// Create the <see cref="JsonValue"/> from all components and behaviors used at <see cref="DataEntity.components"/>.<br/>
/// </summary>
internal sealed class ComponentWriter
{
    private readonly    ObjectWriter                    componentWriter;
    private             Utf8JsonWriter                  writer;
    private             Bytes                           buffer;
    private readonly    ComponentType[]                 structTypes;
    private readonly    Dictionary<Type, ComponentType> componentTypeByType;
    private readonly    int                             unresolvedIndex;
    
    internal ComponentWriter() {
        buffer              = new Bytes(128);
        componentWriter     = new ObjectWriter(EntityStore.Static.TypeStore);
        var schema          = EntityStore.Static.ComponentSchema;
        structTypes         = schema.components;
        componentTypeByType = schema.componentTypeByType;
        unresolvedIndex     = schema.unresolvedType.structIndex;
    }
    
    internal JsonValue Write(GameEntity entity, bool pretty)
    {
        var archetype = entity.archetype;
        if (entity.ComponentCount() == 0) {
            return default;
        }
        var componentCount = 0;
        writer.InitSerializer();
        writer.SetPretty(pretty);
        writer.ObjectStart();
        // --- write components
        var heaps           = archetype.Heaps;
        for (int n = 0; n < heaps.Length; n++) {
            var heap        = heaps[n];
            if (heap.structIndex == unresolvedIndex) {
                var unresolved = entity.GetComponent<Unresolved>();
                var components = unresolved.components;
                if (components != null) {
                    foreach (var component in components) {
                        var key     = Encoding.UTF8.GetBytes(component.Key); // todo remove byte[] allocation
                        var raw     = component.Value;
                        var data    = new Bytes { buffer = raw.MutableArray, start = raw.start, end = raw.start + raw.Count };
                        writer.MemberBytes(key, data);
                        componentCount++;
                    }
                }
                continue;
            }
            var value       = heap.Write(componentWriter, entity.compIndex);
            var keyBytes    = structTypes[heap.structIndex].componentKeyBytes; 
            writer.MemberBytes(keyBytes.AsSpan(), value);
            componentCount++;
        }
        // --- write behaviors
        foreach (var behavior in entity.Behaviors) {
            componentWriter.WriteObject(behavior, ref buffer);
            var classType   = componentTypeByType[behavior.GetType()];
            var keyBytes    = classType.componentKeyBytes;
            writer.MemberBytes(keyBytes.AsSpan(), buffer);
            componentCount++;
        }
        if (componentCount == 0) {
            return default;
        }
        writer.ObjectEnd();
        return new JsonValue(writer.json);
    }
}
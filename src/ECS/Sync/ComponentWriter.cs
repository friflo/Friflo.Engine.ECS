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
/// Create the <see cref="JsonValue"/> from all components and scripts used at <see cref="DataEntity.components"/>.<br/>
/// </summary>
internal sealed class ComponentWriter
{
    private  readonly   ObjectWriter                    componentWriter;
    private             Utf8JsonWriter                  writer;
    internal            Bytes                           buffer;
    private  readonly   ComponentType[]                 structTypes;
    private  readonly   Dictionary<Type, ComponentType> componentTypeByType;
    private  readonly   int                             unresolvedIndex;
    
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
        var heaps = archetype.Heaps;
        for (int n = 0; n < heaps.Length; n++) {
            var heap = heaps[n];
            if (heap.structIndex == unresolvedIndex) {
                componentCount += WriteUnresolvedComponents(entity);
                continue;
            }
            var value       = heap.Write(componentWriter, entity.compIndex);
            var keyBytes    = structTypes[heap.structIndex].componentKeyBytes; 
            writer.MemberBytes(keyBytes.AsSpan(), value);
            componentCount++;
        }
        // --- write scripts
        foreach (var script in entity.Scripts) {
            componentWriter.WriteObject(script, ref buffer);
            var classType   = componentTypeByType[script.GetType()];
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
    
    private int WriteUnresolvedComponents(GameEntity entity)
    {
        var unresolved = entity.GetComponent<Unresolved>();
        var components = unresolved.components;
        if (components == null) {
            return 0;
        }
        int count = 0;
        foreach (var component in components)
        {
            var key     = Encoding.UTF8.GetBytes(component.key); // todo remove byte[] allocation
            var data    = JsonUtils.JsonValueToBytes(component.value);
            writer.MemberBytes(key, data);
            count++;
        }
        return count;
    }
}
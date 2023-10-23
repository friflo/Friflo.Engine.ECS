// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

namespace Friflo.Fliox.Engine.ECS.Database;

/// <summary>
/// Create the <see cref="JsonValue"/> from all components and behaviors used at <see cref="DatabaseEntity.components"/>.<br/>
/// </summary>
internal sealed class ComponentWriter
{
    private readonly    ObjectWriter                    componentWriter;
    private             Utf8JsonWriter                  writer;
    private             Bytes                           buffer;
    private readonly    ComponentType[]                 structTypes;
    private readonly    Dictionary<Type, ComponentType> componentTypeByType;
    
    internal static readonly ComponentWriter Instance = new ComponentWriter();
    
    private ComponentWriter() {
        buffer              = new Bytes(128);
        componentWriter     = new ObjectWriter(EntityStore.Static.TypeStore);
        var schema          = EntityStore.Static.ComponentSchema;
        structTypes         = schema.components;
        componentTypeByType = schema.componentTypeByType;
    }
    
    internal JsonValue Write(GameEntity entity)
    {
        var archetype = entity.archetype;
        if (entity.ComponentCount() == 0) {
            return default;
        }
        writer.InitSerializer();
        writer.ObjectStart();
        // --- write components
        var heaps           = archetype.Heaps;
        for (int n = 0; n < heaps.Length; n++) {
            var heap        = heaps[n];
            var value       = heap.Write(componentWriter, entity.compIndex);
            var keyBytes    = structTypes[heap.structIndex].componentKeyBytes; 
            writer.MemberBytes(keyBytes, value);
        }
        // --- write behaviors
        foreach (var component in entity.Behaviors) {
            componentWriter.WriteObject(component, ref buffer);
            var classType   = componentTypeByType[component.GetType()];
            var keyBytes    = classType.componentKeyBytes;
            writer.MemberBytes(keyBytes, buffer);
        }
        writer.ObjectEnd();
        return new JsonValue(writer.json);
    }
}
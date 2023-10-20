// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS.Sync;

/// <summary>
/// Create the <see cref="JsonValue"/> from all class / struct components used at <see cref="DataNode.components"/>.<br/>
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
        structTypes         = schema.structs;
        componentTypeByType = schema.componentTypeByType;
    }
    
    internal JsonValue Write(GameEntity entity)
    {
        var archetype = entity.archetype;
        if (entity.ComponentCount == 0) {
            return default;
        }
        writer.InitSerializer();
        writer.ObjectStart();
        // --- write struct components
        var heaps = archetype.Heaps;
        for (int n = 0; n < heaps.Length; n++) {
            var heap        = heaps[n];
            var value       = heap.Write(componentWriter, entity.compIndex);
            var keyBytes    = structTypes[heap.structIndex].componentKeyBytes; 
            writer.MemberBytes(keyBytes, value);
        }
        // --- write class components
        foreach (var component in entity.ClassComponents) {
            componentWriter.WriteObject(component, ref buffer);
            var classType   = componentTypeByType[component.GetType()];
            var keyBytes    = classType.componentKeyBytes;
            writer.MemberBytes(keyBytes, buffer);
        }
        writer.ObjectEnd();
        return new JsonValue(writer.json);
    }
}
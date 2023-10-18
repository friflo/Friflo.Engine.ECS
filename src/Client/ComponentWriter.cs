// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

namespace Friflo.Fliox.Engine.Client;

/// <summary>
/// Create the <see cref="JsonValue"/> from all class / struct components used at <see cref="DataNode.components"/>.<br/>
/// </summary>
internal sealed class ComponentWriter
{
    private readonly    ObjectWriter    componentWriter;
    private             Utf8JsonWriter  writer;
    private             Bytes           buffer;
    private readonly    ComponentType[] structTypes;
    
    internal static readonly ComponentWriter Instance = new ComponentWriter();
    
    private ComponentWriter() {
        buffer          = new Bytes(128);
        componentWriter = new ObjectWriter(EntityStore.Static.TypeStore);
        var schema      = EntityStore.Static.ComponentSchema;
        structTypes     = schema.structs;
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
            var classKey = ClassUtils.GetClassKeyBytes(component.GetType());
            writer.MemberBytes(classKey, buffer);
        }
        writer.ObjectEnd();
        return new JsonValue(writer.json);
    }
}
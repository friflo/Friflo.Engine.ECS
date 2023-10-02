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
    
    internal static readonly ComponentWriter Instance = new ComponentWriter();
    
    private ComponentWriter() {
        buffer          = new Bytes(128);
        componentWriter = new ObjectWriter(EntityStore.Static.TypeStore);
    }
    
    internal JsonValue  Write(GameEntity entity)
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
            var keyBytes    = new Bytes(heap.keyName); // todo cache bytes 
            writer.MemberBytes(keyBytes, value);
        }
        // --- write class components
        var classComponents = entity.ClassComponents;
        foreach (var component in classComponents) {
            componentWriter.WriteObject(component, ref buffer);
            var keyName     = ClassUtils.GetKeyName(component.GetType());
            var keyBytes    = new Bytes(keyName);       // todo cache bytes 
            writer.MemberBytes(keyBytes, buffer);
        }
        writer.ObjectEnd();
        return new JsonValue(writer.json);
    }
    
}
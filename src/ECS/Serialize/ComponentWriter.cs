// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using Friflo.Engine.ECS.Utils;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

namespace Friflo.Engine.ECS.Serialize;

/// <summary>
/// Create the <see cref="JsonValue"/> from all components and scripts used at <see cref="DataEntity.components"/>.<br/>
/// </summary>
internal sealed class ComponentWriter
{
    private  readonly   ObjectWriter                    componentWriter;
    private             Utf8JsonWriter                  writer;
    internal            Bytes                           buffer;
    private  readonly   ComponentType[]                 structTypes;
    private  readonly   Dictionary<Type, ScriptType>    scriptTypeByType;
    private  readonly   int                             unresolvedIndex;
    
    internal ComponentWriter() {
        buffer              = new Bytes(128);
        componentWriter     = new ObjectWriter(EntityStoreBase.Static.TypeStore);
        var schema          = EntityStoreBase.Static.EntitySchema;
        structTypes         = schema.components;
        scriptTypeByType    = schema.scriptTypeByType;
        unresolvedIndex     = schema.unresolvedType.StructIndex;
    }
    
    internal JsonValue Write(Entity entity, List<JsonValue> members, bool pretty)
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
        var heaps = archetype.Heaps();
        for (int n = 0; n < heaps.Length; n++) {
            var heap = heaps[n];
            if (heap.structIndex == unresolvedIndex) {
                componentCount += WriteUnresolvedComponents(entity, members);
                continue;
            }
            var componentType = structTypes[heap.structIndex];
            if (componentType.ComponentKey == null) {
                continue;
            }
            var value           = heap.Write(componentWriter, entity.compIndex);
            var keyBytes        = componentType.componentKeyBytes;
            var start           = writer.json.end;
            writer.MemberBytes(keyBytes.AsSpan(), value);
            members?.AddMember(writer, start);
            componentCount++;
        }
        // --- write scripts
        foreach (var script in entity.Scripts) {
            componentWriter.WriteObject(script, ref buffer);
            var classType   = scriptTypeByType[script.GetType()];
            var keyBytes    = classType.componentKeyBytes;
            var start       = writer.json.end;
            writer.MemberBytes(keyBytes.AsSpan(), buffer);
            members?.AddMember(writer, start);
            componentCount++;
        }
        if (componentCount == 0) {
            return default;
        }
        writer.ObjectEnd();
        return new JsonValue(writer.json);
    }
    

    private int WriteUnresolvedComponents(Entity entity, List<JsonValue> members)
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
            var start   = writer.json.end;
            writer.MemberBytes(key, data);
            members?.AddMember(writer, start);
            count++;
        }
        return count;
    }
}

internal static class ComponentWriterExtensions
{
    internal static void AddMember(this List<JsonValue> members, Utf8JsonWriter writer, int start)
    {
        var buffer = writer.json.buffer;
        if (buffer[start] == ',') {
            start++;
        }
        members.Add(new JsonValue(buffer, start, writer.json.end - start));
    }
}
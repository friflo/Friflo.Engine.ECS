// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Serialize;

/// <summary>
/// Used to serialize a single <see cref="DataEntity"/> to JSON.
/// </summary>
public sealed class DataEntitySerializer
{
    private readonly    DataEntity      dataEntity;      
    private readonly    ObjectWriter    objectWriter;
    private             Utf8JsonParser  parser;
    private             Utf8JsonWriter  jsonWriter;
    
    public DataEntitySerializer()
    {
        dataEntity      = new DataEntity();      
        objectWriter    = new (new TypeStore()) {  // todo use global TypeStore
            Pretty              = true,
            WriteNullMembers    = false
        };
    }
    
    /// <summary>
    /// Return the given <see cref="DataEntity"/> as JSON.
    /// </summary>
    public string WriteDataEntity(DataEntity data, out string error)
    {
        parser.InitParser(data.components);
        error = Traverse();
        if (error != null) {
            return null;
        }
        dataEntity.pid          = data.pid;
        dataEntity.tags         = data.tags;
        dataEntity.children     = data.children;
        var components          = jsonWriter.json;
        dataEntity.components   = components.buffer == null ? new JsonValue() : new JsonValue(components);
        
        return objectWriter.Write(dataEntity);
    }
    
    /// <summary> used to format the <see cref="DataEntity.components"/> - one component per line</summary>
    private string Traverse()
    {
        var ev = parser.NextEvent();
        switch (ev)
        {
            case JsonEvent.Error:
                var msg = parser.error.GetMessage();
                return $"'components' error: {msg}";
            case JsonEvent.ValueNull:
                break;
            case JsonEvent.ObjectStart:
                jsonWriter.InitSerializer();
                ev = TraverseComponents();
                if (ev != JsonEvent.ObjectEnd) {
                    return $"'components' element must be an object. was {ev}, component: '{parser.key}'";
                }
                break;
            default:
                return $"expect 'components' == object or null. was: {ev}";
        }
        return null;
    }
    
    private static readonly Bytes   ComponentsStart = new Bytes("{\n");
    private static readonly Bytes   KeyStart        = new Bytes("        \"");
    private static readonly Bytes   KeyEnd          = new Bytes("\": ");
    private static readonly Bytes   ComponentNext   = new Bytes(",\n");
    private static readonly Bytes   ComponentsEnd   = new Bytes("\n    }");
    
    private JsonEvent TraverseComponents()
    {
        ref var json = ref jsonWriter.json;
        json.AppendBytes(ComponentsStart);
        var ev = parser.NextEvent();
        while (true)
        {
            switch (ev)
            {
                case JsonEvent.ObjectStart:
                    json.AppendBytes(KeyStart);
                    json.AppendBytes(parser.key);
                    json.AppendBytes(KeyEnd);
                    jsonWriter.WriteTree(ref parser);
                    ev = parser.NextEvent();
                    if (ev == JsonEvent.ObjectEnd) {
                        json.AppendBytes(ComponentsEnd);
                        return JsonEvent.ObjectEnd;
                    }
                    json.AppendBytes(ComponentNext);
                    break;
                default:
                    return ev;
            }
        }
    }
}
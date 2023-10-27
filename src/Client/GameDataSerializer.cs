// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;

// ReSharper disable UseUtf8StringLiteral
// ReSharper disable MergeIntoPattern
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

public class GameDataSerializer
{
    private readonly    GameEntityStore     store;
    private             Bytes               componentBuf;
    private readonly    EntityConverter     converter;
    // --- write specific fields
    private             Utf8JsonWriter      writer;
    private             bool                isFirst;
    private readonly    DataEntity          writeEntity;
    // --- read specific fields
    private             Utf8JsonParser      parser;
    private             int                 readEntityCount;
    private readonly    DataEntity          readEntity;
    
#region constructor
    public GameDataSerializer(GameEntityStore store) {
        this.store      = store;
        converter       = new EntityConverter();
        componentBuf    = new Bytes(32);
        readEntity      = new DataEntity {
            children        = new List<long>(),
            tags            = new List<string>()
        };
        writeEntity      = new DataEntity();
    }
    #endregion

#region write scene
    private static readonly byte[] ArrayStart  = Encoding.UTF8.GetBytes("[");
    private static readonly byte[] ArrayEnd    = Encoding.UTF8.GetBytes("]");

    public async Task WriteSceneAsync(Stream stream)
    {
        await stream.WriteAsync(ArrayStart);
        writer.SetPretty(true);
        isFirst     = true;
        var nodeMax = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity  = store.GetNodeById(n).Entity;
            if (entity == null) {
                continue;
            }
            WriteEntity(entity);
            var memory = new ReadOnlyMemory<byte>(writer.json.buffer, 0, writer.json.end);
            await stream.WriteAsync(memory);
        }
        await stream.WriteAsync(ArrayEnd);
    }
    
    public void WriteScene(Stream stream)
    {
        stream.Write(ArrayStart);
        writer.SetPretty(true);
        isFirst     = true;
        var nodeMax = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity  = store.GetNodeById(n).Entity;
            if (entity == null) {
                continue;
            }
            WriteEntity(entity);
            stream.Write(writer.json.AsSpan());
        }
        stream.Write(ArrayEnd);
    }
    
    private void WriteEntity(GameEntity entity)
    {
        writer.InitSerializer();
        if (isFirst) {
            isFirst = false;
        } else {
            writer.json.AppendChar(',');
        }
        converter.GameToDataEntity(entity, writeEntity, true);
        WriteDataEntity(writeEntity);
    }
    
    private static readonly     Bytes   PidKey          = new Bytes("pid");
    private static readonly     Bytes   ChildrenKey     = new Bytes("children");
    private static readonly     Bytes   ComponentsKey   = new Bytes("components");
    private static readonly     Bytes   TagsKey         = new Bytes("tags");
    private static readonly     Bytes   Indent          = new Bytes("    ");
    
    private void WriteDataEntity(DataEntity dataEntity)
    {
        writer.ObjectStart();
        writer.MemberLng(PidKey, dataEntity.pid);
        var children = dataEntity.children;
        if (children != null && children.Count > 0)
        {
            writer.MemberArrayStart(ChildrenKey);
            foreach (var child in children) {
                writer.ElementLng(child);
            }
            writer.ArrayEnd();
        }
        if (!dataEntity.components.IsNull())
        {
            FormatComponents(dataEntity.components);
            writer.MemberBytes(ComponentsKey, componentBuf);
        }
        var tags = dataEntity.tags;
        if (tags != null && tags.Count > 0)
        {
            writer.MemberArrayStart(TagsKey);
            foreach (var tag in tags) {
                writer.ElementStr(tag);   
            }
            writer.ArrayEnd();
        }
        writer.ObjectEnd();
    }
    
    private void FormatComponents(in JsonValue components)
    {
        componentBuf.Clear();
        var span    = components.AsReadOnlySpan();
        var start   = 0;
        int n       = 0;
        for (; n < span.Length; n++) {
            if (span[n] != '\n') {
                continue;
            }
            var line = span.Slice(start, n - start + 1);
            componentBuf.AppendBytesSpan(line);
            componentBuf.AppendBytes(Indent);
            start = n + 1;
        }
        var lastLine = span.Slice(start, span.Length - start);
        componentBuf.AppendBytesSpan(lastLine);
    }
    #endregion
    
#region read scene
    public int ReadScene(Stream stream, out string error)
    {
        readEntityCount = 0;
        parser.InitParser(stream);
        var ev = parser.NextEvent();
        switch (ev)
        {
            case JsonEvent.Error:
                error = parser.error.GetMessage();
                return readEntityCount;
            case JsonEvent.ArrayStart:
                ev = ReadEntities();
                if (ev != JsonEvent.ArrayEnd) {
                    error = $"expect array end. was {ev}";
                }
                error = null;
                return readEntityCount;
            default:
                 error = $"expect array. was: {ev}";
                 return readEntityCount;
        }
    }
    
    private JsonEvent ReadEntities()
    {
        while (true) {
            var ev = parser.NextEvent();
            switch (ev)
            {
                case JsonEvent.ObjectStart:
                    readEntity.pid = -1;
                    readEntity.children.Clear();
                    readEntity.components = default;
                    readEntity.tags.Clear();
                    ev = ReadEntity();
                    converter.DataToGameEntity(readEntity, store, out _);
                    readEntityCount++;
                    if (ev != JsonEvent.ObjectEnd) {
                        return ev;
                    }
                    break;
                default:
                    return ev;
            }
        }
    }
    
    private static readonly Bytes   Pid         = new Bytes("pid");
    private static readonly Bytes   Children    = new Bytes("children");
    private static readonly Bytes   Components  = new Bytes("components");
    private static readonly Bytes   Tags        = new Bytes("tags");
    
    private JsonEvent ReadEntity()
    {
        while (true) {
            var ev = parser.NextEvent();
            switch (ev) {
                case JsonEvent.ValueNumber: // pid
                    if (parser.key.IsEqual(Pid)) {
                        readEntity.pid = parser.ValueAsLong(out _);
                    }
                    continue;
                case JsonEvent.ValueString:
                case JsonEvent.ValueNull:
                    continue;
                case JsonEvent.ArrayStart:  // children | tags
                    if (parser.key.IsEqual(Children)) {
                        ReadChildren();
                        continue;
                    }
                    if (parser.key.IsEqual(Tags)) {
                        ReadTags();
                        continue;
                    }
                    parser.SkipTree();
                    continue;
                case JsonEvent.ObjectStart:  // components
                    if (parser.key.IsEqual(Components)) {
                        ReadComponents();
                        continue;
                    }
                    parser.SkipTree();
                    continue;
                case JsonEvent.ObjectEnd:
                    return JsonEvent.ObjectEnd;
                default:
                    return ev;
            }
        }
    }
    
    private void ReadComponents()
    {
        var start = parser.Position;
        parser.SkipTree();
        var end = parser.Position;
        componentBuf.Clear();
        parser.AppendInputSlice(ref componentBuf, start - 1, end);
        readEntity.components = new JsonValue(componentBuf);
    }
    
    private JsonEvent ReadChildren()
    {
        while (true) {
            var ev = parser.NextEvent();
            switch (ev) {
                case JsonEvent.ValueNumber:
                    var childId = parser.ValueAsLong(out _);
                    readEntity.children.Add(childId);
                    continue;
                default:
                    return ev;
            }
        }
    }
    
    private JsonEvent ReadTags()
    {
        while (true) {
            var ev = parser.NextEvent();
            switch (ev) {
                case JsonEvent.ValueString:
                    var tag = parser.value.AsString();
                    readEntity.tags.Add(tag);
                    continue;
                default:
                    return ev;
            }
        }
    }
    #endregion
}
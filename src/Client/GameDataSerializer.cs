// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;

[assembly: InternalsVisibleTo("Tests-internal")]
[assembly: InternalsVisibleTo("Fliox.Tests-internal")]

// ReSharper disable UseUtf8StringLiteral
// ReSharper disable MergeIntoPattern
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

public class GameDataSerializer
{
#region private fields
    private readonly    EntityStore         store;
    private             Bytes               componentBuf;
    private readonly    EntityConverter     converter;
    // --- write specific fields
    private             Utf8JsonWriter      writer;
    private             bool                isFirst;
    private readonly    DataEntity          writeEntity;
    // --- read specific fields
    private             Utf8JsonParser      parser;
    private             int                 readEntityCount;
    private             JsonValue           readJson;
    private             byte[]              readBuffer;
    private readonly    DataEntity          readEntity;
    #endregion
    
#region constructor
    public GameDataSerializer(EntityStore store) {
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
    
    public void WriteEntities(Stream stream, IEnumerable<GameEntity> entities)
    {
        stream.Write(ArrayStart);
        writer.SetPretty(true);
        isFirst     = true;
        foreach (var entity in entities)
        {
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
    
    private static readonly     Bytes   IdKey           = new Bytes("id");
    private static readonly     Bytes   ChildrenKey     = new Bytes("children");
    private static readonly     Bytes   ComponentsKey   = new Bytes("components");
    private static readonly     Bytes   TagsKey         = new Bytes("tags");
    
    private void WriteDataEntity(DataEntity dataEntity)
    {
        writer.ObjectStart();
        writer.MemberLng(IdKey.AsSpan(), dataEntity.pid);
        var children = dataEntity.children;
        if (children != null && children.Count > 0)
        {
            writer.MemberArrayStart(ChildrenKey.AsSpan());
            foreach (var child in children) {
                writer.ElementLng(child);
            }
            writer.ArrayEnd();
        }
        if (!dataEntity.components.IsNull())
        {
            // JsonUtils.FormatComponents(dataEntity.components, ref componentBuf);
            // writer.MemberBytes(ComponentsKey.AsSpan(), componentBuf);
            var componentBytes = JsonUtils.JsonValueToBytes(dataEntity.components);
            writer.MemberBytes(ComponentsKey.AsSpan(), componentBytes);
        }
        var tags = dataEntity.tags;
        if (tags != null && tags.Count > 0)
        {
            writer.MemberArrayStart(TagsKey.AsSpan());
            foreach (var tag in tags) {
                writer.ElementStr(tag);   
            }
            writer.ArrayEnd();
        }
        writer.ObjectEnd();
    }
    

    #endregion
    
#region read scene
    private MemoryStream CreateReadBuffers(Stream stream) {
        readBuffer ??= new byte[16*1024];
        int capacity = 0;
        if (stream is FileStream fileStream) {
            var fileInfo = new FileInfo(fileStream.Name);
            capacity = (int)fileInfo.Length;
        }
        return new MemoryStream(capacity);
    }

    public async Task<ReadSceneResult> ReadSceneAsync(Stream stream)
    {
        if (stream is MemoryStream memoryStream) {
            return ReadSceneSync(memoryStream);
        }
        var readStream = CreateReadBuffers(stream);
        int read;
        while((read = await stream.ReadAsync(readBuffer)) > 0) {
            readStream.Write (readBuffer, 0, read);
        }
        return ReadSceneSync(readStream);
    }
    
    public ReadSceneResult ReadScene(Stream stream)
    {
        if (stream is MemoryStream memoryStream) {
            return ReadSceneSync(memoryStream);
        }
        var readStream = CreateReadBuffers(stream);
        int read;
        while((read = stream.Read (readBuffer)) > 0) {
            readStream.Write(readBuffer, 0, read);
        }
        return ReadSceneSync(readStream);
    }

    private ReadSceneResult ReadSceneSync(MemoryStream memoryStream)
    {
        try {
            readJson = new JsonValue(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            readEntityCount = 0;
            parser.InitParser(readJson);
            var ev = parser.NextEvent();
            switch (ev)
            {
                case JsonEvent.ArrayStart:
                    ev = ReadEntities();
                    if (ev == JsonEvent.Error) {
                        return new ReadSceneResult(readEntityCount, parser.error.GetMessage());
                    }
                    return new ReadSceneResult(readEntityCount, null);
                case JsonEvent.Error:
                    return new ReadSceneResult(readEntityCount, parser.error.GetMessage());
                default:
                    return new ReadSceneResult(readEntityCount, $"expect array. was: {ev} at position: {parser.Position}");
            }
        }
        finally {
            readJson = default;
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
                    if (ev != JsonEvent.ObjectEnd) {
                        return ev;
                    }
                    converter.DataToGameEntity(readEntity, store, out _);
                    readEntityCount++;
                    break;
                case JsonEvent.ArrayEnd:
                case JsonEvent.Error:
                    return ev;
                default:
                    return ReadError($"expect object entity. was: {ev}");
            }
        }
    }
    
    private static readonly Bytes   Id          = new Bytes("id");
    private static readonly Bytes   Children    = new Bytes("children");
    private static readonly Bytes   Components  = new Bytes("components");
    private static readonly Bytes   Tags        = new Bytes("tags");
    
    private JsonEvent ReadEntity()
    {
        while (true) {
            var ev = parser.NextEvent();
            switch (ev) {
                case JsonEvent.ValueNumber:
                    if (parser.key.IsEqual(Id)) {           // "id"
                        readEntity.pid = parser.ValueAsLong(out _);
                    }
                    continue;
                case JsonEvent.ValueNull:
                case JsonEvent.ValueBool:
                case JsonEvent.ValueString:
                    continue;
                case JsonEvent.ArrayStart:
                    if (parser.key.IsEqual(Children)) {     // "children"
                        ev = ReadChildren();
                        if (ev == JsonEvent.Error) {
                            return ev;
                        }
                        continue;
                    }
                    if (parser.key.IsEqual(Tags)) {         // "tags"
                        ev = ReadTags();
                        if (ev == JsonEvent.Error) {
                            return ev;
                        }
                        continue;
                    }
                    parser.SkipTree();
                    continue;
                case JsonEvent.ObjectStart:  
                    if (parser.key.IsEqual(Components)) {   // "components"
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
        var start = parser.Position - 1;
        parser.SkipTree();
        componentBuf.Clear();
        readEntity.components = new JsonValue(readJson.MutableArray, start, parser.Position - start);
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
                case JsonEvent.ArrayEnd:
                case JsonEvent.Error:
                    return ev;
                default:
                    return ReadError($"expect child id number. was: {ev}");
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
                case JsonEvent.ArrayEnd:
                case JsonEvent.Error:
                    return ev;
                default:
                    return ReadError($"expect tag string. was: {ev}");
            }
        }
    }
    
    private JsonEvent ReadError(string message)
    {
        parser.ErrorMsg("GameDataSerializer", $"{message} at position: {parser.Position}");
        return JsonEvent.Error;
    }
    #endregion
}
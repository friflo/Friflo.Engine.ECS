// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Friflo.Engine.ECS.Utils;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable UseUtf8StringLiteral
// ReSharper disable MergeIntoPattern
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Engine.ECS.Serialize;

/// <summary>
/// Enables serialization of entities to / from JSON.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#json-serialization">Example.</a>
/// </summary>
public sealed class EntitySerializer
{
#region private fields
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
    private readonly    DataEntity          readEntityBuffer;
    private             DataEntity          readEntity;
    #endregion
    
#region constructor
    public EntitySerializer()
    {
        converter           = new EntityConverter();
        componentBuf        = new Bytes(32);
        readEntityBuffer    = new DataEntity {
            children            = new List<long>(),
            tags                = new List<string>()
        };
        writeEntity      = new DataEntity();
    }
    #endregion

#region write entities
    private static readonly byte[] ArrayStart  = Encoding.UTF8.GetBytes("[");
    private static readonly byte[] ArrayEnd    = Encoding.UTF8.GetBytes("]");

    /// <summary>
    /// Asynchronously writes all entities in the given <paramref name="store"/> as a JSON array to the passed <paramref name="stream"/>. 
    /// </summary>
    public async Task WriteStoreAsync(EntityStore store, Stream stream)
    {
        await stream.WriteAsync(ArrayStart, 0 , ArrayStart.Length);
        writer.SetPretty(true);
        isFirst     = true;
        var nodeMax = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity  = store.GetEntityById(n);
            if (entity.IsNull) {
                continue;
            }
            WriteEntityInternal(entity);
            var json = writer.json;
            await stream.WriteAsync(json.buffer, json.start, json.end - json.start);
        }
        await stream.WriteAsync(ArrayEnd, 0, ArrayEnd.Length);
    }
    
    /// <summary>
    /// Writes all entities in the given <paramref name="store"/> as a JSON array to the passed <paramref name="stream"/>. 
    /// </summary>
    public void WriteStore(EntityStore store, Stream stream)
    {
        stream.Write(ArrayStart, 0, ArrayStart.Length);
        writer.SetPretty(true);
        isFirst     = true;
        var nodeMax = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity  = store.GetEntityById(n);
            if (entity.IsNull) {
                continue;
            }
            WriteEntityInternal(entity);
            var json = writer.json;
            stream.Write(json.buffer, json.start, json.end - json.start);
        }
        stream.Write(ArrayEnd, 0, ArrayEnd.Length);
    }
    
    /// <summary>
    /// Writes the given <paramref name="entities"/> as a JSON array to the passed <paramref name="stream"/>. 
    /// </summary>
    public void WriteEntities(IEnumerable<Entity> entities, Stream stream)
    {
        stream.Write(ArrayStart, 0, ArrayStart.Length);
        writer.SetPretty(true);
        isFirst     = true;
        foreach (var entity in entities)
        {
            if (entity.IsNull) {
                continue;
            }
            WriteEntityInternal(entity);
            var json = writer.json;
            stream.Write(json.buffer, json.start, json.end - json.start);
        }
        stream.Write(ArrayEnd, 0, ArrayEnd.Length);
    }
    
    /// <summary>
    /// Returns a JSON object of the passed <paramref name="entity"/> as a string. 
    /// </summary>
    public string WriteEntity(Entity entity)
    {
        writer.SetPretty(true);
        isFirst     = true;
        WriteEntityInternal(entity);
        return writer.json.AsString();
    }
    
    private void WriteEntityInternal(Entity entity)
    {
        writer.InitSerializer();
        if (isFirst) {
            isFirst = false;
        } else {
            writer.json.AppendChar(',');
        }
        converter.EntityToDataEntity(entity, writeEntity, true);
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
    
#region read entities into store
    /// <remarks> The "id" in the passed <paramref name="value"/> is ignored. </remarks>
    internal string ReadIntoEntity(Entity entity, JsonValue value)
    {
        readJson        = value;
        readEntityCount = 0;
        parser.InitParser(readJson);
        var ev = ReadIntoEntityInternal(entity);
        if (ev == JsonEvent.EOF) {
            return null;
        }
        return parser.error.GetMessage();
    }
    
    private JsonEvent ReadIntoEntityInternal(Entity entity)
    {
        var store = entity.store;
        while (true) {
            var ev = parser.NextEvent();
            switch (ev)
            {
                case JsonEvent.ObjectStart:
                    readEntity              = readEntityBuffer;
                    readEntity.components   = default;
                    readEntity.children.Clear();
                    readEntity.tags.Clear();
                    ev = ReadEntity();
                    if (ev != JsonEvent.ObjectEnd) {
                        return ev;
                    }
                    ev = ReadEntity();
                    if (ev != JsonEvent.EOF) {
                        return ev;
                    }
                    readEntity.pid = entity.Pid;
                    converter.DataEntityToEntity(readEntity, store, out string error);
                    readEntityCount++;
                    if (error != null) {
                        return ReadError(error);
                    }
                    return ev;
                case JsonEvent.Error:
                    return ev;
                default:
                    return ReadError($"expect object entity. was: {ev}");
            }
        }
    }

    private MemoryStream CreateReadBuffers(Stream stream)
    {
        readBuffer ??= new byte[16*1024];
        int capacity = 0;
        if (stream is FileStream fileStream) {
            var fileInfo = new FileInfo(fileStream.Name);
            capacity = (int)fileInfo.Length;
        }
        return new MemoryStream(capacity);
    }

    /// <summary>
    /// Asynchronously reads the JSON array of the given <paramref name="stream"/> into the passed <paramref name="store"/>.   
    /// </summary>
    public async Task<ReadResult> ReadIntoStoreAsync(EntityStore store, Stream stream)
    {
        if (stream is MemoryStream memoryStream) {
            return ReadIntoStoreSync(store, memoryStream);
        }
        var readStream = CreateReadBuffers(stream);
        int read;
        while((read = await stream.ReadAsync(readBuffer, 0, readBuffer.Length)) > 0) {
            readStream.Write (readBuffer, 0, read);
        }
        return ReadIntoStoreSync(store, readStream);
    }
    
    /// <summary>
    /// Reads the JSON array of the given <paramref name="stream"/> into the passed <paramref name="store"/>.   
    /// </summary>
    public ReadResult ReadIntoStore(EntityStore store, Stream stream)
    {
        if (stream is MemoryStream memoryStream) {
            return ReadIntoStoreSync(store, memoryStream);
        }
        var readStream = CreateReadBuffers(stream);
        int read;
        while((read = stream.Read (readBuffer, 0, readBuffer.Length)) > 0) {
            readStream.Write(readBuffer, 0, read);
        }
        return ReadIntoStoreSync(store, readStream);
    }

    private ReadResult ReadIntoStoreSync(EntityStore store, MemoryStream memoryStream)
    {
        try {
            readJson = new JsonValue(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            readEntityCount = 0;
            parser.InitParser(readJson);
            var ev = parser.NextEvent();
            switch (ev)
            {
                case JsonEvent.ArrayStart:
                    ev = ReadEntitiesArrayIntoStore(store);
                    if (ev == JsonEvent.Error) {
                        return new ReadResult(readEntityCount, parser.error.GetMessage());
                    }
                    return new ReadResult(readEntityCount, null);
                case JsonEvent.Error:
                    return new ReadResult(readEntityCount, parser.error.GetMessage());
                default:
                    return new ReadResult(readEntityCount, $"expect array. was: {ev} at position: {parser.Position}");
            }
        }
        finally {
            readJson = default;
        }
    }
    
    private JsonEvent ReadEntitiesArrayIntoStore(EntityStore store)
    {
        while (true) {
            var ev = parser.NextEvent();
            switch (ev)
            {
                case JsonEvent.ObjectStart:
                    readEntity              = readEntityBuffer;
                    readEntity.pid          = -1;
                    readEntity.components   = default;
                    readEntity.children.Clear();
                    readEntity.tags.Clear();
                    ev = ReadEntity();
                    if (ev != JsonEvent.ObjectEnd) {
                        return ev;
                    }
                    converter.DataEntityToEntity(readEntity, store, out var error);
                    readEntityCount++;
                    if (error != null) {
                        return ReadError(error);
                    }
                    break;
                case JsonEvent.ArrayEnd:
                case JsonEvent.Error:
                    return ev;
                default:
                    return ReadError($"expect object entity. was: {ev}");
            }
        }
    }
    #endregion
    
#region read entities
    /// <summary>
    /// Reads the JSON array of the given <paramref name="stream"/> into the passed <paramref name="entities"/> list.   
    /// </summary>
    public ReadResult ReadEntities(List<DataEntity> entities, Stream stream)
    {
        if (stream is MemoryStream memoryStream) {
            return ReadEntitiesSync(entities, memoryStream);
        }
        var readStream = CreateReadBuffers(stream);
        int read;
        while((read = stream.Read (readBuffer, 0 , readBuffer.Length)) > 0) {
            readStream.Write(readBuffer, 0, read);
        }
        return ReadEntitiesSync(entities, readStream);
    }

    private ReadResult ReadEntitiesSync(List<DataEntity> entities, MemoryStream memoryStream)
    {
        try {
            readJson = new JsonValue(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            parser.InitParser(readJson);
            var ev = parser.NextEvent();
            switch (ev)
            {
                case JsonEvent.ArrayStart:
                    ev = ReadEntitiesArray(entities);
                    if (ev == JsonEvent.Error) {
                        return new ReadResult(entities.Count, parser.error.GetMessage());
                    }
                    return new ReadResult(entities.Count, null);
                case JsonEvent.Error:
                    return new ReadResult(entities.Count, parser.error.GetMessage());
                default:
                    return new ReadResult(entities.Count, $"expect array. was: {ev} at position: {parser.Position}");
            }
        }
        finally {
            readJson = default;
        }
    }
    
    private JsonEvent ReadEntitiesArray(List<DataEntity> entities)
    {
        while (true) {
            var ev = parser.NextEvent();
            switch (ev)
            {
                case JsonEvent.ObjectStart:
                    readEntity = new DataEntity();
                    ev = ReadEntity();
                    if (ev != JsonEvent.ObjectEnd) {
                        return ev;
                    }
                    entities.Add(readEntity);
                    break;
                case JsonEvent.ArrayEnd:
                case JsonEvent.Error:
                    return ev;
                default:
                    return ReadError($"expect object entity. was: {ev}");
            }
        }
    }
    #endregion
    
#region read JSON entity
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
                    if (parser.key.IsEqual(Components)) {   // "components" - error
                        parser.ErrorMsg(nameof(EntitySerializer), $"expect 'components' == object. was: array.");
                        return JsonEvent.Error;
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
                    var children = readEntity.children;
                    if (children == null) {
                        children = readEntity.children = new List<long>(1);
                    }
                    children.Add(childId);
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
                    var tag     = parser.value.AsString();
                    var tags    = readEntity.tags;
                    if (tags == null) {
                        tags = readEntity.tags = new List<string>(1);
                    }
                    tags.Add(tag);
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
        parser.ErrorMsg("EntitySerializer", message);
        return JsonEvent.Error;
    }
    #endregion
}
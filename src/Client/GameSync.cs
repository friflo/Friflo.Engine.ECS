// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Client;

// ReSharper disable UseUtf8StringLiteral
// ReSharper disable MergeIntoPattern
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public sealed class GameSync
{
    private readonly    GameEntityStore                 store;
    private readonly    GameClient                      client;
    private readonly    LocalEntities<long, DataEntity> localEntities;
    private readonly    EntityConverter                 converter;
    private             Utf8JsonWriter                  writer;
    private             Bytes                           componentBuf;

    public GameSync (GameEntityStore store, GameClient client) {
        this.store      = store;
        this.client     = client;
        localEntities   = client.entities.Local;
        converter       = new EntityConverter();
        componentBuf    = new Bytes(32);
    }
    
    public void LoadGameEntities()
    {
        var query = client.entities.QueryAll();
        client.SyncTasks().Wait(); // todo enable synchronous queries in MemoryDatabase
        
        var dataEntities = query.Result;
        foreach (var data in dataEntities) {
            converter.DataToGameEntity(data, store, out _);
        }
    }
    
    public void StoreGameEntities()
    {
        var nodeMax = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            ref var node    = ref store.GetNodeById(n);
            var entity      = node.Entity;
            if (entity == null) {
                continue;
            }
            if (!localEntities.TryGetEntity(node.Id, out DataEntity dataEntity)) {
                dataEntity = new DataEntity();
            }
            dataEntity = converter.GameToDataEntity(entity, dataEntity, true);
            client.entities.Upsert(dataEntity);
        }
        client.SyncTasksSynchronous();
    }
    
    private static readonly byte[] ArrayStart  = Encoding.UTF8.GetBytes("[");
    private static readonly byte[] Comma       = Encoding.UTF8.GetBytes(",");
    private static readonly byte[] ArrayEnd    = Encoding.UTF8.GetBytes("]");
    
    public async Task WriteSceneAsync(Stream stream)
    {
        await stream.WriteAsync(ArrayStart);
        writer.SetPretty(true);
        var dataEntity  = new DataEntity();
        var nodeMax     = store.NodeMaxId;
        var isFirst     = true;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity  = store.GetNodeById(n).Entity;
            if (entity == null) {
                continue;
            }
            if (isFirst) {
                isFirst = false;
            } else {
                await stream.WriteAsync(Comma);
            }
            converter.GameToDataEntity(entity, dataEntity, true);
            writer.InitSerializer();
            WriteDataEntity(dataEntity);
            var memory = new ReadOnlyMemory<byte>(writer.json.buffer, 0, writer.json.end);
            await stream.WriteAsync(memory);
        }
        await stream.WriteAsync(ArrayEnd);
    }
    
    public void WriteScene(Stream stream)
    {
        stream.Write(ArrayStart);
        writer.SetPretty(true);
        var dataEntity  = new DataEntity();
        var nodeMax     = store.NodeMaxId;
        var isFirst     = true;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity  = store.GetNodeById(n).Entity;
            if (entity == null) {
                continue;
            }
            if (isFirst) {
                isFirst = false;
            } else {
                stream.Write(Comma);
            }
            converter.GameToDataEntity(entity, dataEntity, true);
            writer.InitSerializer();
            WriteDataEntity(dataEntity);
            stream.Write(writer.json.AsSpan());
        }
        stream.Write(ArrayEnd);
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
}
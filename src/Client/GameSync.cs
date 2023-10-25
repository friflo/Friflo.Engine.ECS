// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Burst;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Client;

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

    public GameSync (GameEntityStore store, GameClient client) {
        this.store      = store;
        this.client     = client;
        localEntities   = client.entities.Local;
        converter       = new EntityConverter();
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
            dataEntity = converter.GameToDataEntity(entity, dataEntity);
            client.entities.Upsert(dataEntity);
        }
        client.SyncTasksSynchronous();
    }
    
    public JsonValue WriteSceneFile()
    {
        writer.InitSerializer();
        writer.SetPretty(true);
        writer.ArrayStart(false);
        var dataEntity  = new DataEntity();
        var nodeMax     = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            ref var node    = ref store.GetNodeById(n);
            var entity      = node.Entity;
            if (entity == null) {
                continue;
            }
            converter.GameToDataEntity(entity, dataEntity);
            WriteDataEntity(dataEntity);
        }
        writer.ArrayEnd();
        return new JsonValue(writer.json);
    }
    
    private static readonly     Bytes   PidKey          = new Bytes("pid");
    private static readonly     Bytes   ChildrenKey     = new Bytes("children");
    private static readonly     Bytes   ComponentsKey   = new Bytes("components");
    private static readonly     Bytes   TagsKey         = new Bytes("tags");
    
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
        var components = dataEntity.components;
        if (!components.IsNull())
        {
            var componentBytes = new Bytes { buffer = components.MutableArray, end = components.Count };
            writer.MemberBytes(ComponentsKey, componentBytes);
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
}
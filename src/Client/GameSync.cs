// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Client;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public sealed class GameSync
{
    private readonly    GameEntityStore                 store;
    private readonly    GameClient                      client;
    private readonly    LocalEntities<long, DataEntity> localEntities;
    private readonly    EntityConverter                 converter;

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
}
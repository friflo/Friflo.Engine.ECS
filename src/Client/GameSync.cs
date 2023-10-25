// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public sealed class GameSync
{
    private readonly    GameEntityStore                 store;
    private readonly    GameClient                      client;
    private readonly    EntityConverter                 converter;

    public GameSync (GameEntityStore store, GameClient client) {
        this.store  = store;
        this.client = client;
        converter   = new EntityConverter();
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
        foreach (var node in store.Nodes) {
            var entity = node.Entity;
            if (entity == null) {
                continue;
            }
            var dataEntity = converter.GameToDataEntity(entity);
            client.entities.Upsert(dataEntity);
        }
        client.SyncTasksSynchronous();
    }
}
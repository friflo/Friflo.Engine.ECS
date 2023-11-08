// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Client;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public sealed class GameDataSync
{
    public              GameEntityStore                 Store => store;
    
    private readonly    GameEntityStore                 store;
    private readonly    GameClient                      client;
    private readonly    LocalEntities<long, DataEntity> localEntities;
    private readonly    EntityConverter                 converter;
    private readonly    Dictionary<int, EntityChange>   entityChanges;


    public GameDataSync (GameEntityStore store, GameClient client) {
        this.store      = store     ?? throw new ArgumentNullException(nameof(store));
        this.client     = client    ?? throw new ArgumentNullException(nameof(client));
        localEntities   = client.entities.Local;
        converter       = new EntityConverter();
        entityChanges   = new Dictionary<int, EntityChange>();
        client.entities.WritePretty = true;
    }
    
    public void ClearData() {
        client.Reset();
    }
    
    public void LoadGameEntities()
    {
        var query = client.entities.QueryAll();
        client.SyncTasks().Wait(); // todo enable synchronous queries in MemoryDatabase
        ConvertDataEntities(query.Result);
    }
    
    public async Task LoadGameEntitiesAsync()
    {
        var query = client.entities.QueryAll();
        await client.SyncTasks();
        ConvertDataEntities(query.Result);
    }
    
    private void ConvertDataEntities(List<DataEntity> dataEntities)
    {
        foreach (var data in dataEntities) {
            converter.DataToGameEntity(data, store, out _);
        }
    }
    
    public void StoreGameEntities()
    {
        UpsertDataEntities();
        client.SyncTasksSynchronous();
    }
    
    public async Task StoreGameEntitiesAsync()
    {
        UpsertDataEntities();
        await client.SyncTasks();
    }
    
    private void UpsertDataEntities()
    {
        var nodeMax = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity = store.GetNodeById(n).Entity;
            UpsertDataEntity(entity);
        }
    }
    
    private void UpsertDataEntity(GameEntity entity)
    {
        if (entity == null) {
            return;
        }
        if (!localEntities.TryGetEntity(entity.Id, out DataEntity dataEntity)) {
            dataEntity = new DataEntity();
        }
        dataEntity = converter.GameToDataEntity(entity, dataEntity, true);
        client.entities.Upsert(dataEntity);
    }
    
    public void SubscribeDatabaseChanges()
    {
        client.entities.SubscribeChanges(Change.All, EntitiesChangeHandler);
        client.SyncTasksSynchronous();
    }
    
    public async Task SubscribeDatabaseChangesAsync()
    {
        client.entities.SubscribeChanges(Change.All, EntitiesChangeHandler);
        await client.SyncTasks();
    }
    
    /// <summary>SYNC: <see cref="DataEntity"/> -> <see cref="GameEntityStore"/></summary>
    private void EntitiesChangeHandler(Changes<long, DataEntity> changes, EventContext context)
    {
        Console.WriteLine($"Changes: {changes}");
        if (context.IsOrigin) {
            return;
        }
        foreach (var upsert in changes.Upserts) {
            converter.DataToGameEntity(upsert.entity, store, out _);
        }
        foreach (var delete in changes.Deletes) {
            ref var node = ref store.GetNodeByPid(delete.key);
            node.Entity.DeleteEntity();
        }
    }
    
    public void UpsertDataEntity(int entityId)
    {
        entityChanges[entityId] = EntityChange.Upsert;
    }
    
    public void DeleteDataEntity(int entityId)
    {
        entityChanges[entityId] = EntityChange.Delete;
    }

    /// <summary>Sync accumulated entity changes</summary>
    public async Task SyncChangesAsync()
    {
        foreach (var pair in entityChanges)
        {
            var id = pair.Key;
            switch (pair.Value) {
                case EntityChange.Upsert:
                    var entity = store.GetNodeById(id).Entity;
                    UpsertDataEntity(entity);
                    continue;
                case EntityChange.Delete:
                    client.entities.Delete(id);
                    continue;
            }
        }
        entityChanges.Clear();

        await client.SyncTasks();
    }
}

internal enum EntityChange
{
    Upsert  = 0,
    Delete  = 1,
}
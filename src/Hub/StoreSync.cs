// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox.Hub.Client;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Engine.Hub;

[CLSCompliant(true)]
public sealed class StoreSync
{
    public              EntityStore                     Store => store;
    
    private readonly    EntityStore                     store;
    private readonly    StoreClient                     client;
    private readonly    LocalEntities<long, DataEntity> localEntities;
    private readonly    EntityConverter                 converter;
    private readonly    Dictionary<int, EntityChange>   entityChanges;
    private readonly    List<DataEntity>                upsertBuffer;
    private readonly    List<long>                      deleteBuffer;
    private readonly    HashSet<int>                    idSet;


    public StoreSync (EntityStore store, StoreClient client) {
        this.store      = store     ?? throw new ArgumentNullException(nameof(store));
        this.client     = client    ?? throw new ArgumentNullException(nameof(client));
        localEntities   = client.entities.Local;
        converter       = new EntityConverter();
        entityChanges   = new Dictionary<int, EntityChange>();
        upsertBuffer    = new List<DataEntity>();
        deleteBuffer    = new List<long>();
        idSet           = new HashSet<int>();
        client.entities.WritePretty = true;
    }
    
    public void ClearData() {
        client.Reset();
    }
    
    public List<string> LoadEntities()
    {
        var query = client.entities.QueryAll();
        client.SyncTasks().Wait(); // todo enable synchronous queries in MemoryDatabase
        return ConvertDataEntities(query.Result);
    }
    
    public async Task<List<string>> LoadEntitiesAsync()
    {
        var query = client.entities.QueryAll();
        await client.SyncTasks();
        return ConvertDataEntities(query.Result);
    }
    
    private List<string> ConvertDataEntities(List<DataEntity> dataEntities)
    {
        var errors = new List<string>();
        foreach (var data in dataEntities) {
            converter.DataEntityToEntity(data, store, out string error);
            if (error == null) {
                continue;
            }
            errors.Add($"entity: {data.pid} - {error}");
        }
        return errors;
    }
    
    public void StoreEntities()
    {
        UpsertDataEntities();
        client.SyncTasksSynchronous();
    }
    
    public async Task StoreEntitiesAsync()
    {
        UpsertDataEntities();
        await client.SyncTasks();
    }
    
    private void UpsertDataEntities()
    {
        var nodeMax = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++)
        {
            var entity = store.GetEntityById(n);
            AddDataEntityUpsert(entity);
        }
        CreateChangeTasks();
    }
    
    private void AddDataEntityUpsert(Entity entity)
    {
        if (entity.IsNull) {
            return;
        }
        if (!localEntities.TryGetEntity(entity.Id, out DataEntity dataEntity)) {
            dataEntity = new DataEntity();
        }
        dataEntity = converter.EntityToDataEntity(entity, dataEntity, true);
        upsertBuffer.Add(dataEntity);
    }
    
    /// <summary>
    /// Creates an upsert task containing all <see cref="DataEntity"/>'s if needed.<br/>
    /// Creates a delete task containing all deleted <see cref="DataEntity"/>'s if needed.
    /// </summary>
    private void CreateChangeTasks()
    {
        if (upsertBuffer.Count > 0) {
            client.entities.UpsertRange(upsertBuffer);
            upsertBuffer.Clear();
        }
        if (deleteBuffer.Count > 0) {
            client.entities.DeleteRange(deleteBuffer);
            deleteBuffer.Clear();
        }
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
    
    /// <summary>SYNC: <see cref="DataEntity"/> -> <see cref="EntityStore"/></summary>
    private void EntitiesChangeHandler(Changes<long, DataEntity> changes, EventContext context)
    {
        // Console.WriteLine($"Changes: {changes}");
        if (context.IsOrigin) {
            return;
        }
        idSet.Clear();
        foreach (var upsert in changes.Upserts) {
            var entity = converter.DataEntityToEntity(upsert.entity, store, out _);
            idSet.Add(entity.Id);
        }
        foreach (var delete in changes.Deletes) {
            var entity = store.GetEntityByPid(delete.key);
            entity.DeleteEntity();
        }
        // Send event. See: SEND_EVENT notes
        var args = new EntitiesChanged(idSet);
        store.CastEntitiesChanged(null, args);
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
            var id      = pair.Key;
            switch (pair.Value) {
                case EntityChange.Upsert:
                    var entity = store.GetEntityById(id);
                    AddDataEntityUpsert(entity);
                    continue;
                case EntityChange.Delete:
                    var pid = store.IdToPid(id);
                    deleteBuffer.Add(pid);
                    continue;
            }
        }
        entityChanges.Clear();
        CreateChangeTasks();
        await client.SyncTasks();
    }
}

internal enum EntityChange
{
    Upsert  = 0,
    Delete  = 1,
}
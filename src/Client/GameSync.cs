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
    // private readonly    LocalEntities<long, DataEntity> entities;
    private readonly    EntityConverter                 converter;

    public GameSync (GameEntityStore store, GameClient client) {
        this.store  = store;
        this.client = client;
        // entities    = client.entities.Local;
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
    
        
    /*
    /// <summary>
    /// Stores the given <see cref="GameEntity"/> as a <see cref="DataEntity"/>
    /// </summary>
    public DataEntity AddGameEntity(GameEntity entity)
    {
        if (entity == null) {
            throw new ArgumentNullException(nameof(entity));
        }
        var entityStore = entity.Store;
        if (entityStore != store) {
            throw EntityStore.InvalidStoreException(nameof(entity));
        }
        var pid = store.GetNodeById(entity.Id).Pid;
        if (!entities.TryGetEntity(pid, out var dataEntity)) {
            dataEntity = new DataEntity { pid = pid };
            entities.Add(dataEntity);
        }
        converter.GameToDataEntity(entity, dataEntity);
        return dataEntity;
    }
    
    /// <summary>
    /// Loads the entity with given <paramref name="pid"/> as a <see cref="GameEntity"/>
    /// </summary>
    /// <returns>an <see cref="StoreOwnership.attached"/> entity</returns>
    public GameEntity GetGameEntity(long pid, out string error)
    {
        // --- stored DataEntity references have an identity - their reference and their pid   
        if (!entities.TryGetEntity(pid, out var dataEntity)) {
            error = $"entity not found. pid: {pid}";
            return null;
        }
        return converter.DataToGameEntity(dataEntity, store, out error);
    } */
}
// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Client;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public sealed class GameDatabase
{
    public              LocalEntities<long, DatabaseEntity> Local => local;
    
    private readonly    GameEntityStore                     store;
    private readonly    LocalEntities<long, DatabaseEntity> local;
    private readonly    EntityConverter                     converter;

    public GameDatabase (GameEntityStore store, GameClient client) {
        this.store  = store;
        local       = client.entities.Local;
        converter   = new EntityConverter();
    }
        
    /// <summary>
    /// Stores the given <see cref="GameEntity"/> as a <see cref="DatabaseEntity"/> in the <see cref="GameDatabase"/>
    /// </summary>
    public DatabaseEntity StoreEntity(GameEntity entity)
    {
        if (entity == null) {
            throw new ArgumentNullException(nameof(entity));
        }
        var entityStore = entity.Store;
        if (entityStore != store) {
            throw EntityStore.InvalidStoreException(nameof(entity));
        }
        var pid = store.GetNodeById(entity.Id).Pid;
        if (!local.TryGetEntity(pid, out var dbEntity)) {
            dbEntity = new DatabaseEntity { pid = pid };
            local.Add(dbEntity);
        }
        converter.GameToDatabaseEntity(entity, dbEntity);
        return dbEntity;
    }
    
    /// <summary>
    /// Loads the given <see cref="DatabaseEntity"/> as a <see cref="GameEntity"/> from the <see cref="GameDatabase"/>
    /// </summary>
    /// <returns>an <see cref="StoreOwnership.attached"/> entity</returns>
    public GameEntity LoadEntity(DatabaseEntity databaseEntity, out string error)
    {
        // --- stored DatabaseEntity references have an identity - their reference and their pid   
        if (!local.TryGetEntity(databaseEntity.pid, out var storedEntity)) {
            storedEntity = new DatabaseEntity();
        }
        // --- copy all fields to eliminate side effects by mutations on the passed databaseEntity
        storedEntity.pid        = databaseEntity.pid;
        storedEntity.children   = databaseEntity.children?.ToList();
        storedEntity.components = new JsonValue(databaseEntity.components);
        storedEntity.tags       = databaseEntity.tags?.ToList();
        storedEntity.sceneName  = databaseEntity.sceneName;
        storedEntity.prefab     = databaseEntity.prefab;
        storedEntity.modify     = databaseEntity.modify;
        
        return converter.DatabaseToGameEntity(storedEntity, store, out error);
    }
}
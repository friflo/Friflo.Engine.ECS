// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox.Hub.Client;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public sealed class GameDatabase
{
    public              LocalEntities<long, DatabaseEntity> Entities => entities;
    
    private readonly    GameEntityStore                     store;
    private readonly    LocalEntities<long, DatabaseEntity> entities;
    private readonly    EntityConverter                     converter;

    public GameDatabase (GameEntityStore store, GameClient client) {
        this.store  = store;
        entities    = client.entities.Local;
        converter   = new EntityConverter();
    }
        
    /// <summary>
    /// Stores the given <see cref="GameEntity"/> as a <see cref="DatabaseEntity"/> in the <see cref="GameDatabase"/>
    /// </summary>
    public DatabaseEntity AddGameEntity(GameEntity entity)
    {
        if (entity == null) {
            throw new ArgumentNullException(nameof(entity));
        }
        var entityStore = entity.Store;
        if (entityStore != store) {
            throw EntityStore.InvalidStoreException(nameof(entity));
        }
        var pid = store.GetNodeById(entity.Id).Pid;
        if (!entities.TryGetEntity(pid, out var dbEntity)) {
            dbEntity = new DatabaseEntity { pid = pid };
            entities.Add(dbEntity);
        }
        converter.GameToDatabaseEntity(entity, dbEntity);
        return dbEntity;
    }
    
    /// <summary>
    /// Loads the entity with given <paramref name="pid"/> as a <see cref="GameEntity"/> from the <see cref="GameDatabase"/>
    /// </summary>
    /// <returns>an <see cref="StoreOwnership.attached"/> entity</returns>
    public GameEntity GetAsGameEntity(long pid, out string error)
    {
        // --- stored DatabaseEntity references have an identity - their reference and their pid   
        if (!entities.TryGetEntity(pid, out var databaseEntity)) {
            error = $"entity not found. pid: {pid}";
            return null;
        }
        return converter.DatabaseToGameEntity(databaseEntity, store, out error);
    }
}
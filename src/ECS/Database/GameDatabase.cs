// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Friflo.Json.Fliox;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.ECS.Database;

public interface IGameDatabaseSync
{
    bool TryGetEntity (long pid, out DatabaseEntity databaseEntity);
    void AddEntity    (DatabaseEntity databaseEntity);
}

public class GameDatabase
{
    private readonly    GameEntityStore     store;
    private readonly    IGameDatabaseSync   sync;
    
    public GameDatabase (GameEntityStore store, IGameDatabaseSync sync) {
        this.store      = store;
        this.sync  = sync;
    }
        
    public DatabaseEntity StoreEntity(GameEntity entity)
    {
        if (entity == null) {
            throw new ArgumentNullException(nameof(entity));
        }
        var entityStore = entity.archetype.gameEntityStore;
        if (entityStore != store) {
            throw EntityStore.InvalidStoreException(nameof(entity));
        }
        if (!sync.TryGetEntity(entity.id, out var dbEntity)) {
            dbEntity = new DatabaseEntity { pid = entity.id };
            sync.AddEntity(dbEntity);
        }
        entityStore.StoreEntity(entity, dbEntity);
        return dbEntity;
    }
    
    /// <returns>an <see cref="StoreOwnership.attached"/> entity</returns>
    public GameEntity LoadEntity(DatabaseEntity databaseEntity, out string error)
    {
        if (databaseEntity == null) {
            throw new ArgumentNullException(nameof(databaseEntity));
        }
        // --- stored DatabaseEntity references have an identity - their reference and their pid   
        if (!sync.TryGetEntity(databaseEntity.pid, out var storedNode)) {
            storedNode = new DatabaseEntity();
        }
        // --- copy all fields to eliminate side effects by mutations on the passed databaseEntity
        storedNode.pid          = databaseEntity.pid;
        storedNode.children     = databaseEntity.children?.ToList();
        storedNode.components   = new JsonValue(databaseEntity.components);
        storedNode.tags         = databaseEntity.tags?.ToList();
        storedNode.sceneName    = databaseEntity.sceneName;
        storedNode.prefab       = databaseEntity.prefab;
        storedNode.modify       = databaseEntity.modify;
        
        return store.LoadEntity(storedNode, out error);
    }
}
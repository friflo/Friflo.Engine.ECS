// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Friflo.Json.Fliox;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.ECS.Database;

[CLSCompliant(true)]
public sealed class GameDatabase
{
    private readonly    GameEntityStore     store;
    private readonly    IDatabaseSync       sync;
    
    public GameDatabase (GameEntityStore store, IDatabaseSync sync) {
        this.store  = store;
        this.sync   = sync;
    }
        
    /// <summary>
    /// Stores the given <see cref="GameEntity"/> as a <see cref="DatabaseEntity"/> in the <see cref="GameDatabase"/>
    /// </summary>
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
    
    /// <summary>
    /// Loads the given <see cref="DatabaseEntity"/> as a <see cref="GameEntity"/> from the <see cref="GameDatabase"/>
    /// </summary>
    /// <returns>an <see cref="StoreOwnership.attached"/> entity</returns>
    public GameEntity LoadEntity(DatabaseEntity databaseEntity, out string error)
    {
        if (databaseEntity == null) {
            throw new ArgumentNullException(nameof(databaseEntity));
        }
        // --- stored DatabaseEntity references have an identity - their reference and their pid   
        if (!sync.TryGetEntity(databaseEntity.pid, out var storedEntity)) {
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
        
        return store.LoadEntity(storedEntity, out error);
    }
}
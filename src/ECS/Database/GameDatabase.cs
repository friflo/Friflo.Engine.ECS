// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.ECS.Database;

public interface IGameDatabaseSync
{
    bool TryGetDataNode (long pid, out DataNode dataNode);
    void AddDataNode    (DataNode dataNode);
}

public class GameDatabase
{
    private readonly    GameEntityStore     store;
    private readonly    IGameDatabaseSync   databaseSync;
    
    public GameDatabase (GameEntityStore store, IGameDatabaseSync databaseSync) {
        this.store      = store;
        this.databaseSync  = databaseSync;
    }
        
    public DataNode StoreEntity(GameEntity entity)
    {
        if (entity == null) {
            throw new ArgumentNullException(nameof(entity));
        }
        var entityStore = entity.archetype.gameEntityStore;
        if (entityStore != store) {
            throw EntityStore.InvalidStoreException(nameof(entity));
        }
        if (!databaseSync.TryGetDataNode(entity.id, out var dataNode)) {
            dataNode = new DataNode { pid = entity.id };
            databaseSync.AddDataNode(dataNode);
        }
        entityStore.StoreEntity(entity, dataNode);
        return dataNode;
    }
    
    /// <returns>an <see cref="StoreOwnership.attached"/> entity</returns>
    public GameEntity LoadEntity(DataNode dataNode, out string error)
    {
        if (dataNode == null) {
            throw new ArgumentNullException(nameof(dataNode));
        }
        return store.LoadEntity(dataNode, out error);
    }
}
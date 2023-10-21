// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace Friflo.Fliox.Engine.ECS.Sync;

public interface IEntityDatabaseSync
{
    bool TryGetDataNode (long pid, out DataNode dataNode);
    void AddDataNode    (DataNode dataNode);
}

public class GameDatabase
{
    private readonly    GameEntityStore     store;
    private readonly    IEntityDatabaseSync databaseSync;
    
    public GameDatabase (GameEntityStore store, IEntityDatabaseSync databaseSync) {
        this.store      = store;
        this.databaseSync  = databaseSync;
    }
        
    public DataNode DataNodeFromEntity(GameEntity entity)
    {
        if (entity == null) {
            throw new ArgumentNullException(nameof(entity));
        }
        var entityStore = entity.archetype.gameEntityStore;
        if (entityStore != store) {
            throw new InvalidOperationException();
        }
        if (!databaseSync.TryGetDataNode(entity.id, out var dataNode)) {
            dataNode = new DataNode { pid = entity.id };
            databaseSync.AddDataNode(dataNode);
        }
        entityStore.DataNodeFromEntity(entity, dataNode);
        return dataNode;
    }
    
    public GameEntity DataNodeToEntity(DataNode dataNode, out string error)
    {
        if (dataNode == null) {
            throw new ArgumentNullException(nameof(dataNode));
        }
        return store.DataNodeToEntity(dataNode, out error);
    }
}
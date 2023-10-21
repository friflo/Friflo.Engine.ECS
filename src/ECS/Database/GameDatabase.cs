// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Friflo.Json.Fliox;

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
    private readonly    IGameDatabaseSync   sync;
    
    public GameDatabase (GameEntityStore store, IGameDatabaseSync sync) {
        this.store      = store;
        this.sync  = sync;
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
        if (!sync.TryGetDataNode(entity.id, out var dataNode)) {
            dataNode = new DataNode { pid = entity.id };
            sync.AddDataNode(dataNode);
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
        // --- stored DataNode's references have an identity - their reference and their pid   
        if (!sync.TryGetDataNode(dataNode.pid, out var storedNode)) {
            storedNode = new DataNode();
        }
        // --- copy all fields to eliminate side effects by mutations on the passed dataNode
        storedNode.pid          = dataNode.pid;
        storedNode.children     = dataNode.children?.ToList();
        storedNode.components   = new JsonValue(dataNode.components);
        storedNode.tags         = dataNode.tags?.ToList();
        storedNode.sceneName    = dataNode.sceneName;
        storedNode.prefab       = dataNode.prefab;
        storedNode.modify       = dataNode.modify;
        
        return store.LoadEntity(storedNode, out error);
    }
}
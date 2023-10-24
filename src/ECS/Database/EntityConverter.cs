// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable ConvertConstructorToMemberInitializers
using System;

namespace Friflo.Fliox.Engine.ECS.Database;

public class EntityConverter
{
    internal readonly   ComponentReader reader;
    internal readonly   ComponentWriter writer;
    
    public static readonly EntityConverter Default = new EntityConverter();
    
    public EntityConverter() {
        reader = new ComponentReader();
        writer = new ComponentWriter();
    }
    
    public DatabaseEntity GameEntityToDatabaseEntity(GameEntity gameEntity)
    {
        if (gameEntity == null) {
            throw new ArgumentNullException(nameof(gameEntity));
        }
        var store = gameEntity.archetype.gameEntityStore;
        var pid             = store.GetNodeById(gameEntity.id).pid;
        var databaseEntity  = new DatabaseEntity { pid = pid };
        store.GameEntityToDatabaseEntity(gameEntity, databaseEntity, writer);
        return databaseEntity;
    }
    
    public GameEntity DatabaseEntityToGameEntity(DatabaseEntity databaseEntity, GameEntityStore store, out string error)
    {
        if (databaseEntity == null) {
            throw new ArgumentNullException(nameof(databaseEntity));
        }
        return store.DatabaseEntityToGameEntity(databaseEntity, out error, reader);
    }
}
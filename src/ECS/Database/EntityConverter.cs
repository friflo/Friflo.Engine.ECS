// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable ConvertConstructorToMemberInitializers
using System;

namespace Friflo.Fliox.Engine.ECS.Database;

public class EntityConverter
{
    private  readonly   ComponentReader reader;
    private  readonly   ComponentWriter writer;
    
    public static readonly EntityConverter Default = new EntityConverter();
    
    public EntityConverter() {
        reader = new ComponentReader();
        writer = new ComponentWriter();
    }
    
    public DatabaseEntity GameToDatabaseEntity(GameEntity gameEntity, DatabaseEntity databaseEntity = null)
    {
        if (gameEntity == null) {
            throw new ArgumentNullException(nameof(gameEntity));
        }
        var store           = gameEntity.archetype.gameEntityStore;
        var pid             = store.GetNodeById(gameEntity.id).pid;
        databaseEntity    ??= new DatabaseEntity();
        databaseEntity.pid  = pid;
        store.GameToDatabaseEntity(gameEntity, databaseEntity, writer);
        return databaseEntity;
    }
    
    public GameEntity DatabaseToGameEntity(DatabaseEntity databaseEntity, GameEntityStore store, out string error)
    {
        if (databaseEntity == null) {
            throw new ArgumentNullException(nameof(databaseEntity));
        }
        return store.DatabaseToGameEntity(databaseEntity, out error, reader);
    }
}
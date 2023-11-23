// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable ConvertConstructorToMemberInitializers
using System;

namespace Friflo.Fliox.Engine.ECS.Sync;

public class EntityConverter
{
    private  readonly   ComponentReader reader;
    private  readonly   ComponentWriter writer;
    
    public static readonly EntityConverter Default = new EntityConverter();
    
    public EntityConverter() {
        reader = new ComponentReader();
        writer = new ComponentWriter();
    }
    
    public DataEntity GameToDataEntity(Entity gameEntity, DataEntity dataEntity = null, bool pretty = false)
    {
        if (gameEntity == null) {
            throw new ArgumentNullException(nameof(gameEntity));
        }
        var store       = gameEntity.archetype.entityStore;
        var pid         = store.GetNodeById(gameEntity.id).pid;
        dataEntity    ??= new DataEntity();
        dataEntity.pid  = pid;
        store.GameToDataEntity(gameEntity, dataEntity, writer, pretty);
        return dataEntity;
    }
    
    public Entity DataToGameEntity(DataEntity dataEntity, EntityStore store, out string error)
    {
        if (dataEntity == null) {
            throw new ArgumentNullException(nameof(dataEntity));
        }
        return store.DataToGameEntity(dataEntity, out error, reader);
    }
}
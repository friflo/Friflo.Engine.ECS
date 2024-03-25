// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable ConvertConstructorToMemberInitializers
using System;

namespace Friflo.Engine.ECS.Serialize;

/// <summary>
/// Converts an <see cref="Entity"/> to a <see cref="DataEntity"/> and vice versa.
/// </summary>
public sealed class EntityConverter
{
    private  readonly   ComponentReader reader;
    private  readonly   ComponentWriter writer;
    
    /// <summary>
    /// An <see cref="EntityConverter"/> singleton. Must be used only from the main thread.
    /// </summary>
    public static readonly EntityConverter Default = new EntityConverter();
    
    public EntityConverter() {
        reader = new ComponentReader();
        writer = new ComponentWriter();
    }
    
    /// <summary>
    /// Returns the passed <see cref="Entity"/> as a <see cref="DataEntity"/> 
    /// </summary>
    public DataEntity EntityToDataEntity(Entity entity, DataEntity dataEntity, bool pretty)
    {
        if (entity.IsNull) {
            throw new ArgumentNullException(nameof(entity));
        }
        var store       = entity.archetype.entityStore;
        var pid         = store.IdToPid(entity.Id);
        dataEntity    ??= new DataEntity();
        dataEntity.pid  = pid;
        store.EntityToDataEntity(entity, dataEntity, writer, pretty);
        return dataEntity;
    }
    
    /// <summary>
    /// Add / update the passed <see cref="DataEntity"/> in the given <paramref name="store"/> and returns
    /// the added / updated <see cref="Entity"/>. 
    /// </summary>
    public Entity DataEntityToEntity(DataEntity dataEntity, EntityStore store, out string error)
    {
        if (dataEntity == null) {
            throw new ArgumentNullException(nameof(dataEntity));
        }
        return store.DataEntityToEntity(dataEntity, out error, reader);
    }
}
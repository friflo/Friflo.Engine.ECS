// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable ConvertConstructorToMemberInitializers
using System;
using System.Collections.Generic;
using Friflo.Json.Fliox;

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
        var store       = entity.GetStore() ?? throw EntityStoreBase.EntityArgumentNullException(entity, nameof(entity));
        var pid         = store.IdToPid(entity.Id);
        dataEntity    ??= new DataEntity();
        dataEntity.pid  = pid;
        store.EntityToDataEntity(entity, dataEntity, writer, pretty);
        return dataEntity;
    }
    
    public void EntityComponentsToJsonMembers(Entity entity, List<JsonValue> members)
    {
        if (entity.IsNull) {
            throw new ArgumentNullException(nameof(entity));
        }
        members.Clear();
        writer.Write(entity, members, false);
    }
    
    /// <summary>
    /// Add / update the passed <see cref="DataEntity"/> in the given <paramref name="store"/> and returns
    /// the added / updated <see cref="Entity"/>. 
    /// </summary>
    public Entity DataEntityToEntity(DataEntity dataEntity, EntityStore store, out string error)
    {
        var options = new ConvertOptions { preserve = true, preserveComponents = ComponentTypes.Get<TreeNode>() };
        return store.DataEntityToEntity(dataEntity, out error, reader, options);
    }
    
    /// <summary>
    /// Add / update the passed <see cref="DataEntity"/> in the given <paramref name="store"/> and returns
    /// the added / updated <see cref="Entity"/>.<br/>
    /// The specified <paramref name="componentTypes"/> and <paramref name="tags"/> are preserved if present on entity. 
    /// </summary>
    public Entity DataEntityToEntityPreserve(
        DataEntity          dataEntity,
        EntityStore         store,
        out string          error,
        in  ComponentTypes  componentTypes,
        in  Tags            tags)
    {
        var options = new ConvertOptions { preserveComponents = componentTypes, preserveTags = tags, preserve = true };
        return store.DataEntityToEntity(dataEntity, out error, reader, options);
    }
}

internal struct ConvertOptions
{
    internal ComponentTypes  preserveComponents;
    internal Tags            preserveTags;
    internal bool            preserve;
}
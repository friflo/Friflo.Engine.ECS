// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable ConvertConstructorToMemberInitializers
using System;
using System.Collections.Generic;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

namespace Friflo.Engine.ECS.Serialize;

/// <summary>
/// Converts an <see cref="Entity"/> to a <see cref="DataEntity"/> and vice versa.
/// </summary>
public sealed class EntityConverter
{
    private  readonly   ComponentReader reader;
    private  readonly   ComponentWriter writer;
    private static readonly    TypeStore       TypeStore       = new TypeStore();
    
    /// <summary>
    /// An <see cref="EntityConverter"/> singleton. Must be used only from the main thread.
    /// </summary>
    public static readonly EntityConverter Default = new EntityConverter();
    
    private static readonly object  TypeMapperGate  = new object();
    private static          bool    typeMappersInUse;
    
    /// <summary>
    /// Adds an <see cref="ITypeMatcher"/> used to find the JSON mapper of component types.<br/>
    /// Matchers added here are asked before the default mappers, so a matcher can replace how a component type is serialized.<br/>
    /// Must be called before the first entity is serialized or deserialized: the mapper of a type is chosen when the type is
    /// used the first time. Throws an <see cref="InvalidOperationException"/> if called afterwards.
    /// </summary>
    public static void AddTypeMatcher(ITypeMatcher matcher)
    {
        if (matcher == null) throw new ArgumentNullException(nameof(matcher));
        lock (TypeMapperGate) {
            ThrowIfTypeMappersInUse(nameof(AddTypeMatcher));
            TypeStore.typeResolver.AddGenericTypeMatcher(matcher);
        }
    }
    
    /// <summary>
    /// Adds a <see cref="TypeMapper"/> for exactly the type the mapper is created for.<br/>
    /// Must be called before the first entity is serialized or deserialized. Throws an <see cref="InvalidOperationException"/>
    /// if called afterwards.
    /// </summary>
    public static void AddTypeMapper(TypeMapper mapper)
    {
        if (mapper == null) throw new ArgumentNullException(nameof(mapper));
        lock (TypeMapperGate) {
            ThrowIfTypeMappersInUse(nameof(AddTypeMapper));
            TypeStore.typeResolver.AddTypeMapper(mapper);
        }
    }
    
    private static void ThrowIfTypeMappersInUse(string method)
    {
        if (typeMappersInUse) {
            throw new InvalidOperationException($"{method}() must be called before the first entity is serialized or deserialized. " +
                                                "The JSON mapper of a type is chosen when the type is used the first time.");
        }
    }
    
    /// <summary> Called by <see cref="ComponentReader"/> / <see cref="ComponentWriter"/> before they use a mapper. </summary>
    internal static void MarkTypeMappersInUse()
    {
        typeMappersInUse = true;
    }
    
    public EntityConverter() {
        reader = new ComponentReader(TypeStore);
        writer = new ComponentWriter(TypeStore);
    }
    
    public void SetMapperContext<T>(T context) where T : class, IMapperContext {
        reader.componentReader.SetMapperContext(context);
        writer.componentWriter.SetMapperContext(context);
    }
    
    /// <summary>
    /// Returns the passed <see cref="Entity"/> as a <see cref="DataEntity"/>
    /// <param name="entity">Source entity.</param>
    /// <param name="dataEntity">The target entity data is copied to.</param>
    /// <param name="pretty">
    /// If true each component in <see cref="DataEntity.components"/> JSON is written in a separate line.<br/>
    /// If false <see cref="DataEntity.components"/> JSON has no line feeds.
    /// </param>
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
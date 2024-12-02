// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Relations;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static class RelationExtensions
{
#region Entity
    /// <summary>
    /// Returns the relation of the <paramref name="entity"/> with the given <paramref name="key"/>.<br/>
    /// Executes in O(N) N: number of entity relations.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The relation is not found at the passed entity.</exception>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static ref TRelation GetRelation<TRelation, TKey>(this Entity entity, TKey key)
        where TRelation : struct, IRelation<TKey>
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return ref AbstractEntityRelations.GetRelation<TRelation, TKey>(entity.store, entity.Id, key);
    }
    
    /// <summary>
    /// Returns the relation of the <paramref name="entity"/> with the given <paramref name="key"/>.<br/>
    /// Executes in O(N) N: number of entity relations.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static bool TryGetRelation<TRelation, TKey>(this Entity entity, TKey key, out TRelation value)
        where TRelation : struct, IRelation<TKey>
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return AbstractEntityRelations.TryGetRelation(entity.store, entity.Id, key, out value);
    }
    
    /// <summary>
    /// Returns all unique relations of the passed <paramref name="entity"/>.<br/>
    /// Executes in O(1). In case <typeparamref name="TRelation"/> is a <see cref="ILinkRelation"/> it returns all linked entities.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static Relations<TRelation> GetRelations<TRelation>(this Entity entity)
        where TRelation : struct, IRelation
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return AbstractEntityRelations.GetRelations<TRelation>(entity.store, entity.Id);
    }
    
    /// <summary>
    /// Add the relation with the specified <typeparamref name="TRelation"/> type to the entity.<br/>
    /// Executes in O(1)
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    /// <returns>true - relation is newly added to the entity.<br/> false - relation is updated.</returns>
    public static bool AddRelation<TRelation>(this Entity entity, in TRelation component)
        where TRelation : struct, IRelation
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return AbstractEntityRelations.AddRelation(entity.store, entity.Id, component);
    }
    
    /// <summary>
    /// Removes the relation with the specified <paramref name="key"/> from an entity.<br/>
    /// Executes in O(N) N: number of relations of the specific entity.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    /// <returns>true if the entity contained a relation of the given type before. </returns>
    public static bool RemoveRelation<TRelation, TKey>(this Entity entity, TKey key)
        where TRelation : struct, IRelation<TKey>
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity); 
        return AbstractEntityRelations.RemoveRelation<TRelation, TKey>(entity.store, entity.Id, key);
    }
    
    /// <summary>
    /// Removes the specified link relation <paramref name="target"/> from an entity.<br/>
    /// Executes in O(N) N: number of link relations of the specified entity.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    /// <returns>true if the entity contained a link relation of the given type before. </returns>
    public static bool RemoveRelation<TRelation>(this Entity entity, Entity target)
        where TRelation : struct, ILinkRelation
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return AbstractEntityRelations.RemoveRelation<TRelation, Entity>(entity.store, entity.Id, target);
    }
    
    /// <summary>
    /// Return the entities with a link relation referencing the <paramref name="target"/> entity of the passed <see cref="IRelation"/> type.<br/>
    /// Executes in O(1).
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static EntityLinks<TRelation> GetIncomingLinks<TRelation>(this Entity target)
        where TRelation: struct, ILinkRelation
    {
        if (target.IsNull) throw EntityStoreBase.EntityNullException(target);
        var entities = AbstractEntityRelations.GetIncomingLinkRelations(target.store, target.Id, StructInfo<TRelation>.Index, out var relations);
        return new EntityLinks<TRelation>(target, entities, relations);
    }
    #endregion
    
#region EntityStore
    /// <summary>
    /// Return the storage for all entity relations of the specified <typeparamref name="TRelation"/> type.
    /// </summary>
    public static EntityRelations<TRelation> EntityRelations<TRelation>(this EntityStore store)
        where TRelation : struct, IRelation
    {
        var relations = AbstractEntityRelations.GetEntityRelations(store, StructInfo<TRelation>.Index);
        return new EntityRelations<TRelation>(relations);
    }
    
    /// <summary>
    /// Obsolete: Use <see cref="ECS.EntityRelations{TRelation}.Entities"/><br/>
    /// Returns a collection of entities having one or more relations of the specified <typeparamref name="TRelation"/> type.<br/>
    /// Executes in O(1).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when relations are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get all entities including their relations (the cartesian product aka CROSS JOIN) use<br/>
    ///     <see cref="ECS.EntityRelations{TRelation}.Pairs"/>
    ///   </item>
    /// </list>
    /// </remarks>
    [Obsolete("replace with property: EntityRelations<TRelation>().Entities")]
    [ExcludeFromCodeCoverage]
    public static EntityReadOnlyCollection GetAllEntitiesWithRelations<TRelation>(this EntityStore store)
        where TRelation : struct, IRelation
    {
        var relations = AbstractEntityRelations.GetEntityRelations(store, StructInfo<TRelation>.Index);
        return new EntityReadOnlyCollection(store, relations.positionMap.Keys);
    }
    
    /// <summary>
    /// Obsolete: Use <see cref="ECS.EntityRelations{TRelation}.For"/><br/>
    /// Iterates all entity relations of the specified <typeparamref name="TRelation"/> type.<br/>
    /// Executes in O(N) N: number of all entity relations.
    /// </summary>
    [Obsolete("replace with method: EntityRelations<TRelation>().For()")]
    [ExcludeFromCodeCoverage]
    public static void ForAllEntityRelations<TRelation>(this EntityStore store, ForEachEntity<TRelation> lambda)
        where TRelation : struct, IRelation
    {
        var relations = AbstractEntityRelations.GetEntityRelations(store, StructInfo<TRelation>.Index);
        relations.ForAllEntityRelations(lambda);
    }
    
    /// <summary>
    /// Obsolete: Use <see cref="ECS.EntityRelations{TRelation}.Pairs"/><br/> 
    /// Return all entity relations  of the specified <typeparamref name="TRelation"/> type.<br/>
    /// Executes in O(1).  Most efficient way to iterate all entity relations.
    /// </summary>
    [Obsolete("replace with property: EntityRelations<TRelation>().Pairs")]
    [ExcludeFromCodeCoverage]
    public static (Entities entities, Chunk<TRelation> relations) GetAllEntityRelations<TRelation>(this EntityStore store)
        where TRelation : struct, IRelation
    {
        var entityRelations = AbstractEntityRelations.GetEntityRelations(store, StructInfo<TRelation>.Index);
        return entityRelations.GetAllEntityRelations<TRelation>();
    }
    #endregion
}
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
    public static ref TComponent GetRelation<TComponent, TKey>(this Entity entity, TKey key)
        where TComponent : struct, IRelationComponent<TKey>
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return ref EntityRelations.GetRelation<TComponent, TKey>(entity.store, entity.Id, key);
    }
    
    /// <summary>
    /// Returns the relation of the <paramref name="entity"/> with the given <paramref name="key"/>.<br/>
    /// Executes in O(N) N: number of entity relations.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static bool TryGetRelation<TComponent, TKey>(this Entity entity, TKey key, out TComponent value)
        where TComponent : struct, IRelationComponent<TKey>
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return EntityRelations.TryGetRelation(entity.store, entity.Id, key, out value);
    }
    
    /// <summary>
    /// Returns all unique relation components of the passed <paramref name="entity"/>.<br/>
    /// Executes in O(1). In case <typeparamref name="TComponent"/> is a <see cref="ILinkRelation"/> it returns all linked entities.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static RelationComponents<TComponent> GetRelations<TComponent>(this Entity entity)
        where TComponent : struct, IRelationComponent
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return EntityRelations.GetRelations<TComponent>(entity.store, entity.Id);
    }
    
    /// <summary>
    /// Add the relation component with the specified <typeparamref name="TComponent"/> type to the entity.<br/>
    /// Executes in O(1)
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    /// <returns>true - relation is newly added to the entity.<br/> false - relation is updated.</returns>
    public static bool AddRelation<TComponent>(this Entity entity, in TComponent component)
        where TComponent : struct, IRelationComponent
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return EntityRelations.AddRelation(entity.store, entity.Id, component);
    }
    
    /// <summary>
    /// Removes the relation component with the specified <paramref name="key"/> from an entity.<br/>
    /// Executes in O(N) N: number of relations of the specific entity.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    /// <returns>true if the entity contained a relation of the given type before. </returns>
    public static bool RemoveRelation<TComponent, TKey>(this Entity entity, TKey key)
        where TComponent : struct, IRelationComponent<TKey>
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity); 
        return EntityRelations.RemoveRelation<TComponent, TKey>(entity.store, entity.Id, key);
    }
    
    /// <summary>
    /// Removes the specified link relation <paramref name="target"/> from an entity.<br/>
    /// Executes in O(N) N: number of link relations of the specified entity.
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    /// <returns>true if the entity contained a link relation of the given type before. </returns>
    public static bool RemoveRelation<TComponent>(this Entity entity, Entity target)
        where TComponent : struct, ILinkRelation
    {
        if (entity.IsNull) throw EntityStoreBase.EntityNullException(entity);
        return EntityRelations.RemoveRelation<TComponent, Entity>(entity.store, entity.Id, target);
    }
    
    /// <summary>
    /// Return the entities with a link relation referencing the <paramref name="target"/> entity of the passed <see cref="IRelationComponent"/> type.<br/>
    /// Executes in O(1).
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static EntityLinks<TComponent> GetIncomingLinks<TComponent>(this Entity target)
        where TComponent: struct, IRelationComponent
    {
        if (target.IsNull) throw EntityStoreBase.EntityNullException(target);
        var entities = EntityRelations.GetIncomingLinkRelations(target.store, target.Id, StructInfo<TComponent>.Index, out var relations);
        return new EntityLinks<TComponent>(target, entities, relations);
    }
    #endregion
    
#region EntityStore
    /// <summary>
    /// Returns a collection of entities having one or more relations of the specified <typeparamref name="TComponent"/> type.<br/>
    /// Executes in O(1).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when relations are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get all entities including their relations (the cartesian product aka CROSS JOIN) use<br/>
    ///     <see cref="GetAllEntityRelations{TComponent}"/>
    ///   </item>
    /// </list>
    /// </remarks>
    public static EntityReadOnlyCollection GetAllEntitiesWithRelations<TComponent>(this EntityStore store)
        where TComponent : struct, IRelationComponent
    {
        var relations = EntityRelations.GetEntityRelations(store, StructInfo<TComponent>.Index);
        return new EntityReadOnlyCollection(store, relations.positionMap.Keys);
    }
    
    /// <summary>
    /// Iterates all entity relations of the specified <typeparamref name="TComponent"/> type.<br/>
    /// Executes in O(N) N: number of all entity relations.
    /// </summary>
    public static void ForAllEntityRelations<TComponent>(this EntityStore store, ForEachEntity<TComponent> lambda)
        where TComponent : struct, IRelationComponent
    {
        var relations = EntityRelations.GetEntityRelations(store, StructInfo<TComponent>.Index);
        relations.ForAllEntityRelations(lambda);
    }
    
    /// <summary>
    /// Return all entity relations  of the specified <typeparamref name="TComponent"/> type.<br/>
    /// Executes in O(1).  Most efficient way to iterate all entity relations.
    /// </summary>
    public static (Entities entities, Chunk<TComponent> relations) GetAllEntityRelations<TComponent>(this EntityStore store)
        where TComponent : struct, IRelationComponent
    {
        var entityRelations = EntityRelations.GetEntityRelations(store, StructInfo<TComponent>.Index);
        return entityRelations.GetAllEntityRelations<TComponent>();
    }
    
    [ExcludeFromCodeCoverage]
    public static IReadOnlyCollection<Entity> GetAllLinkedEntities<TComponent>(this EntityStore store)
        where TComponent: struct, IRelationComponent
    {
        throw new NotImplementedException();
    }
    #endregion
}
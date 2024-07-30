// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide extension methods to query all or specific component values.<br/>
/// Enables to query all or specific entity links (relationships).
/// </summary>
public static class IndexExtensions
{
#region Entity
    /// <summary>
    /// Return the entities with a link component referencing the <paramref name="target"/> entity of the passed <see cref="ILinkComponent"/> type.<br/>
    /// Executes in O(1). 
    /// </summary>
    /// <exception cref="NullReferenceException">If the entity is null.</exception>
    public static EntityLinks<TComponent> GetIncomingLinks<TComponent>(this Entity target)
        where TComponent: struct, ILinkComponent
    {
        if (target.IsNull) throw EntityStoreBase.EntityNullException(target);
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(target.store, StructInfo<TComponent>.Index);
        return new EntityLinks<TComponent>(target, index.GetHasValueEntities(target), null);
    }
    #endregion
    
#region EntityStore
    /// <summary>
    /// Return the entities with the passed component value.<br/>
    /// Executes in O(1) with default index. 
    /// </summary>
    public static Entities GetEntitiesWithComponentValue<TComponent, TValue>(this EntityStore store, TValue value)
        where TComponent: struct, IIndexedComponent<TValue>
    {
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(store, StructInfo<TComponent>.Index);
        return index.GetHasValueEntities(value);
    }
    
    /// <summary>
    /// Returns all indexed component values of the passed <typeparamref name="TComponent"/> type.<br/>
    /// Executes in O(1). Each value in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when indexed component values are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get the entities having a specific component value use <see cref="GetEntitiesWithComponentValue{TComponent,TValue}"/>.
    ///   </item>
    ///   <item>
    ///     If <typeparamref name="TValue"/> is a class all collection values are not null.<br/>
    ///     Use <see cref="GetEntitiesWithComponentValue{TComponent,TValue}"/> to check if null is referenced.
    ///   </item>
    /// </list>
    /// </remarks>
    public static  IReadOnlyCollection<TValue> GetAllIndexedComponentValues<TComponent, TValue>(this EntityStore store)
        where TComponent: struct, IIndexedComponent<TValue>
    {
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(store, StructInfo<TComponent>.Index);
        return index.IndexedComponentValues;
    }
    
    /// <summary>
    /// Returns all entities linked by the specified <see cref="ILinkComponent"/> type.<br/>
    /// Executes in O(1). Each entity in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when component link values are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get the entities linking a specific entity use <see cref="GetIncomingLinks{TComponent}"/>.<br/>
    ///   </item>
    ///   <item>
    ///     The method is a specialized version of <see cref="GetAllIndexedComponentValues{TComponent,TValue}"/><br/>
    ///     using <c> TComponent = ILinkComponent</c> and <c>TValue = Entity</c>.  
    ///   </item>
    /// </list>
    /// </remarks>
    public static IReadOnlyCollection<Entity> GetAllLinkedEntities<TComponent>(this EntityStore store)
        where TComponent: struct, ILinkComponent
    {
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(store, StructInfo<TComponent>.Index);
        return index.IndexedComponentValues;
    }
    #endregion
}
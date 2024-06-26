// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace

using System.Collections.Generic;

namespace Friflo.Engine.ECS.Index;

/// <summary>
/// Provide extension methods to query all or specific component values.<br/>
/// Enables to query all or specific entity links (relationships).
/// </summary>
public static class IndexExtensions
{
#region Entity
    /// <summary>
    /// Return the entities with a component link referencing this entity of the passed <see cref="ILinkComponent"/> type.<br/>
    /// Executes in O(1). 
    /// </summary>
    /// <remarks>
    /// The method id a specialized version of <see cref="GetEntitiesWithComponentValue{TComponent,TValue}"/><br/>
    /// using <c> TComponent = IIndexedComponent&lt;Entity>, TValue = Entity</c> and <c>value = this</c>.  
    /// </remarks>
    public static Entities GetEntityReferences<TComponent>(this Entity entity) where TComponent: struct, ILinkComponent {
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(entity.store, StructInfo<TComponent>.Index);
        return index.GetHasValueEntities(entity);
    }
    #endregion
    
#region EntityStore
    /// <summary>
    /// Return the entities with the passed component value.<br/>
    /// Executes in O(1) with default index. 
    /// </summary>
    public static Entities GetEntitiesWithComponentValue<TComponent, TValue>(this EntityStore store, TValue value) where TComponent: struct, IIndexedComponent<TValue> {
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
    public static  IReadOnlyCollection<TValue> GetAllIndexedComponentValues<TComponent, TValue>(this EntityStore store) where TComponent: struct, IIndexedComponent<TValue> {
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
    ///     To get the entities linking a specific entity use <see cref="GetEntityReferences{TComponent}"/>.<br/>
    ///   </item>
    ///   <item>
    ///     The method id a specialized version of <see cref="GetAllIndexedComponentValues{TComponent,TValue}"/><br/>
    ///     using <c> TComponent = ILinkComponent</c> and <c>TValue = Entity</c>.  
    ///   </item>
    /// </list>
    /// </remarks>
    public static IReadOnlyCollection<Entity> GetAllLinkedEntities<TComponent>(this EntityStore store) where TComponent: struct, ILinkComponent {
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(store, StructInfo<TComponent>.Index);
        return index.IndexedComponentValues;
    }
    #endregion
}
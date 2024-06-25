// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public sealed partial class EntityStore
{
    /// <summary>
    /// Return the entities with the passed component value.<br/>
    /// Executes in O(1) with default index. 
    /// </summary>
    public Entities GetEntitiesWithComponentValue<TComponent, TValue>(TValue value) where TComponent: struct, IIndexedComponent<TValue> {
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(this, StructInfo<TComponent>.Index);
        return index.GetHasValueEntities(value);
    }
    
    /// <summary>
    /// Returns a collection of all indexed component values of the passed <typeparamref name="TComponent"/> type.<br/>
    /// Executes in O(1). Each value in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The collection changes when indexed component values are updated, removed or added.
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
    public IReadOnlyCollection<TValue> GetIndexedComponentValues<TComponent, TValue>() where TComponent: struct, IIndexedComponent<TValue> {
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(this, StructInfo<TComponent>.Index);
        return index.IndexedComponentValues;
    }
    
    /// <summary>
    /// Returns all entities linked by the specified <see cref="ILinkComponent"/> type.<br/>
    /// Executes in O(1). Each entity in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The collection changes when component link values are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get the entities linking a specific entity use <see cref="IndexExtensions.GetLinkingEntities{TComponent}"/>.<br/>
    ///   </item>
    ///   <item>
    ///     The method id a specialized version of <see cref="GetIndexedComponentValues{TComponent,TValue}"/><br/>
    ///     using <c> TComponent = IIndexedComponent&lt;Entity></c> and <c>TValue = Entity</c>.  
    ///   </item>
    /// </list>
    /// </remarks>
    public IReadOnlyCollection<Entity> GetLinkedEntities<TComponent>() where TComponent: struct, ILinkComponent {
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(this, StructInfo<TComponent>.Index);
        return index.IndexedComponentValues;
    }
}
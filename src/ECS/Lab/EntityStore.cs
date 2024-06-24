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
    internal Entities GetEntitiesWithComponentValue<TComponent, TValue>(TValue value) where TComponent: struct, IIndexedComponent<TValue> {
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(this, StructInfo<TComponent>.Index);
        return index.GetHasValueEntities(value);
    }
    
    /// <summary>
    /// Returns a collection of all indexed component values of the passed <typeparamref name="TComponent"/> type.<br/>
    /// Executes in O(1). Each value in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// - The collection changes when indexed component values are updated, removed or added.<br/>
    /// - To get the entities referenced by a component value use <see cref="GetEntitiesWithComponentValue{TComponent,TValue}"/>.<br/>
    /// - If <typeparamref name="TValue"/> is a class all collection values are not null.
    ///   Use <see cref="GetEntitiesWithComponentValue{TComponent,TValue}"/> to check if null is referenced.<br/>
    /// </remarks>
    internal IReadOnlyCollection<TValue> GetIndexedComponentValues<TComponent, TValue>() where TComponent: struct, IIndexedComponent<TValue> {
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(this, StructInfo<TComponent>.Index);
        return index.IndexedComponentValues;
    }
    
    internal IReadOnlyCollection<Entity> GetLinkedEntities<TComponent>() where TComponent: struct, ILinkComponent {
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(this, StructInfo<TComponent>.Index);
        return index.IndexedComponentValues;
    }
}
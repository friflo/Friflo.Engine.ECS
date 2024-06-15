// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal interface IIndexedComponent<out TValue> : IComponent
{
    TValue GetIndexedValue();
}

internal abstract class Clause
{
    internal abstract Entities GetEntities(EntityStore store);
}

internal class HasClause<TComponent, TValue> : Clause where TComponent : struct, IIndexedComponent<TValue>
{
    private readonly TValue value;
    
    internal HasClause(TValue value) {
        this.value = value;
    }
    
    internal override Entities GetEntities(EntityStore store)
    {
        var componentIndex  = store.extension.componentIndexes[StructInfo<TComponent>.Index];
        var typedIndex      = (InvertedIndex<TValue>)componentIndex;
        if (!typedIndex.map.TryGetValue(value, out var ids)) {
            return default;
        }
        return new Entities(ids, store, 0, ids.Length);
    }
}
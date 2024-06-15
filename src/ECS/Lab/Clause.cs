// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class Clause
{
    internal abstract Entities GetMatchingEntities(EntityStore store);
}

internal class HasClause<TComponent, TValue> : Clause where TComponent : struct, IIndexedComponent<TValue>
{
    private readonly TValue value;
    
    internal HasClause(TValue value) {
        this.value = value;
    }
    
    internal override Entities GetMatchingEntities(EntityStore store)
    {
        var index = (ComponentIndex<TValue>)store.extension.componentIndexes[StructInfo<TComponent>.Index];
        return index.GetMatchingEntities(value);
    }
}
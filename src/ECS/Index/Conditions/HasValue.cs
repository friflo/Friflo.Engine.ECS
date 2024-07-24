// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class HasValueCondition<TComponent, TValue> : ValueCondition
    where TComponent : struct, IIndexedComponent<TValue>
{
    private readonly TValue value;
    
    internal HasValueCondition(TValue value) {
        this.value = value;
    }
    
    internal override void AddMatchingEntities(EntityStore store, HashSet<int> idSet)
    {
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(store, StructInfo<TComponent>.Index);
        var entities = index.GetHasValueEntities(value);
        foreach (var id in entities.Ids) {
            idSet.Add(id);
        }
    }
}
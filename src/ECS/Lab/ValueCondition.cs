// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal abstract class ValueCondition
{
    internal abstract void AddMatchingEntities(EntityStore store, HashSet<int> idSet);
}

internal sealed class HasValueCondition<TComponent, TValue> : ValueCondition
    where TComponent : struct, IIndexedComponent<TValue>
{
    private readonly TValue value;
    
    internal HasValueCondition(TValue value) {
        this.value = value;
    }
    
    internal override void AddMatchingEntities(EntityStore store, HashSet<int> idSet)
    {
        var index = (ComponentIndex<TValue>)store.extension.componentIndexes[StructInfo<TComponent>.Index];
        var entities = index.GetMatchingEntities(value);
        foreach (var id in entities.Ids) {
            idSet.Add(id);
        }
    }
}

internal sealed class ValueInRangeCondition<TComponent, TValue> : ValueCondition
    where TComponent : struct, IIndexedComponent<TValue>
    where TValue     : IComparable<TValue>
{
    private readonly TValue min;
    private readonly TValue max;
    
    internal ValueInRangeCondition(TValue min, TValue max) {
        this.min = min;
        this.max = max;
    }
    
    internal override void AddMatchingEntities(EntityStore store, HashSet<int> idSet)
    {
        var index = (ComponentIndex<TValue>)store.extension.componentIndexes[StructInfo<TComponent>.Index];
        index.AddValueInRangeEntities(min, max, idSet);
    }
}
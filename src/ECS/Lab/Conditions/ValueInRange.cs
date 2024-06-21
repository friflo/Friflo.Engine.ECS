// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

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
        var index = (ComponentIndex<TValue>)StoreIndex.GetIndex(store, StructInfo<TComponent>.Index);
        index.AddValueInRangeEntities(min, max, idSet);
    }
}
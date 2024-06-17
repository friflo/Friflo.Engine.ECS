// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal abstract class ValueCondition
{
    internal abstract void AddMatchingEntities(EntityStore store, HashSet<int> set);
}

internal sealed class HasValueCondition<TComponent, TValue> : ValueCondition where TComponent : struct, IIndexedComponent<TValue>
{
    private readonly TValue value;
    
    internal HasValueCondition(TValue value) {
        this.value = value;
    }
    
    internal override void AddMatchingEntities(EntityStore store, HashSet<int> set)
    {
        var index = (ComponentIndex<TValue>)store.extension.componentIndexes[StructInfo<TComponent>.Index];
        index.AddMatchingEntities(value, set);
    }
}
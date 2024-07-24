// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class ValueStructIndex<TIndexedComponent,TValue>  : ComponentIndex<TValue>
    where TIndexedComponent : struct, IIndexedComponent<TValue>
    where TValue            : struct
{
    internal override   int                         Count       => entityMap.Count;
#region fields
    /// map:  indexed value  ->  entities (ids) containing a <see cref="IIndexedComponent{TValue}"/> with the indexed value as key.
    private  readonly   Dictionary<TValue, IdArray> entityMap   = new();
    #endregion
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        TValue value = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        DictionaryUtils.AddComponentValue    (id, value, entityMap, this);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        TValue oldValue = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        TValue value    = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        var localMap  = entityMap;
        DictionaryUtils.RemoveComponentValue (id, oldValue, localMap, this);
        DictionaryUtils.AddComponentValue    (id, value,    localMap, this);
    }

    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        TValue value = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        DictionaryUtils.RemoveComponentValue (id, value, entityMap, this);
    }
    
    internal override void RemoveEntityFromIndex(int id, Archetype archetype, int compIndex)
    {
        var map         = entityMap;
        var heap        = idHeap;
        var components  = ((StructHeap<TIndexedComponent>)archetype.heapMap[componentType.StructIndex]).components;
        TValue value    = components[compIndex].GetIndexedValue();
        map.TryGetValue(value, out var idArray);
        var idSpan  = idArray.GetSpan(heap, store);
        var index   = idSpan.IndexOf(id);
        idArray.RemoveAt(index, heap);
        if (idArray.Count == 0) {
            map.Remove(value);
        } else {
            map[value] = idArray;
        }
        store.nodes[id].isOwner &= ~indexBit;
    }
    #endregion
    
#region get matches
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => entityMap.Keys;
    
    internal override Entities GetHasValueEntities(TValue value)
    {
        entityMap.TryGetValue(value, out var ids);
        return idHeap.GetEntities(store, ids);
    }
    
    internal override void AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet) {
        SortUtils<TValue>.AddValueInRangeEntities(min, max, idSet, entityMap, this);
    }
    #endregion
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class ValueStructIndex<TIndexedComponent,TValue>  : ComponentIndex<TValue>
    where TIndexedComponent : struct, IIndexedComponent<TValue>
    where TValue : struct
{
    internal override   int                         Count       => map.Count;
    
    /// map: indexed value -> entity ids
    private  readonly   Dictionary<TValue, IdArray> map         = new();
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        DictionaryUtils.AddComponentValue    (id, value, map, this);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        var oldValue = IndexUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        var value    = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        var localMap  = map;
        DictionaryUtils.RemoveComponentValue (id, oldValue, localMap, this);
        DictionaryUtils.AddComponentValue    (id, value,    localMap, this);
    }

    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        DictionaryUtils.RemoveComponentValue (id, value, map, this);
    }
    
    internal override void RemoveEntityIndex(int id, Archetype archetype, int compIndex)
    {
        var localMap    = map;
        var components  = ((StructHeap<TIndexedComponent>)archetype.heapMap[componentType.StructIndex]).components;
        var value       = components[compIndex].GetIndexedValue();
        localMap.TryGetValue(value, out var idArray);
        var idSpan  = idArray.GetIdSpan(arrayHeap);
        var index   = idSpan.IndexOf(id);
        idArray.RemoveAt(index, arrayHeap);
        if (idArray.Count == 0) {
            localMap.Remove(value);
        } else {
            localMap[value] = idArray;
        }
        store.nodes[id].references &= ~indexBit;
    }
    #endregion
    
#region get matches
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => map.Keys;
    
    internal override Entities GetHasValueEntities(TValue value)
    {
        map.TryGetValue(value, out var ids);
        return arrayHeap.GetEntities(store, ids);
    }
    
    internal override void AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet) {
        SortUtils<TValue>.AddValueInRangeEntities(min, max, idSet, map, this);
    }
    #endregion
}
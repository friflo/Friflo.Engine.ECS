// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS.Collections;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

/// <summary>
/// A component index optimized to execute range queries in O(log N) at the cost of index updates in O(log N).<br/>
/// The default index executes in O(1) when adding, removing or updating indexed component values. 
/// </summary>
[ExcludeFromCodeCoverage] // not used - kept only for reference
public sealed class RangeIndex<TIndexedComponent,TValue> : ComponentIndex<TValue>
    where TIndexedComponent : struct, IIndexedComponent<TValue>
{
    internal override   int                         Count       => entityMap.Count;
    
#region fields
    /// map:  indexed value  ->  entities (ids) containing a <see cref="IIndexedComponent{TValue}"/> with the indexed value as key.
    private  readonly   SortedList<TValue, IdArray> entityMap   = new();
    
    private             ReadOnlyCollection<TValue>  keyCollection;
    #endregion
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        TValue value = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        SortedListUtils.AddComponentValue    (id, value, entityMap, this);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        TValue oldValue = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        TValue value    = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        var map = entityMap;
        SortedListUtils.RemoveComponentValue (id, oldValue, map, this);
        SortedListUtils.AddComponentValue    (id, value,    map, this);
    }

    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        TValue value = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        SortedListUtils.RemoveComponentValue (id, value, entityMap, this);
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
    internal override Entities GetHasValueEntities(TValue value)
    {
        entityMap.TryGetValue(value, out var ids);
        return idHeap.GetEntities(store, ids);
    }
    
    internal override void AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet)
    {
        var keys        = entityMap.Keys;
        var heap        = idHeap;
        var localStore  = store;
        int lowerIndex  = RangeUtils<TValue>.LowerBound(keys, min);
        int upperIndex  = RangeUtils<TValue>.UpperBound(keys, max);
        
        var values = entityMap.Values;
        for (int n = lowerIndex; n < upperIndex; n++) {
            var idArray = values[n];
            var entities = heap.GetEntities(localStore, idArray);
            foreach (var id in entities.Ids) {
                idSet.Add(id);
            }
        }
    }
    
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => keyCollection ??= new ReadOnlyCollection<TValue>(entityMap.Keys);
    #endregion
}

[ExcludeFromCodeCoverage] // not used - kept only for reference
internal static class RangeUtils<TValue>
{
    private static readonly Comparer<TValue> Comparer = Comparer<TValue>.Default; 
        
    // https://stackoverflow.com/questions/23806296/what-is-the-fastest-way-to-get-all-the-keys-between-2-keys-in-a-sortedlist
    internal static int LowerBound(IList<TValue> list, TValue value)
    {
        int lower = 0, upper = list.Count - 1;

        while (lower <= upper)
        {
            int middle = lower + (upper - lower) / 2;
            int comparisonResult = Comparer.Compare(value, list[middle]);

            // slightly adapted here
            if (comparisonResult <= 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }
        return lower;
    }
    
    internal static int UpperBound(IList<TValue> list, TValue value)
    {
        int lower = 0, upper = list.Count - 1;

        while (lower <= upper)
        {
            int middle = lower + (upper - lower) / 2;
            int comparisonResult = Comparer.Compare(value, list[middle]);

            // slightly adapted here
            if (comparisonResult < 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }
        return lower;
    }
}
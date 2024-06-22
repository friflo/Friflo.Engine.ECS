// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class RangeIndex<TValue>  : ComponentIndex<TValue>
{
    internal override   int                         Count       => map.Count;
    private  readonly   SortedList<TValue, IdArray> map         = new();
    private  readonly   IdArrayHeap                 arrayHeap   = new();
    private             ReadOnlyCollection<TValue>  keyCollection;
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        SortedListUtils.AddComponentValue    (id, value, map, arrayHeap);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap heap)
    {
        var oldValue = IndexUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        var value    = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        var localHeap = arrayHeap;
        var localMap  = map;
        SortedListUtils.RemoveComponentValue (id, oldValue, localMap, localHeap);
        SortedListUtils.AddComponentValue    (id, value,    localMap, localHeap);
    }

    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        SortedListUtils.RemoveComponentValue (id, value, map, arrayHeap);
    }
    #endregion
    
#region get matches
    internal override Entities GetHasValueEntities(TValue value)
    {
        map.TryGetValue(value, out var ids);
        return arrayHeap.GetEntities(store, ids);
    }
    
    internal override void AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet)
    {
        var keys        = map.Keys;
        int lowerIndex  = RangeUtils<TValue>.LowerBound(keys, min);
        int upperIndex  = RangeUtils<TValue>.UpperBound(keys, max);
        
        var values = map.Values;
        for (int n = lowerIndex; n < upperIndex; n++) {
            var idArray = values[n];
            var entities = arrayHeap.GetEntities(store, idArray);
            foreach (var id in entities.Ids) {
                idSet.Add(id);
            }
        }
    }
    
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => keyCollection ??= new ReadOnlyCollection<TValue>(map.Keys);
    #endregion
}

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
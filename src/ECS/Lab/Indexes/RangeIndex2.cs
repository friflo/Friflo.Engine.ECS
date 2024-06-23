// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class RangeIndex2<TValue>  : ComponentIndex<TValue>
{
    internal override   int                         Count       => map.Count;
    private  readonly   SortedValues<TValue>        map         = new();
    private  readonly   IdArrayHeap                 arrayHeap   = new();
    private             SortedValuesKeys<TValue>    keyCollection;
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        SortedValuesUtils.AddComponentValue    (id, value, map, arrayHeap);
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
        SortedValuesUtils.RemoveComponentValue (id, oldValue, localMap, localHeap);
        SortedValuesUtils.AddComponentValue    (id, value,    localMap, localHeap);
    }

    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        SortedValuesUtils.RemoveComponentValue (id, value, map, arrayHeap);
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
        int lowerIndex  = map.LowerBound(min);
        int upperIndex  = map.UpperBound(max);
        
        var values = map.ValueSpan;
        for (int n = lowerIndex; n < upperIndex; n++) {
            var idArray = values[n];
            var entities = arrayHeap.GetEntities(store, idArray);
            foreach (var id in entities.Ids) {
                idSet.Add(id);
            }
        }
    }
    
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => keyCollection ??= new SortedValuesKeys<TValue>(map);
    #endregion
}

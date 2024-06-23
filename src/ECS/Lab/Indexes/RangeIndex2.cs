// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class RangeIndex2<TValue>  : ComponentIndex<TValue>
{
    internal override   int                         Count       => map.Count;
    private  readonly   SortedSet<MapItem<TValue>>  map         = new(Comparer);
    private  readonly   IdArrayHeap                 arrayHeap   = new();
    private             SortedMapValues<TValue>     keyCollection;
    
    private static readonly MapItemComparer<TValue> Comparer    = new (); 
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        SortedMapUtils.AddComponentValue    (id, value, map, arrayHeap);
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
        SortedMapUtils.RemoveComponentValue (id, oldValue, localMap, localHeap);
        SortedMapUtils.AddComponentValue    (id, value,    localMap, localHeap);
    }

    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        SortedMapUtils.RemoveComponentValue (id, value, map, arrayHeap);
    }
    #endregion
    
#region get matches
    internal override Entities GetHasValueEntities(TValue value)
    {
        map.TryGetValue(new MapItem<TValue>(value), out var ids);
        return arrayHeap.GetEntities(store, ids.ids);
    }
    
    internal override void AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet)
    {
        var view = map.GetViewBetween(new MapItem<TValue>(min), new MapItem<TValue>(max));
        foreach (var item in view) {
            var entities = arrayHeap.GetEntities(store, item.ids);
            foreach (var id in entities.Ids) {
                idSet.Add(id);
            }
        }
    }
    
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => keyCollection ??= new SortedMapValues<TValue>(map);
    #endregion
}

// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class ValueClassIndex<TValue>  : ComponentIndex<TValue> where TValue : class
{
    internal override   int                         Count       => map.Count + (nullValue.count > 0 ? 1 : 0);
    private  readonly   Dictionary<TValue, IdArray> map         = new();
    private             IdArray                     nullValue;

    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        AddComponentValue    (id, value);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        var oldValue = IndexUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        var value    = IndexUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        RemoveComponentValue (id, oldValue);
        AddComponentValue    (id, value);
    }

    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        var value = IndexUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        RemoveComponentValue (id, value);
    }
    #endregion
    
#region null handling
    private void AddComponentValue(int id, in TValue value)
    {
        if (value != null) {
            DictionaryUtils.AddComponentValue (id, value, map, this);
            return;
        }
        var idSpan = nullValue.GetIdSpan(arrayHeap);
        if (idSpan.IndexOf(id) != -1) return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        nullValue.AddId(id, arrayHeap);
    }
    
    internal void RemoveComponentValue(int id, in TValue value)
    {
        if (value != null) {
            DictionaryUtils.RemoveComponentValue (id, value, map, this);
            return;
        }
        var idSpan  = nullValue.GetIdSpan(arrayHeap);
        var index   = idSpan.IndexOf(id);
        if (index == -1) return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        nullValue.RemoveAt(index, arrayHeap);
    }
    #endregion
    
#region get matches
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => map.Keys;
    
    internal override Entities GetHasValueEntities(TValue value)
    {
        if (value != null) {
            map.TryGetValue(value, out var ids);
            return arrayHeap.GetEntities(store, ids);
        }
        return arrayHeap.GetEntities(store, nullValue);
    }
    
    internal override void AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet) {
        SortUtils<TValue>.AddValueInRangeEntities(min, max, idSet, map, this);
    }
    #endregion
}
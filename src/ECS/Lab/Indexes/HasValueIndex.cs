// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class HasValueIndex<TValue>  : ComponentIndex<TValue>
{
    internal            int                         Count => map.Count;
    private readonly    Dictionary<TValue, IdArray> map         = new();
    private readonly    IdArrayHeap                 arrayHeap   = new();
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        DictionaryUtils.AddComponentValue    (id, value, map, arrayHeap);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap heap)
    {
        var oldValue = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        var value    = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        DictionaryUtils.RemoveComponentValue (id, oldValue, map, arrayHeap);
        DictionaryUtils.AddComponentValue    (id, value,    map, arrayHeap);
    }

    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        DictionaryUtils.RemoveComponentValue (id, value, map, arrayHeap);
    }
    #endregion
    
#region get matches
    internal override Entities GetMatchingEntities(TValue value)
    {
        map.TryGetValue(value, out var ids);
        return arrayHeap.GetEntities(store, ids);
    }
    #endregion
}
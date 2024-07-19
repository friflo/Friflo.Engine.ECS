// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class ValueClassIndex<TIndexedComponent,TValue> : ComponentIndex<TValue>
    where TIndexedComponent : struct, IIndexedComponent<TValue>
    where TValue            : class
{
    internal override   int                         Count       => entityMap.Count + (nullValue.count > 0 ? 1 : 0);
    
#region fields
    /// map:  indexed value  ->  entities (ids) containing a <see cref="IIndexedComponent{TValue}"/> with the indexed value as key.
    private  readonly   Dictionary<TValue, IdArray> entityMap   = new();
    
    /// store entity ids for indexed value == null
    private             IdArray                     nullValue;
    #endregion

    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        TValue value = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        AddComponentValue    (id, value);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        TValue oldValue = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        TValue value    = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        RemoveComponentValue (id, oldValue);
        AddComponentValue    (id, value);
    }

    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        TValue value = IndexedValueUtils<TComponent,TValue>.GetIndexedValue(heap.componentStash);
        RemoveComponentValue (id, value);
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
    
#region null handling
    private void AddComponentValue(int id, in TValue value)
    {
        if (value != null) {
            DictionaryUtils.AddComponentValue (id, value, entityMap, this);
            return;
        }
        var heap    = idHeap;
        var idSpan  = nullValue.GetSpan(heap, store);
        if (idSpan.IndexOf(id) != -1) return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        nullValue.Add(id, heap);
    }
    
    internal void RemoveComponentValue(int id, in TValue value)
    {
        if (value != null) {
            DictionaryUtils.RemoveComponentValue (id, value, entityMap, this);
            return;
        }
        var heap    = idHeap;
        var idSpan  = nullValue.GetSpan(heap, store);
        var index   = idSpan.IndexOf(id);
        if (index == -1) return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        nullValue.RemoveAt(index, heap);
    }
    #endregion
    
#region get matches
    internal override IReadOnlyCollection<TValue> IndexedComponentValues => entityMap.Keys;
    
    internal override Entities GetHasValueEntities(TValue value)
    {
        var heap        = idHeap;
        var localStore  = store;
        if (value != null) {
            entityMap.TryGetValue(value, out var ids);
            return heap.GetEntities(localStore, ids);
        }
        return heap.GetEntities(localStore, nullValue);
    }
    
    internal override void AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet) {
        SortUtils<TValue>.AddValueInRangeEntities(min, max, idSet, entityMap, this);
    }
    #endregion
}
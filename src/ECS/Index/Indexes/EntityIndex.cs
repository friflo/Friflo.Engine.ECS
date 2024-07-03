// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal abstract class EntityIndex : ComponentIndex<Entity>
{
    internal override   int                         Count       => map.Count;
    
#region fields
    /// map: entity id -> entity ids
    internal readonly   Dictionary<int, IdArray>    map         = new();
    
    private             EntityIndexValues           keyCollection;
    #endregion
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<Entity>)component).GetIndexedValue();    // boxes component
        DictionaryUtils.AddComponentValue    (id, value.Id, map, this);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap)
    {
        var oldValue = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(heap.componentStash).Id;
        var value    = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(component).Id;
        if (oldValue == value) {
            return;
        }
        var localMap  = map;
        DictionaryUtils.RemoveComponentValue (id, oldValue, localMap, this);
        DictionaryUtils.AddComponentValue    (id, value,    localMap, this);
    }
    
    internal override void Remove<TComponent>(int id, StructHeap<TComponent> heap)
    {
        var value = IndexedValueUtils<TComponent,Entity>.GetIndexedValue(heap.componentStash);
        DictionaryUtils.RemoveComponentValue (id, value.Id, map, this);
    }
    #endregion
    
#region get matches
    internal override Entities GetHasValueEntities(Entity value)
    {
        map.TryGetValue(value.Id, out var ids);
        return idHeap.GetEntities(store, ids);
    }
    
    internal override IReadOnlyCollection<Entity> IndexedComponentValues => keyCollection ??= new EntityIndexValues(this);
    #endregion
}

internal sealed class EntityIndex<TIndexedComponent> : EntityIndex
    where TIndexedComponent : struct, IIndexedComponent<Entity>
{
    internal override void RemoveEntityIndex(int id, Archetype archetype, int compIndex)
    {
        var localMap    = map;
        var heap        = idHeap;
        var components  = ((StructHeap<TIndexedComponent>)archetype.heapMap[componentType.StructIndex]).components;
        var value       = components[compIndex].GetIndexedValue().Id;
        localMap.TryGetValue(value, out var idArray);
        var idSpan  = idArray.GetIdSpan(heap);
        var index   = idSpan.IndexOf(id);
        idArray.RemoveAt(index, heap);
        if (idArray.Count == 0) {
            localMap.Remove(value);
        } else {
            localMap[value] = idArray;
        }
        store.nodes[id].references &= ~indexBit;
    }
}
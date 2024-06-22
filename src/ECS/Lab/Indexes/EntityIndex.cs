// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class EntityIndex : ComponentIndex<Entity>
{
    internal override   int                         Count       => map.Count;
    private  readonly   Dictionary<int, IdArray>    map         = new();
    private  readonly   IdArrayHeap                 arrayHeap   = new();
    
#region indexing
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexUtils<TComponent,Entity>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<Entity>)component).GetIndexedValue();    // boxes component
        DictionaryUtils.AddComponentValue    (id, value.Id, map, arrayHeap);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap heap)
    {
        var oldValue = IndexUtils<TComponent,Entity>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash).Id;
        var value    = IndexUtils<TComponent,Entity>.GetIndexedValue(component).Id;
        if (oldValue == value) {
            return;
        }
        var localHeap = arrayHeap;
        var localMap  = map;
        DictionaryUtils.RemoveComponentValue (id, oldValue, localMap, localHeap);
        DictionaryUtils.AddComponentValue    (id, value,    localMap, localHeap);
    }
    
    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexUtils<TComponent,Entity>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        DictionaryUtils.RemoveComponentValue (id, value.Id, map, arrayHeap);
    }
    #endregion
    
#region get matches
    internal override Entities GetHasValueEntities(Entity value)
    {
        map.TryGetValue(value.Id, out var ids);
        return arrayHeap.GetEntities(store, ids);
    }
    
    internal override IReadOnlyCollection<Entity> IndexedComponentValues => throw new NotImplementedException();

    #endregion
}
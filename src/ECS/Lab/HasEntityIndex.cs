// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class HasEntityIndex : ComponentIndex<Entity>
{
    internal            int                         Count => map.Count;
    private readonly    Dictionary<int, IdArray>    map;
    private readonly    IdArrayHeap                 arrayHeap;
    
#region general
    internal HasEntityIndex() {
        map         = new Dictionary<int, IdArray>();
        arrayHeap   = new IdArrayHeap();
    }
    #endregion
    
    
#region add / update
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<Entity>)component).GetIndexedValue();    // boxes component
        AddComponentValue(id, value);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap heap)
    {
        var oldValue = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        var value    = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(component);
        if (oldValue.Id == value.Id) {
            return;
        }
        RemoveComponentValue(id, oldValue);
        AddComponentValue   (id, value);
    }
    
    private void AddComponentValue(int id, in Entity value)
    {
#if NET6_0_OR_GREATER
        ref var ids = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(map, value.Id, out _);
#else
        map.TryGetValue(value.Id, out var ids);
#endif
        var idSpan = ids.GetIdSpan(arrayHeap);
        if (idSpan.IndexOf(id) != -1) {
            return;
        }
        ids.AddId(id, arrayHeap);
        MapUtils.Set(map, value.Id, ids);
    }
    #endregion
    
#region remove
    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexedComponentUtils<TComponent,Entity>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        RemoveComponentValue(id, value);
    }
    
    private void RemoveComponentValue(int id, in Entity value)
    {
#if NET6_0_OR_GREATER
        ref var ids = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(map, value.Id, out _);
#else
        map.TryGetValue(value.Id, out var ids);
#endif
        var idSpan = ids.GetIdSpan(arrayHeap);
        var index = idSpan.IndexOf(id);
        if (index == -1) {
            return;
        }
        if (ids.Count == 1) {
            map.Remove(value.Id);
            return;
        }
        ids.RemoveAt(index, arrayHeap);
        MapUtils.Set(map, value.Id, ids);
    }
    #endregion
    
#region get matches
    internal override Entities GetMatchingEntities(Entity value)
    {
        map.TryGetValue(value.Id, out var ids);
        return arrayHeap.GetEntities(store, ids);
    }
    #endregion
}
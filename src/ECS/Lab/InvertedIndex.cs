// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class InvertedIndex<TValue>  : ComponentIndex<TValue>
{
    internal            int                         Count => map.Count;
    private readonly    Dictionary<TValue, IdArray> map;
    private readonly    IdArrayHeap                 arrayHeap;
    
#region general
    internal InvertedIndex() {
        map         = new Dictionary<TValue, IdArray>(GetEqualityComparer());
        arrayHeap   = new IdArrayHeap();
    }
    
    private static IEqualityComparer<TValue> GetEqualityComparer()
    {
        if (typeof(Entity) == typeof(TValue)) {
            return (IEqualityComparer<TValue>)(object)EntityUtils.EqualityComparer;
        }
        return null;
    }
    #endregion
    
    
#region add / update
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(component);
    //  var value = ((IIndexedComponent<TValue>)component).GetIndexedValue();    // boxes component
        AddComponentValue(id, value);
    }
    
    internal override void Update<TComponent>(int id, in TComponent component, StructHeap heap)
    {
        var oldValue = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        var value    = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(component);
        if (EqualityComparer<TValue>.Default.Equals(oldValue , value)) {
            return;
        }
        RemoveComponentValue(id, oldValue);
        AddComponentValue   (id, value);
    }
    
    private void AddComponentValue(int id, in TValue value)
    {
#if NET6_0_OR_GREATER
        ref var ids = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(map, value, out _);
#else
        map.TryGetValue(value, out var ids);
#endif
        var idSpan = ids.GetIdSpan(arrayHeap);
        if (idSpan.IndexOf(id) != -1) {
            return;
        }
        ids.AddId(id, arrayHeap);
        MapUtils.Set(map, value, ids);
    }
    #endregion
    
#region remove
    internal override void Remove<TComponent>(int id, StructHeap heap)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).componentStash);
        RemoveComponentValue(id, value);
    }
    
    internal void RemoveComponentValue(int id, in TValue value)
    {
#if NET6_0_OR_GREATER
        ref var ids = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(map, value, out _);
#else
        map.TryGetValue(value, out var ids);
#endif
        var idSpan = ids.GetIdSpan(arrayHeap);
        var index = idSpan.IndexOf(id);
        if (index == -1) {
            return;
        }
        if (ids.Count == 1) {
            map.Remove(value);
            return;
        }
        ids.RemoveAt(index, arrayHeap);
        MapUtils.Set(map, value, ids);
    }
    #endregion
    
#region get matches
    internal override void AddMatchingEntities(in TValue value, HashSet<int> set)
    {
        map.TryGetValue(value, out var ids);
        foreach (var id in ids.GetIdSpan(arrayHeap)) {
            set.Add(id);   
        }
    }
    #endregion
}
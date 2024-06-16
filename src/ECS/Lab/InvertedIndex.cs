// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class InvertedIndex<TValue>  : ComponentIndex<TValue>
{
    private readonly    Dictionary<TValue, IdArray> map     = new ();
    private readonly    IdArrayHeap                 heap    = new IdArrayHeap();
    
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(component);
        // var indexedComponent    = (IIndexedComponent<TValue>)component; // boxing implementation of IIndexedComponent<>.GetValue()
        // var value               = indexedComponent.GetValue();
#if NET6_0_OR_GREATER
        ref var ids = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(map, value, out _);
#else
        map.TryGetValue(value, out var ids);
#endif
        var idSpan = ids.GetIdSpan(heap);
        if (idSpan.IndexOf(id) != -1) {
            return;
        }
        ids.AddId(id, heap);
        MapUtils.Set(map, value, ids);
    }
    
    internal override void Remove<TComponent>(int id, StructHeap heap, int compIndex)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).components[compIndex]);
        bool exists;
#if NET6_0_OR_GREATER
        ref var ids = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(map, value, out exists);
#else
        exists      = map.TryGetValue(value, out var ids);
#endif
        if (!exists) {
            return;
        }
        var idSpan = ids.GetIdSpan(this.heap);
        var index = idSpan.IndexOf(id);
        if (index == -1) {
            return;
        }
        if (ids.Count == 1) {
            map.Remove(value);
            return;
        }
        ids.RemoveAt(index, this.heap);
        MapUtils.Set(map, value, ids);
    }
    
    internal override void AddMatchingEntities(in TValue value, HashSet<int> set)
    {
        map.TryGetValue(value, out var ids);
        foreach (var id in ids.GetIdSpan(heap)) {
            set.Add(id);   
        }
    }
}
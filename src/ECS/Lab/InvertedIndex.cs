// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class InvertedIndex<TValue>  : ComponentIndex
{
    internal readonly    Dictionary<TValue, int[]>   map = new ();
    
    internal override void Add<TComponent>(int id, in TComponent component)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(component);
        // var indexedComponent    = (IIndexedComponent<TValue>)component; // boxing implementation of IIndexedComponent<>.GetValue()
        // var value               = indexedComponent.GetValue();
        if (!map.TryGetValue(value, out var ids)) {
            map.Add(value, new int[] { id });                           // TODO avoid array creation
            return;
        }
        if (Array.IndexOf(ids, id) != -1) {
            return;
        }
        var newIds = new int[ids.Length + 1];                           // TODO avoid array creation
        ids.CopyTo(newIds, 0);
        newIds[ids.Length]  = id;
        map[value]          = newIds;
    }
    
    internal override void Remove<TComponent>(int id, StructHeap heap, int compIndex)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(((StructHeap<TComponent>)heap).components[compIndex]);
        if (!map.TryGetValue(value, out var ids)) {
            return;
        }
        var idIndex = Array.IndexOf(ids, id);
        if (idIndex == -1) {
            return;
        }
        var newLength = ids.Length - 1;
        if (newLength == 0) {
            map.Remove(value);
            return;
        }
        var newIds = new int[newLength];                           // TODO avoid array creation
        Array.Copy(ids, 0,           newIds, 0,       idIndex);
        Array.Copy(ids, idIndex + 1, newIds, idIndex, newLength - idIndex);
        map[value] = newIds;
    }
}
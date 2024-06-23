// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal static class SortedMapUtils
{
    internal static void RemoveComponentValue<TValue>(int id, in TValue value, SortedSet<MapItem<TValue>> map, IdArrayHeap arrayHeap)
    {
        var item = new MapItem<TValue>(value);
        map.TryGetValue(item, out var current);
        var idSpan  = current.ids.GetIdSpan(arrayHeap);
        var index   = idSpan.IndexOf(id);
        if (index == -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        map.Remove(item);
        if (idSpan.Length == 1) {
            return;
        }
        current.ids.RemoveAt(index, arrayHeap);
        map.Add(item);
    }
    
    internal static void AddComponentValue<TValue>(int id, in TValue value, SortedSet<MapItem<TValue>> map, IdArrayHeap arrayHeap)
    {
        var item = new MapItem<TValue>(value);
        map.TryGetValue(item, out var current);
        var idSpan = current.ids.GetIdSpan(arrayHeap);
        if (idSpan.IndexOf(id) != -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        current.ids.AddId(id, arrayHeap);
        map.Add(item);
    }
}
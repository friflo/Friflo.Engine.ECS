// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal static class SortedValuesUtils
{
    internal static void RemoveComponentValue<TValue>(int id, in TValue value, SortedValues<TValue> map, IdArrayHeap arrayHeap)
    {
        map.TryGetValue(value, out var ids);
        var idSpan  = ids.GetIdSpan(arrayHeap);
        var index   = idSpan.IndexOf(id);
        if (index == -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        if (ids.Count == 1) {
            map.Remove(value);
            return;
        }
        ids.RemoveAt(index, arrayHeap);
        map[value] = ids;
    }
    
    internal static void AddComponentValue<TValue>(int id, in TValue value, SortedValues<TValue> map, IdArrayHeap arrayHeap)
    {
        map.TryGetValue(value, out var ids);
        var idSpan = ids.GetIdSpan(arrayHeap);
        if (idSpan.IndexOf(id) != -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        ids.AddId(id, arrayHeap);
        map[value] = ids;
    }
}
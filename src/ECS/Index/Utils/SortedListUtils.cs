// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

[ExcludeFromCodeCoverage] // not used - kept only for reference
internal static class SortedListUtils
{
    internal static void RemoveComponentValue<TValue>(int id, in TValue value, SortedList<TValue, IdArray> map, ComponentIndex componentIndex)
    {
        map.TryGetValue(value, out var ids);
        var idSpan  = ids.GetIdSpan(componentIndex.arrayHeap);
        var index   = idSpan.IndexOf(id);
        if (index == -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        componentIndex.store.nodes[id].references &= ~componentIndex.indexBit;
        if (ids.Count == 1) {
            map.Remove(value);
            return;
        }
        ids.RemoveAt(index, componentIndex.arrayHeap);
        map[value] = ids;
    }
    
    internal static void AddComponentValue<TValue>(int id, in TValue value, SortedList<TValue, IdArray> map, ComponentIndex componentIndex)
    {
        map.TryGetValue(value, out var ids);
        var idSpan = ids.GetIdSpan(componentIndex.arrayHeap);
        if (idSpan.IndexOf(id) != -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        componentIndex.store.nodes[id].references |= componentIndex.indexBit;
        ids.AddId(id, componentIndex.arrayHeap);
        map[value] = ids;
    }
}
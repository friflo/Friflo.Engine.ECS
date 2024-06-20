// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal static class DictionaryUtils
{
    internal static void RemoveComponentValue<TValue>(int id, in TValue value, Dictionary<TValue, IdArray> map, IdArrayHeap arrayHeap)
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
    
    internal static void AddComponentValue<TValue>(int id, in TValue value, Dictionary<TValue, IdArray> map, IdArrayHeap arrayHeap)
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
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

/// <summary>
/// Similar to <see cref="DictionaryUtils"/> and additionally updates <see cref="EntityNode.isLinked"/> state.
/// </summary>
internal static class EntityIndexUtils
{
    internal static void RemoveComponentValue(int id, int link, EntityIndex componentIndex)
    {
        var map     = componentIndex.entityMap;
        var idHeap  = componentIndex.idHeap;
        map.TryGetValue(link, out var ids);
        var idSpan  = ids.GetIdSpan(idHeap);
        var index   = idSpan.IndexOf(id);
        if (index == -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        var nodes = componentIndex.store.nodes;
       nodes[id].references &= ~componentIndex.indexBit;
        if (ids.Count == 1) {
            nodes[link].isLinked  &= ~componentIndex.indexBit;
            componentIndex.modified =  true;
            map.Remove(link);
            return;
        }
        ids.RemoveAt(index, idHeap);
        map[link] = ids;
    }
    
    internal static void AddComponentValue(int id, int link, EntityIndex componentIndex)
    {
        var map     = componentIndex.entityMap;
        var idHeap  = componentIndex.idHeap;
        map.TryGetValue(link, out var ids);
        var idSpan = ids.GetIdSpan(idHeap);
        if (idSpan.IndexOf(id) != -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        var nodes = componentIndex.store.nodes;
        nodes[id].references |= componentIndex.indexBit;
        if (ids.Count == 0) {
            nodes[link].isLinked  |= componentIndex.indexBit;
            componentIndex.modified = true;
        }
        ids.AddId(id, idHeap);
        map[link] = ids;
    }
}
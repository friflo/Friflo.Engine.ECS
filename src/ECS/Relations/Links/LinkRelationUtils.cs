// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;

internal static class LinkRelationUtils
{
    internal static void RemoveComponentValue(int id, int target, EntityRelations componentIndex)
    {
        var map         = componentIndex.linkEntityMap;
        var linkHeap    = componentIndex.linkIdsHeap;
        map.TryGetValue(target, out var ids);
        var idSpan  = ids.GetSpan(linkHeap, componentIndex.store);
        int index   = idSpan.IndexOf(id);
        if (index == -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        var nodes =  componentIndex.store.nodes;
        if (ids.Count == 1) {
            nodes[target].isLinked   &= ~componentIndex.relationBit;
            map.Remove(target);
            return;
        }
        ids.RemoveAt(index, linkHeap);
        map[target] = ids;
    }
    
    internal static void AddComponentValue(int id, int target, EntityRelations componentIndex)
    {
        var map         = componentIndex.linkEntityMap;
        var linkHeap    = componentIndex.linkIdsHeap;
        map.TryGetValue(target, out var ids);
        var idSpan = ids.GetSpan(linkHeap, componentIndex.store);
        if (idSpan.IndexOf(id) != -1) {
            return; // unexpected. Better safe than sorry. Used belts with suspenders :)
        }
        var nodes = componentIndex.store.nodes;
        if (ids.Count == 0) {
            nodes[target].isLinked   |= componentIndex.relationBit;
        }
        ids.Add(id, linkHeap);
        map[target] = ids;
    }
}
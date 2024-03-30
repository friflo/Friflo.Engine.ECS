// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Utils;


// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial struct  Entity
{
    public void Remove<T1>(
        in Tags tags = default)
            where T1 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = new BitSet();
        removeTypes.ClearBit(StructHeap<T1>.StructIndex);
        var newType         = store.GetArchetypeRemove(removeTypes, oldType, tags);
        StashRemoveComponents(store, newType, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(store, Id, newType, oldType);
    }
    
    
    // ------------------------------------------------- utils -------------------------------------------------
    private static void StashRemoveComponents(EntityStoreBase store, Archetype newType, Archetype oldType, int oldCompIndex)
    {
        if (store.ComponentRemoved == null) {
            return;
        }
        var newHeapMap = newType.heapMap;
        foreach (var oldHeap in oldType.structHeaps) {
            var newHeap = newHeapMap[oldHeap.structIndex];
            if (newHeap == null) {
                continue;
            }
            oldHeap.StashComponent(oldCompIndex);
        }
    }
    
    private static void SendRemoveEvents(EntityStoreBase store, int id, Archetype newType, Archetype oldType)
    {
        // --- tag event
        var tagsChanged = store.TagsChanged;
        if (tagsChanged != null && !newType.tags.bitSet.Equals(oldType.Tags.bitSet)) {
            tagsChanged(new TagsChanged(store, id, newType.tags, oldType.Tags));
        }
        // --- component events 
        var componentRemoved = store.ComponentRemoved;
        if (componentRemoved == null) {
            return;
        }
        var newHeaps    = newType.structHeaps;
        var oldHeapMap  = oldType.heapMap;
        for (int n = 0; n < newHeaps.Length; n++)
        {
            var structIndex = newHeaps[n].structIndex;
            var oldHeap     = oldHeapMap[structIndex];
            var action      = oldHeap == null ? ComponentChangedAction.Add : ComponentChangedAction.Update;
            componentRemoved(new ComponentChanged (store, id, action, structIndex, oldHeap));
        }
    }
} 
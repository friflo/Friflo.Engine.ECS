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
        removeTypes.SetBit(StructHeap<T1>.StructIndex);
        var newType         = store.GetArchetypeRemove(removeTypes, oldType, tags);
        StashRemoveComponents(store, removeTypes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(store, Id, removeTypes, newType, oldType);
    }
    
    public void Remove<T1, T2>(
        in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = new BitSet();
        removeTypes.SetBit(StructHeap<T1>.StructIndex);
        removeTypes.SetBit(StructHeap<T2>.StructIndex);
        var newType         = store.GetArchetypeRemove(removeTypes, oldType, tags);
        StashRemoveComponents(store, removeTypes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(store, Id, removeTypes, newType, oldType);
    }
    
    public void Remove<T1, T2, T3>(
        in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = new BitSet();
        removeTypes.SetBit(StructHeap<T1>.StructIndex);
        removeTypes.SetBit(StructHeap<T2>.StructIndex);
        removeTypes.SetBit(StructHeap<T3>.StructIndex);
        var newType         = store.GetArchetypeRemove(removeTypes, oldType, tags);
        StashRemoveComponents(store, removeTypes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(store, Id, removeTypes, newType, oldType);
    }
    
    public void Remove<T1, T2, T3, T4>(
        in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = new BitSet();
        removeTypes.SetBit(StructHeap<T1>.StructIndex);
        removeTypes.SetBit(StructHeap<T2>.StructIndex);
        removeTypes.SetBit(StructHeap<T3>.StructIndex);
        removeTypes.SetBit(StructHeap<T4>.StructIndex);
        var newType         = store.GetArchetypeRemove(removeTypes, oldType, tags);
        StashRemoveComponents(store, removeTypes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(store, Id, removeTypes, newType, oldType);
    }
    
    public void Remove<T1, T2, T3, T4, T5>(
        in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = new BitSet();
        removeTypes.SetBit(StructHeap<T1>.StructIndex);
        removeTypes.SetBit(StructHeap<T2>.StructIndex);
        removeTypes.SetBit(StructHeap<T3>.StructIndex);
        removeTypes.SetBit(StructHeap<T4>.StructIndex);
        removeTypes.SetBit(StructHeap<T5>.StructIndex);
        var newType         = store.GetArchetypeRemove(removeTypes, oldType, tags);
        StashRemoveComponents(store, removeTypes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(store, Id, removeTypes, newType, oldType);
    }
    
    
    // ------------------------------------------------- utils -------------------------------------------------
    private static void StashRemoveComponents(EntityStoreBase store, in BitSet removeTypes, Archetype oldType, int oldCompIndex)
    {
        if (store.ComponentRemoved == null) {
            return;
        }
        var oldHeapMap = oldType.heapMap;
        foreach (var removeTypeIndex in removeTypes) {
            var oldHeap = oldHeapMap[removeTypeIndex];
            if (oldHeap == null) {
                continue;
            }
            oldHeap.StashComponent(oldCompIndex);
        }
    }
    
    private static void SendRemoveEvents(EntityStoreBase store, int id, in BitSet removeTypes, Archetype newType, Archetype oldType)
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
        var oldHeapMap = oldType.heapMap;
        foreach (var removeTypeIndex in removeTypes) {
            var oldHeap = oldHeapMap[removeTypeIndex];
            if (oldHeap == null) {
                continue;
            }
            componentRemoved(new ComponentChanged (store, id, ComponentChangedAction.Remove, removeTypeIndex, oldHeap));
        }
    }
} 
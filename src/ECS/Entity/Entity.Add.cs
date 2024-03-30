// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Utils;

// ReSharper disable UseNullPropagation
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial struct  Entity
{
    public void Add<T1>(
        in T1   component1,
        in Tags tags = default)
            where T1 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addTypes        = new BitSet();
        addTypes.SetBit(StructHeap<T1>.StructIndex);
        var newType         = store.GetArchetypeAdd(addTypes, oldType, tags);
        StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(store, Id, addTypes, newType, oldType);
    }
    
    public void Add<T1, T2>(
        in T1   component1,
        in T2   component2,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2>(ref addTypes);
        var newType         = store.GetArchetypeAdd(addTypes, oldType, tags);
        StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(store, Id, addTypes, newType, oldType);
    }
    
    public void Add<T1, T2, T3>(
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2,T3>(ref addTypes);
        var newType         = store.GetArchetypeAdd(addTypes, oldType, tags);
        StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(store, Id, addTypes, newType, oldType);
    }
    
    public void Add<T1, T2, T3, T4>(
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2,T3,T4>(ref addTypes);
        var newType         = store.GetArchetypeAdd(addTypes, oldType, tags);
        StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(store, Id, addTypes, newType, oldType);
    }
    
    public void Add<T1, T2, T3, T4, T5>(
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
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
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2,T3,T4,T5>(ref addTypes);
        var newType         = store.GetArchetypeAdd(addTypes, oldType, tags);
        StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(store, Id, addTypes, newType, oldType);
    }
    
    // ------------------------------------------------- utils -------------------------------------------------
    private static void StashAddComponents(EntityStoreBase store, in BitSet addTypes, Archetype oldType, int oldCompIndex)
    {
        if (store.ComponentAdded == null) {
            return;
        }
        var oldHeapMap  = oldType.heapMap;
        foreach (var addTypeIndex in addTypes) {
            var oldHeap = oldHeapMap[addTypeIndex];
            if (oldHeap == null) {
                continue;
            }
            oldHeap.StashComponent(oldCompIndex);
        }
    }
    
    private static void SendAddEvents(EntityStoreBase store, int id, in BitSet addTypes, Archetype newType, Archetype oldType)
    {
        // --- tag event
        var tagsChanged = store.TagsChanged;
        if (tagsChanged != null && !newType.tags.bitSet.Equals(oldType.Tags.bitSet)) {
            tagsChanged(new TagsChanged(store, id, newType.tags, oldType.Tags));
        }
        // --- component events 
        var componentAdded = store.ComponentAdded;
        if (componentAdded == null) {
            return;
        }
        var oldHeapMap  = oldType.heapMap;
        foreach (var addTypeIndex in addTypes) {
            var oldHeap     = oldHeapMap[addTypeIndex];
            var action      = oldHeap == null ? ComponentChangedAction.Add : ComponentChangedAction.Update;
            componentAdded(new ComponentChanged (store, id, action, addTypeIndex, oldHeap));
        }
    }
} 
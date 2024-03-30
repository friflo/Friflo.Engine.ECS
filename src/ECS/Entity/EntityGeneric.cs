// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS.Utils;

// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal static class EntityGeneric
{
#region set bits
    internal static void SetBits<T1>(ref BitSet bitSet)
        where T1 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
    }

    internal static void SetBits<T1, T2>(ref BitSet bitSet)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
    }
    
    internal static void SetBits<T1, T2, T3>(ref BitSet bitSet)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
    }
    
    internal static void SetBits<T1, T2, T3, T4>(ref BitSet bitSet)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
    }
    
    internal static void SetBits<T1, T2, T3, T4, T5>(ref BitSet bitSet)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
    {
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
    }
    #endregion
    
#region set components

    internal static void SetComponents<T1>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1)
        where T1 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
    }

    internal static void SetComponents<T1, T2>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
    }

    internal static void SetComponents<T1, T2, T3>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2,
        in T3   component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructHeap<T3>.StructIndex]).components[compIndex] = component3;
    }

    internal static void SetComponents<T1, T2, T3, T4>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructHeap<T3>.StructIndex]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructHeap<T4>.StructIndex]).components[compIndex] = component4;
    }

    internal static void SetComponents<T1, T2, T3, T4, T5>(
        Archetype   archetype,
        int     compIndex,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
    {
        var heapMap = archetype.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[compIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[compIndex] = component2;
        ((StructHeap<T3>)heapMap[StructHeap<T3>.StructIndex]).components[compIndex] = component3;
        ((StructHeap<T4>)heapMap[StructHeap<T4>.StructIndex]).components[compIndex] = component4;
        ((StructHeap<T5>)heapMap[StructHeap<T5>.StructIndex]).components[compIndex] = component5;
    }
    #endregion
    
#region add components
    internal static void StashAddComponents(EntityStoreBase store, in BitSet addTypes, Archetype oldType, int oldCompIndex)
    {
        if (store.ComponentAdded == null) {
            return;
        }
        var oldHeapMap  = oldType.heapMap;
        foreach (var addTypeIndex in addTypes)
        {
            var oldHeap = oldHeapMap[addTypeIndex];
            if (oldHeap == null) {
                continue;
            }
            oldHeap.StashComponent(oldCompIndex);
        }
    }
    
    internal static void SendAddEvents(EntityStoreBase store, int id, in BitSet addTypes, Archetype newType, Archetype oldType)
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
        foreach (var addTypeIndex in addTypes)
        {
            var oldHeap     = oldHeapMap[addTypeIndex];
            var action      = oldHeap == null ? ComponentChangedAction.Add : ComponentChangedAction.Update;
            componentAdded(new ComponentChanged (store, id, action, addTypeIndex, oldHeap));
        }
    }
    #endregion
    
#region remove components
    internal static void StashRemoveComponents(EntityStoreBase store, in BitSet removeTypes, Archetype oldType, int oldCompIndex)
    {
        if (store.ComponentRemoved == null) {
            return;
        }
        var oldHeapMap = oldType.heapMap;
        foreach (var removeTypeIndex in removeTypes)
        {
            var oldHeap = oldHeapMap[removeTypeIndex];
            if (oldHeap == null) {
                continue;
            }
            oldHeap.StashComponent(oldCompIndex);
        }
    }
    
    internal static void SendRemoveEvents(EntityStoreBase store, int id, in BitSet removeTypes, Archetype newType, Archetype oldType)
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
        foreach (var removeTypeIndex in removeTypes)
        {
            var oldHeap = oldHeapMap[removeTypeIndex];
            if (oldHeap == null) {
                continue;
            }
            componentRemoved(new ComponentChanged (store, id, ComponentChangedAction.Remove, removeTypeIndex, oldHeap));
        }
    }
    #endregion
}
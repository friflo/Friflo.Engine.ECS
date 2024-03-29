// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial struct  Entity
{
    public void Add<T1, T2>(in T1 component1, T2 component2, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var oldType         = archetype;
        var oldCompIndex    = compIndex;
        var bitSet          = oldType.componentTypes.bitSet;
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        var newComponentTypes = new ComponentTypes { bitSet = bitSet };
        var newTags = new Tags();
        newTags.Add(tags);
        
        StashComponents(store, newComponentTypes, oldType, oldCompIndex);

        var newType         = store.GetArchetype(newComponentTypes, newTags);
        var newCompIndex    = refCompIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        refArchetype        = newType;
        var heapMap         = newType.heapMap;
        ((StructHeap<T1>)heapMap[StructHeap<T1>.StructIndex]).components[newCompIndex] = component1;
        ((StructHeap<T2>)heapMap[StructHeap<T2>.StructIndex]).components[newCompIndex] = component2;
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(store, Id, newType, oldType);
    }
    
    private static void StashComponents(EntityStoreBase store, in ComponentTypes newComponentTypes, Archetype oldType, int oldCompIndex)
    {
        var componentAdded = store.ComponentAdded;
        if (componentAdded == null) {
            return;
        }
        var oldHeapMap = oldType.heapMap;
        foreach (var newComponentType in newComponentTypes) {
            var oldHeap = oldHeapMap[newComponentType.StructIndex];
            if (oldHeap == null) {
                continue;
            }
            oldHeap.StashComponent(oldCompIndex);
        }
    }
    
    private static void SendAddEvents(EntityStoreBase store, int id, Archetype type, Archetype oldType)
    {
        // --- tag event
        var tagsChanged = store.TagsChanged;
        if (tagsChanged != null && !type.tags.bitSet.Equals(oldType.Tags.bitSet)) {
            tagsChanged(new TagsChanged(store, id, type.tags, oldType.Tags));
        }
        // --- component events 
        var componentAdded = store.ComponentAdded;
        if (componentAdded == null) {
            return;
        }
        var heaps = type.structHeaps;
        for (int n = 0; n < heaps.Length; n++)
        {
            var structIndex = heaps[n].structIndex;
            ComponentChangedAction action;
            StructHeap oldHeap;
            if (oldType.componentTypes.bitSet.Has(structIndex)) {
                action = ComponentChangedAction.Update;
                oldHeap = oldType.heapMap[structIndex];
            } else {
                action  = ComponentChangedAction.Add;
                oldHeap = null;
            }
            componentAdded(new ComponentChanged (store, id, action, structIndex, oldHeap));    
        }
    }

} 
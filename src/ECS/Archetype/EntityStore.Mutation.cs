﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// Hard rule: this file MUST NOT use type: Entity

using Friflo.Engine.ECS.Utils;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    // ------------------------------------ add / remove component ------------------------------------
#region add / remove component
    internal static bool AddComponent<T>(
            int         id,
            int         structIndex,
        ref Archetype   archetype,  // possible mutation is not null
        ref int         compIndex,
        // ReSharper disable once RedundantAssignment - archIndex must be changed before send event
        ref int         archIndex,
        in  T           component)      where T : struct, IComponent
    {
        var                     arch    = archetype;
        var                     store   = arch.store;
        ComponentChangedAction  action;
        bool                    added;

        var structHeap  = arch.heapMap[structIndex];
        var oldHeap     = structHeap;
        if (structHeap != null) {
            // --- case: archetype contains the component type  => archetype remains unchanged
            ((StructHeap<T>)oldHeap).StashComponent(compIndex);
            added   = false;
            action  = ComponentChangedAction.Update;
            goto AssignComponent;
        }
        // --- case: archetype doesn't contain component type   => change entity archetype
        var newArchetype    = GetArchetypeWith(store, arch, structIndex);
        compIndex           = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
        archetype           = arch = newArchetype;
        added               = true;
        action              = ComponentChangedAction.Add;
        archIndex           = arch.archIndex;
        structHeap          = arch.heapMap[structIndex];
        
    AssignComponent:  // --- assign passed component value
        var heap                    = (StructHeap<T>)structHeap;
        heap.components[compIndex]  = component;
        // Send event. See: SEND_EVENT notes
        var componentAdded = store.internBase.componentAdded;
        if (componentAdded == null) {
            return added;
        }
        componentAdded.Invoke(new ComponentChanged (store, id, action, structIndex, oldHeap));
        return added;
    }
    
    internal static bool RemoveComponent<T>(
            int         id,
        ref Archetype   archetype,    // possible mutation is not null
        ref int         compIndex,
        // ReSharper disable once RedundantAssignment - archIndex must be changed before send event
        ref int         archIndex,
            int         structIndex) where T : struct, IComponent
    {
        var arch    = archetype;
        var store   = arch.store;
        var heap    = arch.heapMap[structIndex];
        if (heap == null) {
            return false;
        }
        ((StructHeap<T>)heap).StashComponent(compIndex);
        var newArchetype = GetArchetypeWithout(store, arch, structIndex);

        // --- change entity archetype
        archetype   = newArchetype;
        archIndex   = newArchetype.archIndex;
        compIndex   = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
        // Send event. See: SEND_EVENT notes
        var componentRemoved = store.internBase.componentRemoved;
        if (componentRemoved == null) {
            return true;
        }
        componentRemoved.Invoke(new ComponentChanged (store, id, ComponentChangedAction.Remove, structIndex, heap));
        return true;
    }
    #endregion
    
    // ------------------------------------ add / remove entity Tag ------------------------------------
#region add / remove tags

    internal static bool AddTags(
        EntityStoreBase store,
        in Tags         tags,
        int             id,
        ref Archetype   archetype,      // possible mutation is not null
        ref int         compIndex,
        ref int         archIndex)
    {
        var arch        = archetype;
        var curTags     = arch.tags;
        var newTags     = new Tags (BitSet.Add(curTags.bitSet, tags.bitSet));
        if (newTags.bitSet.Equals(curTags.bitSet)) {
            return false;
        }
        var newArchetype    = GetArchetypeWithTags(store, arch, newTags);
        archetype           = newArchetype;
        archIndex           = newArchetype.archIndex;
        compIndex           = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
        // Send event. See: SEND_EVENT notes
        var tagsChanged = store.internBase.tagsChanged;
        if (tagsChanged == null) {
            return true;
        }
        tagsChanged.Invoke(new TagsChanged(store, id, newTags, curTags));
        return true;
    }
    
    internal static bool RemoveTags(
        EntityStoreBase store,
        in Tags         tags,
        int             id,
        ref Archetype   archetype,      // possible mutation is not null
        ref int         compIndex,
        ref int         archIndex)
    {
        var arch        = archetype;
        var curTags     = arch.tags;
        var newTags     = new Tags (BitSet.Remove(curTags.bitSet, tags.bitSet));
        if (newTags.bitSet.Equals(curTags.bitSet)) {
            return false;
        }
        var newArchetype    = GetArchetypeWithTags(store, arch, newTags);
        archetype           = newArchetype;
        archIndex           = newArchetype.archIndex;
        compIndex           = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
        // Send event. See: SEND_EVENT notes
        var tagsChanged = store.internBase.tagsChanged;
        if (tagsChanged == null) {
            return true;
        }
        tagsChanged.Invoke(new TagsChanged(store, id, newTags, curTags));
        return true;
    }
    #endregion
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertConstructorToMemberInitializers
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
        StructHeap              structHeap;
        StructHeap              oldHeap;
        ComponentChangedAction  action;
        bool                    added;
        if (arch != store.defaultArchetype)
        {
            structHeap  = arch.heapMap[structIndex];
            oldHeap     = structHeap;
            if (structHeap != null) {
                // --- case: archetype contains the component type  => archetype remains unchanged
                ((StructHeap<T>)oldHeap).StashComponent(compIndex);
                added   = false;
                action  = ComponentChangedAction.Update;
                goto AssignComponent;
            }
            // --- case: archetype doesn't contain component type   => change entity archetype
            // removed passing typeof(T) in commit:
            //   Engine - extract EntityStoreBase.AddComponentInternal() to prepare sending events for EntityStoreBase.AddComponent<>()
            //   https://github.com/friflo/Friflo.Json.Fliox/commit/f1cf0db5a59a961fc39c30918157678d82d3573e
            var newArchetype    = GetArchetypeWith(store, arch, structIndex);
            compIndex           = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
            archetype           = arch = newArchetype;
            added               = true;
            action              = ComponentChangedAction.Add;
        } else {
            // --- case: entity is assigned to default archetype    => get archetype with component and add entity
            oldHeap             = null;
            arch                = GetArchetype(store, arch.tags, structIndex);
            compIndex           = Archetype.AddEntity(arch, id);
            archetype           = arch;
            added               = true;
            action              = ComponentChangedAction.Add;
        }
        archIndex   = arch.archIndex;
        structHeap  = arch.heapMap[structIndex];
        
    AssignComponent:  // --- assign passed component value
        var heap    = (StructHeap<T>)structHeap;
        heap.components[compIndex] = component;
        // Send event. See: SEND_EVENT notes
        store.internBase.componentAdded?.Invoke(new ComponentChanged (store, id, action, structIndex, oldHeap));
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
        if (newArchetype == store.defaultArchetype) {
            int removePos = compIndex; 
            // --- update entity
            archetype   = store.defaultArchetype;
            compIndex   = 0;
            Archetype.MoveLastComponentsTo(arch, removePos);
        } else {
            // --- change entity archetype
            archetype   = newArchetype;
            compIndex   = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
        }
        archIndex   = archetype.archIndex;
        // Send event. See: SEND_EVENT notes
        store.internBase.componentRemoved?.Invoke(new ComponentChanged (store, id, ComponentChangedAction.Remove, structIndex, heap));
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
        var newTags     = new Tags (curTags.bitSet.value | tags.bitSet.value);
        if (newTags.bitSet.value == curTags.bitSet.value) {
            return false;
        }
        var searchKey = store.searchKey;
        searchKey.componentTypes    = arch.componentTypes;
        searchKey.tags              = newTags;
        searchKey.CalculateHashCode();
        Archetype newArchetype;
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            newArchetype = archetypeKey.archetype;
        } else {
            newArchetype = GetArchetypeWithTags(store, arch, searchKey.tags);
        }
        if (arch != store.defaultArchetype) {
            archetype   = newArchetype;
            compIndex   = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
        } else {
            compIndex   = Archetype.AddEntity(newArchetype, id);
            archetype   = newArchetype;
        }
        archIndex = archetype.archIndex;
        // Send event. See: SEND_EVENT notes
        store.internBase.tagsChanged?.Invoke(new TagsChanged(store, id, newTags, curTags));
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
        var newTags     = new Tags (curTags.bitSet.value & ~tags.bitSet.value);
        if (newTags.bitSet.value == curTags.bitSet.value) {
            return false;
        }
        var searchKey = store.searchKey;
        searchKey.componentTypes    = arch.componentTypes;
        searchKey.tags              = newTags;
        searchKey.CalculateHashCode();
        Archetype newArchetype;
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            newArchetype = archetypeKey.archetype;
        } else {
            newArchetype = GetArchetypeWithTags(store, arch, searchKey.tags);
        }
        if (newArchetype == store.defaultArchetype) {
            int removePos = compIndex; 
            // --- update entity
            compIndex   = 0;
            archetype   = store.defaultArchetype;
            Archetype.MoveLastComponentsTo(arch, removePos);
        } else {
            compIndex   = Archetype.MoveEntityTo(arch, id, compIndex, newArchetype);
            archetype   = newArchetype;
        }
        archIndex = archetype.archIndex;
        // Send event. See: SEND_EVENT notes
        store.internBase.tagsChanged?.Invoke(new TagsChanged(store, id, newTags, curTags));
        return true;
    }
    #endregion
}

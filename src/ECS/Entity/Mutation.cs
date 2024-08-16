// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly partial struct  Entity
{ 
    /// <summary>
    /// Add the given <paramref name="component"/> to the entity.<br/>
    /// If the entity contains a component of the same type it is updated.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#component">Example.</a>
    /// </summary>
    /// <returns>true - component is newly added to the entity.<br/> false - component is updated.</returns>
    public bool AddComponent<T>(in T component)      where T : struct, IComponent
    {
        int id          = Id;
        var localStore  = store;
        ref var node    = ref localStore.nodes[id];
        var arch        = node.archetype;
        if (arch == null || Revision != node.revision) {
            throw EntityNullException();  
        }
        int structIndex = StructInfo<T>.Index;
        ComponentChangedAction  action;
        bool                    added;
        if (StructInfo<T>.IsRelation) {
            throw EntityStoreBase.AddRelationException(id, structIndex);
        }
        int localCompIndex  = node.compIndex;
        var oldHeap         = (StructHeap<T>)arch.heapMap[structIndex];
        StructHeap<T> newHeap;
        if (oldHeap != null) {
            // --- case: archetype contains the component type  => archetype remains unchanged
            oldHeap.componentStash = oldHeap.components[localCompIndex];
            added   = false;
            action  = ComponentChangedAction.Update;
            if (StructInfo<T>.HasIndex) StoreIndex.UpdateIndex(localStore, id, component, oldHeap);
            newHeap = oldHeap;
            goto AssignComponent;
        }
        // --- case: archetype doesn't contain component type   => change entity archetype
        var newArchetype    = EntityStoreBase.GetArchetypeWith(localStore, arch, structIndex);
        localCompIndex      = Archetype.MoveEntityTo(arch, id, localCompIndex, newArchetype);
        node.compIndex      = localCompIndex;
        node.archetype      = arch = newArchetype;
        added               = true;
        action              = ComponentChangedAction.Add;
        if (StructInfo<T>.HasIndex) StoreIndex.AddIndex(localStore, id, component);
        newHeap             = (StructHeap<T>)arch.heapMap[structIndex];
        
    AssignComponent:  // --- assign passed component value
        newHeap.components[localCompIndex]  = component;
        // Send event. See: SEND_EVENT notes
        var componentAdded = localStore.internBase.componentAdded;
        if (componentAdded == null) {
            return added;
        }
        componentAdded.Invoke(new ComponentChanged (localStore, id, action, structIndex, oldHeap));
        return added;
    }
    
    /// <summary>Remove the component of the given type from the entity.</summary>
    /// <returns>true if entity contained a component of the given type before</returns>
    /// <remarks>
    /// Executes in O(1)<br/>
    /// <remarks>Note: Use <see cref="EntityUtils.RemoveEntityComponent"/> as non generic alternative</remarks>
    /// </remarks>
    public bool RemoveComponent<T>()            where T : struct, IComponent
    {
        var id              = Id;
        int structIndex     = StructInfo<T>.Index;
        var localStore      = store;
        ref var node        = ref localStore.nodes[id];
        var arch            = node.archetype;
        if (arch == null || Revision != node.revision) {
            throw EntityNullException();
        }
        if (StructInfo<T>.IsRelation) {
            throw EntityStoreBase.RemoveRelationException(id, structIndex);
        }
        int localCompIndex  = node.compIndex;
        var heap            = (StructHeap<T>)arch.heapMap[structIndex];
        if (heap == null) {
            return false;
        }
        heap.componentStash = heap.components[localCompIndex];
        if (StructInfo<T>.HasIndex) StoreIndex.RemoveIndex(localStore, id, heap);
        var newArchetype = EntityStoreBase.GetArchetypeWithout(localStore, arch, structIndex);
        
        // --- change entity archetype
        node.archetype   = newArchetype;
        node.compIndex   = Archetype.MoveEntityTo(arch, id, localCompIndex, newArchetype);
        // Send event. See: SEND_EVENT notes
        var componentRemoved = localStore.internBase.componentRemoved;
        if (componentRemoved == null) {
            return true;
        }
        componentRemoved.Invoke(new ComponentChanged (localStore, id, ComponentChangedAction.Remove, structIndex, heap));
        return true;
    }
}
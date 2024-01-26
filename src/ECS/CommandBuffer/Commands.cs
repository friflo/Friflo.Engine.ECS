// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using static Friflo.Engine.ECS.ComponentChangedAction;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

#pragma warning disable CS0618 // Type or member is obsolete  TODO remove


internal readonly struct EntityChanges
{
    internal readonly   EntityStore                     store;
    internal readonly   Dictionary<int, ComponentTypes> entities;
    
    internal EntityChanges(EntityStore store) {
        this.store  = store;
        entities    = new Dictionary<int, ComponentTypes>();
    }
}

internal abstract class ComponentCommands
{
    internal            int             commandCount;       //  4
    internal readonly   int             structIndex;        //  4
    
    internal abstract void Playback(EntityChanges changes);
    
    internal ComponentCommands(int structIndex) {
        this.structIndex = structIndex;
        
    }
}

internal sealed class ComponentCommands<T> : ComponentCommands
    where T : struct, IComponent
{
    internal    ComponentCommand<T>[]   componentCommands;  //  8
    
    internal ComponentCommands(int structIndex) : base(structIndex) { }
    
    internal override void Playback(EntityChanges changes)
    {
        var index       = structIndex;
        var entities    = changes.entities;
        var commands    = componentCommands;
        var count       = commandCount;
        
        // --- get new component types for changed entities
        for (int n = 0; n < count; n++)
        {
            ref var command = ref commands[n];
            entities.TryGetValue(command.entityId, out var componentTypes);
            switch (command.change) {
                case Remove:    componentTypes.bitSet.ClearBit(index);  break;
                case Add:       componentTypes.bitSet.SetBit  (index);  break;
                case Update:    componentTypes.bitSet.SetBit  (index);  break;
            }
            entities[command.entityId] = componentTypes;
        }

        // --- move changed entities to new archetype
        var store               = changes.store;
        var nodes               = store.nodes;
        var defaultArchetype    = store.defaultArchetype;
        foreach (var (entityId, componentTypes) in changes.entities)
        {
            ref var node        = ref nodes[entityId];
            var curArchetype    = node.Archetype;
            if (curArchetype.componentTypes.bitSet.value == componentTypes.bitSet.value) {
                continue;
            }
            var newArchetype = store.GetArchetype(componentTypes);
            if (curArchetype == defaultArchetype) {
                node.compIndex  = Archetype.AddEntity(newArchetype, entityId);
            } else {
                node.compIndex  = Archetype.MoveEntityTo(curArchetype, entityId, node.compIndex, newArchetype);
            }
            node.archetype  = newArchetype;
        }
        
        // --- set new component values
        for (int n = 0; n < count; n++)
        {
            ref var command = ref commands[n];
            ref var node    = ref nodes[command.entityId];
            switch (command.change) {
                case Remove:
                    break;
                case Add:
                    ((StructHeap<T>)node.archetype.heapMap[index]).components[node.compIndex] = command.component;
                    break;
                case Update:
                    ((StructHeap<T>)node.archetype.heapMap[index]).components[node.compIndex] = command.component;
                    break;
            }
        }
    }
}


internal struct ComponentCommand<T>
    where T : struct, IComponent
{
    internal    ComponentChangedAction  change;     //  4
    internal    int                     entityId;   //  4
    internal    T                       component;  //  sizeof(T)

    public override string ToString() => $"entity: {entityId} - {change} {typeof(T).Name}";
}
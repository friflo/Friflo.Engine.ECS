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
    
    internal abstract void UpdateComponentTypes (EntityChanges changes);
    internal abstract void ExecuteCommands      (EntityChanges changes);
    
    internal ComponentCommands(int structIndex) {
        this.structIndex = structIndex;
    }
}

internal sealed class ComponentCommands<T> : ComponentCommands
    where T : struct, IComponent
{
    internal    ComponentCommand<T>[]   componentCommands;  //  8

    public override string              ToString() => $"[{typeof(T).Name}] commands - Count: {commandCount}";

    internal ComponentCommands(int structIndex) : base(structIndex) { }
    
    internal override void UpdateComponentTypes(EntityChanges changes)
    {
        var index       = structIndex;
        var entities    = changes.entities;
        var commands    = componentCommands;
        var count       = commandCount;
        var nodes       = changes.store.nodes;
        
        // --- set new entity component types for Add/Remove commands
        for (int n = 0; n < count; n++)
        {
            ref var command = ref commands[n];
            if (command.change == Update) {
                continue;
            }
            var entityId = command.entityId;
            if (!entities.TryGetValue(entityId, out var componentTypes)) {
                componentTypes = nodes[entityId].archetype.componentTypes;
            }
            if (command.change == Remove) {
                componentTypes.bitSet.ClearBit(index);
            } else {
                componentTypes.bitSet.SetBit  (index);
            }
            entities[entityId] = componentTypes;
        }
    }
        
    internal override void ExecuteCommands(EntityChanges changes)
    {
        var index       = structIndex;
        var commands    = componentCommands;
        var nodes       = changes.store.nodes;
        var count       = commandCount;
        
        for (int n = 0; n < count; n++)
        {
            ref var command = ref commands[n];
            if (command.change == Remove) {
                // skip Remove commands
                continue;
            }
            // set new component value for Add & Update commands
            ref var node    = ref nodes[command.entityId];
            var heap        = node.archetype.heapMap[index];
            if (heap == null) {
                // case: RemoveComponent<>() was called after AddComponent<>() or SetComponent<>() on same entity
                continue;
            }
            ((StructHeap<T>)heap).components[node.compIndex] = command.component;
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
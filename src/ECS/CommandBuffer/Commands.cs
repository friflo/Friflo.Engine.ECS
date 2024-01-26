// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using static Friflo.Engine.ECS.ComponentChangedAction;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

#pragma warning disable CS0618 // Type or member is obsolete  TODO remove

internal struct ComponentChange
{
    internal        ComponentTypes  componentTypes; // 32
    internal        int             lastCommand;    //  4

    public override string          ToString() => $"change {componentTypes}";
}

internal readonly struct EntityChanges
{
    internal readonly   EntityStore                         store;
    internal readonly   Dictionary<int, ComponentChange>    entities;
    
    internal EntityChanges(EntityStore store) {
        this.store  = store;
        entities    = new Dictionary<int, ComponentChange>();
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
    
    internal ComponentCommands(int structIndex) : base(structIndex) { }
    
    internal override void UpdateComponentTypes(EntityChanges changes)
    {
        var index       = structIndex;
        var entities    = changes.entities;
        var commands    = componentCommands;
        var count       = commandCount;
        
        // --- set new component types for changed entities
        //     store the last command for an entity to execute
        for (int n = 0; n < count; n++)
        {
            ref var command = ref commands[n];
            entities.TryGetValue(command.entityId, out var change);
            switch (command.change) {
                case Remove:    change.componentTypes.bitSet.ClearBit(index);   break;
                case Add:       change.componentTypes.bitSet.SetBit  (index);   break;
                case Update:                                                    break;
            }
            change.lastCommand          = n;
            entities[command.entityId]  = change;
        }
    }
        
    internal override void ExecuteCommands(EntityChanges changes)
    {
        var index       = structIndex;
        var entities    = changes.entities;
        var commands    = componentCommands;
        var nodes       = changes.store.nodes;
        // --- set new component values
        foreach (var (entityId, change) in entities)
        {
            ref var command = ref commands[change.lastCommand];
            ref var node    = ref nodes[entityId];
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
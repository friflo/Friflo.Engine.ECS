// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;
using static Friflo.Engine.ECS.ComponentChangedAction;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal struct EntityChange
{
    internal ComponentTypes componentTypes;
    internal Tags           tags;
}

internal readonly struct Playback
{
    internal readonly   EntityStore                     store;          //  8
    internal readonly   Dictionary<int, EntityChange>   entityChanges;  //  8
    
    internal Playback(EntityStore store) {
        this.store      = store;
        entityChanges   = new Dictionary<int, EntityChange>();
    }
}

internal abstract class ComponentCommands
{
    internal            int             commandCount;       //  4
    internal readonly   int             structIndex;        //  4
    
    internal abstract void UpdateComponentTypes (Playback playback);
    internal abstract void ExecuteCommands      (Playback playback);
    
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
    
    internal override void UpdateComponentTypes(Playback playback)
    {
        var index           = structIndex;
        var commands        = componentCommands;
        var count           = commandCount;
        var entityChanges   = playback.entityChanges;
        var nodes           = playback.store.nodes;
        
        // --- set new entity component types for Add/Remove commands
        for (int n = 0; n < count; n++)
        {
            ref var command = ref commands[n];
            if (command.change == Update) {
                continue;
            }
            var entityId = command.entityId;
            if (!entityChanges.TryGetValue(entityId, out var change)) {
                change.componentTypes = nodes[entityId].archetype.componentTypes;
            }
            if (command.change == Remove) {
                change.componentTypes.bitSet.ClearBit(index);
            } else {
                change.componentTypes.bitSet.SetBit  (index);
            }
            entityChanges[entityId] = change;
        }
    }
        
    internal override void ExecuteCommands(Playback playback)
    {
        var index       = structIndex;
        var commands    = componentCommands;
        var count       = commandCount;
        var nodes       = playback.store.nodes;
        
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

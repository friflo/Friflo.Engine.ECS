// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using static Friflo.Engine.ECS.ComponentChangedAction;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class ComponentCommands
{
    [Browse(Never)] internal            int             commandCount;       //  4
    [Browse(Never)] internal readonly   int             structIndex;        //  4
    
    internal abstract void UpdateComponentTypes (Playback playback);
    internal abstract void ExecuteCommands      (Playback playback);
    
    internal ComponentCommands(int structIndex) {
        this.structIndex = structIndex;
    }
}

internal sealed class ComponentCommands<T> : ComponentCommands
    where T : struct, IComponent
{
    internal       ReadOnlySpan<ComponentCommand<T>>    Commands    => new (componentCommands, 0, commandCount);
    public   override           string                  ToString()  => $"[{typeof(T).Name}] commands - Count: {commandCount}";
    
    [Browse(Never)] internal    ComponentCommand<T>[]   componentCommands;  //  8


    internal ComponentCommands(int structIndex) : base(structIndex) { }
    
    internal override void UpdateComponentTypes(Playback playback)
    {
        var index           = structIndex;
        var commands        = componentCommands.AsSpan(0, commandCount);
        var entityChanges   = playback.entityChanges;
        var nodes           = playback.store.nodes.AsSpan();
        
        // --- set new entity component types for Add/Remove commands
        foreach (ref var command in commands)
        {
            if (command.change == Update) {
                continue;
            }
            var entityId = command.entityId;
            ref var change = ref CollectionsMarshal.GetValueRefOrAddDefault(entityChanges, entityId, out bool exists);
            if (!exists) {
                var archetype           = nodes[entityId].archetype;
                if (archetype == null) {
                    throw EntityNotFound(command.ToString());
                }
                change.componentTypes   = archetype.componentTypes;
                change.tags             = archetype.tags;
            }
            if (command.change == Remove) {
                change.componentTypes.bitSet.ClearBit(index);
            } else {
                change.componentTypes.bitSet.SetBit  (index);
            }
        }
    }
    
    private static InvalidOperationException EntityNotFound(string command) {
        return new InvalidOperationException($"Playback - entity not found. command: {command}");
    }
        
    internal override void ExecuteCommands(Playback playback)
    {
        var index       = structIndex;
        var commands    = componentCommands.AsSpan(0, commandCount);
        var nodes       = playback.store.nodes.AsSpan();
        
        foreach (ref var command in commands)
        {
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

    public override string ToString() => $"entity: {entityId} - {change} [{typeof(T).Name}]";
}

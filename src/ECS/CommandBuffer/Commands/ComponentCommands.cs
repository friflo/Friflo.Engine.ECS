// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Engine.ECS.ComponentChangedAction;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// ReSharper disable InlineTemporaryVariable

// ReSharper disable TooWideLocalVariableScope
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class ComponentCommands : IComponentStash
{
    [Browse(Never)] internal            int             commandCount;       //  4
    [Browse(Never)] internal readonly   int             structIndex;        //  4
    
    
    public   abstract IComponent   GetStashDebug();
    
    internal abstract void UpdateComponentTypes (Playback playback, bool storeOldComponent);
    internal abstract void ExecuteCommands      (Playback playback);
    internal abstract void SendCommandEvents    (Playback playback);
    
    internal ComponentCommands(int structIndex) {
        this.structIndex = structIndex;
    }
}

internal sealed class ComponentCommands<T> : ComponentCommands, IComponentStash<T>
    where T : struct, IComponent
{
    internal       ReadOnlySpan<ComponentCommand<T>>    Commands    => new (componentCommands, 0, commandCount);
    public   override           string                  ToString()  => $"[{typeof(T).Name}] commands - Count: {commandCount}";
    
    [Browse(Never)] internal    ComponentCommand<T>[]   componentCommands;  //  8
    [Browse(Never)] private     T                       stashValue;


    internal ComponentCommands(int structIndex) : base(structIndex) { }
    
    public  override    IComponent  GetStashDebug() => stashValue;
    public              ref T       GetStashRef()   => ref stashValue;

    
    internal override void UpdateComponentTypes(Playback playback, bool storeOldComponent)
    {
        var index       = structIndex;
        var commands    = componentCommands.AsSpan(0, commandCount);
        var indexes     = playback.entityChangesIndexes;
        var changes     = playback.entityChanges;
        var nodes       = playback.store.nodes.AsSpan();
        
        bool exists;
        
        // --- set new entity component types for Add/Remove commands
        foreach (ref var command in commands)
        {
            if (command.change == Update) {
                continue;
            }
            var entityId = command.entityId;
#if NET6_0_OR_GREATER
            ref var changeIndex = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(indexes, entityId, out exists);
#else
            exists = indexes.TryGetValue(entityId, out var changeIndex);
#endif
            ref var node    = ref nodes[entityId];
            var archetype   = node.archetype;
            if (archetype == null) {
                throw EntityNotFound(command.ToString());
            }
            if (storeOldComponent) {
                var heap = archetype.heapMap[index];
                if (heap != null) {
                    command.oldComponent = ((StructHeap<T>)heap).components[node.compIndex];
                }
            }
            if (!exists) {
                changeIndex = playback.entityChangesCount++;
                if (changes.Length == changeIndex) {
                    changes = playback.ResizeChanges();
                }
                ref var newChange           = ref changes[changeIndex];
                newChange.componentTypes    = archetype.componentTypes;
                newChange.tags              = archetype.tags;
                newChange.oldArchetype      = archetype;
                newChange.entityId          = entityId;
            }
            ref var change = ref changes[changeIndex];
            if (command.change == Remove) {
                change.componentTypes.bitSet.ClearBit(index);
            } else {
                change.componentTypes.bitSet.SetBit  (index);
            }
            MapUtils.Set(indexes, entityId, changeIndex);
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
        
    internal override void SendCommandEvents(Playback playback)
    {
        var index       = structIndex;
        var commands    = componentCommands.AsSpan(0, commandCount);
        var store       = playback.store;
        var added       = store.internBase.componentAdded;
        var removed     = store.internBase.componentRemoved;
        var indexes     = playback.entityChangesIndexes;
        var changes     = playback.entityChanges;
        Action<ComponentChanged> changed;
        
        foreach (ref var command in commands)
        {
            var entityId    = command.entityId;
            ref var change  = ref changes[indexes[entityId]];
            var oldHeap     = change.oldArchetype.heapMap[index];
            ComponentChangedAction action; 
            if (command.change == Remove) {
                action = Remove;
                if (oldHeap == null) {
                    continue;
                }
                changed = removed;
            } else {
                action = oldHeap == null ? Add : Update;
                changed = added;
            }
            if (changed == null) {
                continue;
            }
            stashValue = command.oldComponent;
            changed.Invoke(new ComponentChanged(store, entityId, action, index, this));
        }
    }
}


internal struct ComponentCommand<T>
    where T : struct, IComponent
{
    internal    ComponentChangedAction  change;         //  4
    internal    int                     entityId;       //  4
    internal    T                       component;      //  sizeof(T)
    internal    T                       oldComponent;   //  sizeof(T)

    public override string ToString() => $"entity: {entityId} - {change} [{typeof(T).Name}]";
}

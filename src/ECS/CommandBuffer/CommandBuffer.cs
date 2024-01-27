// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

#pragma warning disable CS0618 // Type or member is obsolete


[Obsolete("Experimental")]
public struct CommandBuffer
{
    private readonly    ComponentCommands[] _componentCommands;
    private             ComponentTypes      _changedComponents;
    //
    private             ComponentTypes      _changedTags;
    private             TagCommand[]        _tagCommands;
    private             int                 _tagCommandsCount;
    //
    private readonly    EntityStore         store;
    
#region general methods
    public CommandBuffer(EntityStore store)
    {
        this.store          = store;
        var buffers         = store.GetCommandBuffers();
        _componentCommands  = buffers.componentCommands;
        _tagCommands        = buffers.tagCommands;
    }
    
    public void Playback()
    {
        // early out if there is nothing to change
        if (_changedComponents.Count == 0 && _changedTags.Count == 0) {
            store.ReturnCommandBuffers(_componentCommands, _tagCommands);
            return;
        }
        var playback            = store.GetPlayback();
        var componentCommands   = _componentCommands;
        var changedComponents   = _changedComponents;
        
        foreach (var componentType in changedComponents)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.UpdateComponentTypes(playback);
        }
        
        MoveEntitiesToNewArchetypes(playback);
        
        foreach (var componentType in changedComponents)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.ExecuteCommands(playback);
        }
        Reset();
        playback.entities.Clear();
        store.ReturnCommandBuffers(componentCommands, _tagCommands);
    }
    
    private static void MoveEntitiesToNewArchetypes(Playback playback)
    {
        var store               = playback.store;
        var nodes               = store.nodes;
        var defaultArchetype    = store.defaultArchetype;
        foreach (var (entityId, componentTypes) in playback.entities)
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
                if (newArchetype == defaultArchetype) {
                    Archetype.MoveLastComponentsTo(curArchetype, node.compIndex);
                    node.compIndex = 0;
                } else {
                    node.compIndex  = Archetype.MoveEntityTo(curArchetype, entityId, node.compIndex, newArchetype);
                }
            }
            node.archetype  = newArchetype;
        }
    }
    
    private void Reset()
    {
        var commands = _componentCommands;
        foreach (var componentType in _changedComponents)
        {
            commands[componentType.StructIndex].commandCount = 0;
        }
        _changedComponents  = default;
        _changedTags        = default;
        _tagCommandsCount   = 0;
    }
    #endregion
        
#region component
    public void AddComponent<T>(int entityId)
        where T : struct, IComponent
    {
        ChangeComponent<T>(default, entityId,ComponentChangedAction.Add);
    }
    
    public void AddComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        ChangeComponent(component,  entityId, ComponentChangedAction.Add);
    }
    
    public void SetComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        ChangeComponent(component,  entityId, ComponentChangedAction.Update);
    }
    
    public void RemoveComponent<T>(int entityId)
        where T : struct, IComponent
    {
        ChangeComponent<T>(default, entityId, ComponentChangedAction.Remove);
    }
    
    private void ChangeComponent<T>(in T component, int entityId, ComponentChangedAction change)
        where T : struct, IComponent
    {
        var structIndex = StructHeap<T>.StructIndex;
        _changedComponents.bitSet.SetBit(structIndex);
        var commands    = (ComponentCommands<T>)_componentCommands[structIndex];
        var count       = commands.commandCount; 
        if (count == commands.componentCommands.Length) {
            ArrayUtils.Resize(ref commands.componentCommands, 2 * count);
        }
        commands.commandCount   = count + 1;
        ref var command         = ref commands.componentCommands[count];
        command.change          = change;
        command.entityId        = entityId;
        command.component       = component;
    }
    #endregion
    
#region tag
    
    public void AddTag<T>(int entityId)
        where T : struct, ITag
    {
        ChangeTag(entityId, TagType<T>.TagIndex, TagChange.Add);
    }
    
    public void RemoveTag<T>(int entityId)
        where T : struct, ITag
    {
        ChangeTag(entityId, TagType<T>.TagIndex, TagChange.Remove);
    }
    
    private void ChangeTag(int entityId, int tagIndex, TagChange change)
    {
        _changedTags.bitSet.SetBit(tagIndex);
        
        var count = _tagCommandsCount; 
        if (count == _tagCommands.Length) {
            ArrayUtils.Resize(ref _tagCommands, 2 * count);
        }
        _tagCommandsCount   = count + 1;
        ref var tagCommand  = ref _tagCommands[count];
        tagCommand.tagIndex = (byte)tagIndex;
        tagCommand.entityId = entityId;
        tagCommand.change   = change;
    }
#endregion
}


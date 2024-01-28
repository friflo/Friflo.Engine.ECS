// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable UnusedMember.Global
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
#region public properties
    public              int                 ComponentCommandsCount  => GetComponentCommandsCount(_componentCommandTypes);
    public              int                 TagCommandsCount        => _tagCommandsCount;
    public              int                 EntityCommandsCount     => _entityCommandCount;
    public              bool                ReuseBuffer             { get => reuseBuffer; set => reuseBuffer = value; }
    
    public override     string              ToString() => $"component commands: {ComponentCommandsCount}  tag commands: {TagCommandsCount}"; 

    #endregion
    
#region private fields
    private             ComponentTypes      _changedComponentTypes;
    private             ComponentCommands[] _componentCommandTypes;
    //
    private             TagCommand[]        _tagCommands;
    private             int                 _tagCommandsCount;
    //
    private             EntityCommand[]     _entityCommands;
    private             int                 _entityCommandCount;
    //
    private readonly    EntityStore         store;
    private             bool                reuseBuffer;
    private             bool                returnedBuffers;
    #endregion
    
#region general methods
    public CommandBuffer(EntityStore store)
    {
        this.store              = store;
        var buffers             = store.GetCommandBuffers();
        _componentCommandTypes  = buffers.componentCommands;
        _tagCommands            = buffers.tagCommands;
        _entityCommands         = buffers.entityCommands;
    }
    
    public void Playback()
    {
        var tagCommands         = _tagCommands;
        var componentCommands   = _componentCommandTypes;
        var entityCommands      = _entityCommands;
        if (!reuseBuffer) {
            _tagCommands            = null;
            _componentCommandTypes  = null;
            _entityCommands         = null;
        }
        var playback = store.GetPlayback();
        try {
            bool hasComponentChanges = _changedComponentTypes.Count > 0;
            
            ExecuteEntityCommands(entityCommands);
            ExecuteTagCommands(playback, tagCommands);
            if (hasComponentChanges) {
                PrepareComponentCommands(playback, componentCommands);
            }
            UpdateEntityArchetypes  (playback);
            if (hasComponentChanges) {
                ExecuteComponentCommands(playback, componentCommands);
            }
        }
        finally {
            Reset(componentCommands);
            playback.entityChanges.Clear();
            if (!returnedBuffers && !reuseBuffer) {
                store.ReturnCommandBuffers(componentCommands, tagCommands, entityCommands);
                returnedBuffers = true;
            }
        }
    }
    
    public void ReturnBuffer()
    {
        if (!returnedBuffers) {
            store.ReturnCommandBuffers(_componentCommandTypes, _tagCommands, _entityCommands);
            returnedBuffers = true;
        }
        _tagCommands            = null;
        _componentCommandTypes  = null;
        _entityCommands         = null;
    }
    
    private void ExecuteEntityCommands(EntityCommand[] entityCommands)
    {
        int count = _entityCommandCount;
        if (count == 0) {
            return;
        }
        var commands        = entityCommands.AsSpan(0, count);
        foreach (var command in commands)
        {
            var entityId = command.entityId;
            if (command.action == EntityCommandAction.Create) {
                store.CreateEntity(entityId);
                continue;
            }
            var nodes = store.nodes;
            if (entityId < nodes.Length && nodes[entityId].Flags.HasFlag(NodeFlags.Created)) {
                var entity = store.GetEntityById(command.entityId);
                entity.DeleteEntity();
                continue;
            }
            throw new InvalidOperationException($"Playback - entity not found. Delete entity, entity: {entityId}");
        }
    }
    
    private void ExecuteTagCommands(Playback playback, TagCommand[] tagCommands)
    {
        var count = _tagCommandsCount;
        if (count == 0) {
            return;
        }
        var entityChanges   = playback.entityChanges;
        var nodes           = playback.store.nodes.AsSpan(); 
        var commands        = tagCommands.AsSpan(0, count);
        
        foreach (var tagCommand in commands)
        {
            var entityId = tagCommand.entityId;
            ref var change = ref CollectionsMarshal.GetValueRefOrAddDefault(entityChanges, entityId, out bool exists);
            if (!exists) {
                var archetype           = nodes[entityId].archetype;
                if (archetype == null) {
                    throw EntityNotFound(tagCommand);
                }
                change.componentTypes   = archetype.componentTypes;
                change.tags             = archetype.tags;
            }
            if (tagCommand.change == TagChange.Add) {
                change.tags.bitSet.SetBit(tagCommand.tagIndex);
            } else {
                change.tags.bitSet.ClearBit(tagCommand.tagIndex);
            }
        }
    }
    
    private void PrepareComponentCommands(Playback playback, ComponentCommands[] componentCommands)
    {
        foreach (var componentType in _changedComponentTypes)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.UpdateComponentTypes(playback);
        }
    }
    
    private void ExecuteComponentCommands(Playback playback, ComponentCommands[] componentCommands)
    {
        foreach (var componentType in _changedComponentTypes)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.ExecuteCommands(playback);
        }
    }
    
    private static InvalidOperationException EntityNotFound(TagCommand command) {
        return new InvalidOperationException($"Playback - entity not found. command: {command}");
    }
    
    private static void UpdateEntityArchetypes(Playback playback)
    {
        var store               = playback.store;
        var nodes               = store.nodes.AsSpan();
        var defaultArchetype    = store.defaultArchetype;
        var entityChanges       = playback.entityChanges;
        
        foreach (var entityId in entityChanges.Keys)
        {
            ref var change      = ref CollectionsMarshal.GetValueRefOrAddDefault(entityChanges, entityId, out bool _);
            ref var node        = ref nodes[entityId];
            var curArchetype    = node.Archetype;
            if (curArchetype.componentTypes.bitSet.value == change.componentTypes.bitSet.value &&
                curArchetype.tags.          bitSet.value == change.tags.          bitSet.value) {
                continue;
            }
            var newArchetype = store.GetArchetype(change.componentTypes, change.tags);
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
    
    private void Reset(ComponentCommands[] componentCommands)
    {
        foreach (var componentType in _changedComponentTypes)
        {
            componentCommands[componentType.StructIndex].commandCount = 0;
        }
        _changedComponentTypes  = default;
        _tagCommandsCount       = 0;
        _entityCommandCount     = 0;
    }
    
    private int GetComponentCommandsCount(ComponentCommands[] componentCommands) {
        int count = 0;
        foreach (var componentType in _changedComponentTypes) {
            count += componentCommands[componentType.StructIndex].commandCount;
        }
        return count;
    }
    
    private InvalidOperationException CannotReuseCommandBuffer() {
        if (returnedBuffers && reuseBuffer) {
            return new InvalidOperationException("CommandBuffer - buffers returned to store");    
        }
        return new InvalidOperationException($"Reused CommandBuffer after Playback(). ReuseBuffer: {reuseBuffer}");
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
        _changedComponentTypes.bitSet.SetBit(structIndex);
        if (returnedBuffers) {
            throw CannotReuseCommandBuffer();   
        }
        var commands    = (ComponentCommands<T>)_componentCommandTypes[structIndex];
        var count       = commands.commandCount; 
        if (count == commands.componentCommands.Length) {
            ArrayUtils.Resize(ref commands.componentCommands, Math.Max(4, 2 * count));
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
    
    public void AddTags(int entityId, in Tags tags)
    {
        foreach (var tag in tags) {
            ChangeTag(entityId, tag.TagIndex, TagChange.Add);
        }
    }
    
    public void RemoveTag<T>(int entityId)
        where T : struct, ITag
    {
        ChangeTag(entityId, TagType<T>.TagIndex, TagChange.Remove);
    }
    
    public void RemoveTags(int entityId, in Tags tags)
    {
        foreach (var tag in tags) {
            ChangeTag(entityId, tag.TagIndex, TagChange.Remove);
        }
    }
    
    private void ChangeTag(int entityId, int tagIndex, TagChange change)
    {
        var count = _tagCommandsCount;
        if (returnedBuffers) {
            throw CannotReuseCommandBuffer();
        }
        if (count == _tagCommands.Length) {
            ArrayUtils.Resize(ref _tagCommands, Math.Max(4, 2 * count));
        }
        _tagCommandsCount   = count + 1;
        ref var tagCommand  = ref _tagCommands[count];
        tagCommand.tagIndex = (byte)tagIndex;
        tagCommand.entityId = entityId;
        tagCommand.change   = change;
    }
#endregion

#region entity
    public int CreateEntity()
    {
        var id = store.NewId();
        var count = _entityCommandCount; 
        if (returnedBuffers) {
            throw CannotReuseCommandBuffer();
        }
        if (count == _entityCommands.Length) {
            ArrayUtils.Resize(ref _entityCommands, Math.Max(4, 2 * count));
        }
        _entityCommandCount = count + 1;
        ref var command     = ref _entityCommands[count];
        command.entityId    = id;
        command.action      = EntityCommandAction.Create;
        return id;
    }
    
    public void DeleteEntity(int entityId)
    {
        var count = _entityCommandCount; 
        if (count == _entityCommands.Length) {
            ArrayUtils.Resize(ref _entityCommands, Math.Max(4, 2 * count));
        }
        _entityCommandCount = count + 1;
        ref var command     = ref _entityCommands[count];
        command.entityId    = entityId;
        command.action      = EntityCommandAction.Delete;
    }
    #endregion
}


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

/// <summary>
/// A command buffer enables recording entity changes on <b>arbitrary</b> threads.<br/>
/// These changes are executed by calling <see cref="Playback"/> on the <b>main</b> thread.
/// </summary>
// Note: CommandBuffer is not a struct. Reasons:
// - struct need to be passed as a ref parameter => easy to forget
// - ref parameters cannot be used in lambdas
public sealed class CommandBuffer
{
#region public properties
    /// <summary> Return the number of recorded components commands. </summary>
    public              int                 ComponentCommandsCount  => GetComponentCommandsCount(intern._componentCommandTypes);
    /// <summary> Return the number of recorded tag commands. </summary>
    public              int                 TagCommandsCount        => intern._tagCommandsCount;
    /// <summary> Return the number of recorded entity commands. </summary>
    public              int                 EntityCommandsCount     => intern._entityCommandCount;
    /// <summary>
    /// Set <see cref="ReuseBuffer"/> = true to reuse a <see cref="CommandBuffer"/> instance for multiple <see cref="Playback"/>'s.
    /// </summary>
    public              bool                ReuseBuffer             { get => intern.reuseBuffer; set => intern.reuseBuffer = value; }
    
    public override     string              ToString() => $"component commands: {ComponentCommandsCount}  tag commands: {TagCommandsCount}"; 

    #endregion
    
#region interal debugging properties
    internal ReadOnlySpan<TagCommand>       TagCommands             => new (intern._tagCommands,    0, intern._tagCommandsCount);
    internal ReadOnlySpan<EntityCommand>    EntityCommands          => new (intern._entityCommands, 0, intern._entityCommandCount);
    internal ComponentCommands[]            ComponentCommands       => GetComponentCommands();
    #endregion
    
#region private fields

    private             Intern              intern;
    
    private struct Intern {
        internal            ComponentTypes      _changedComponentTypes;
        internal readonly   ComponentCommands[] _componentCommandTypes;
        
        internal            TagCommand[]        _tagCommands;
        internal            int                 _tagCommandsCount;
        //
        internal            EntityCommand[]     _entityCommands;
        internal            int                 _entityCommandCount;
        
        internal readonly   EntityStore         store;
        internal            bool                reuseBuffer;
        internal            bool                returnedBuffer;
        
        internal Intern(EntityStore store, ComponentCommands[] componentCommandTypes) {
            this.store              = store;
            _componentCommandTypes  = componentCommandTypes;
        }
    }
    #endregion
    
#region general methods
    internal CommandBuffer(EntityStore store)
    {
        var schema          = EntityStoreBase.Static.EntitySchema;
        var maxStructIndex  = schema.maxStructIndex;
        var componentTypes  = schema.components;

        var commands = new ComponentCommands[maxStructIndex];
        for (int n = 1; n < maxStructIndex; n++) {
            commands[n] = componentTypes[n].CreateComponentCommands();
        }
        intern = new Intern(store, commands) {
            _tagCommands    = Array.Empty<TagCommand>(),
            _entityCommands = Array.Empty<EntityCommand>()
        };
    }
    
    /// <summary>
    /// Execute recorded entity changes. <see cref="Playback"/> must be called on the <b>main</b> thread. 
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// When recording commands after calling <see cref="Playback"/>.<br/>
    /// To reuse a <see cref="CommandBuffer"/> instance set <see cref="ReuseBuffer"/> = true.
    /// </exception>
    public void Playback()
    {
        var componentCommands   = intern._componentCommandTypes;
        var playback            = intern.store.GetPlayback();
        try {
            var hasComponentChanges = intern._changedComponentTypes.Count > 0;
            
            ExecuteEntityCommands(intern._entityCommands);
            ExecuteTagCommands(playback, intern._tagCommands);
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
            if (!intern.reuseBuffer) {
                ReturnBuffer();
            }
        }
    }
    
    /// <summary>
    /// Return the resources of the <see cref="CommandBuffer"/> to the <see cref="EntityStore"/>.
    /// </summary>
    public void ReturnBuffer()
    {
        if (!intern.returnedBuffer) {
            intern.store.ReturnCommandBuffer(this);
            intern.returnedBuffer = true;
        }
    }
    
    internal void Reuse()
    {
        intern.returnedBuffer   = false;
        intern.reuseBuffer      = false;
    }
    
    private void ExecuteEntityCommands(EntityCommand[] entityCommands)
    {
        int count = intern._entityCommandCount;
        if (count == 0) {
            return;
        }
        var commands    = entityCommands.AsSpan(0, count);
        var store       = intern.store;
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
        var count = intern._tagCommandsCount;
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
        foreach (var componentType in intern._changedComponentTypes)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.UpdateComponentTypes(playback);
        }
    }
    
    private void ExecuteComponentCommands(Playback playback, ComponentCommands[] componentCommands)
    {
        foreach (var componentType in intern._changedComponentTypes)
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
        foreach (var componentType in intern._changedComponentTypes)
        {
            componentCommands[componentType.StructIndex].commandCount = 0;
        }
        intern._changedComponentTypes   = default;
        intern._tagCommandsCount        = 0;
        intern._entityCommandCount      = 0;
    }
    
    private int GetComponentCommandsCount(ComponentCommands[] componentCommands) {
        int count = 0;
        foreach (var componentType in intern._changedComponentTypes) {
            count += componentCommands[componentType.StructIndex].commandCount;
        }
        return count;
    }
    
    private InvalidOperationException CannotReuseCommandBuffer()
    {
        if (intern.reuseBuffer) {
            return new InvalidOperationException("CommandBuffer - buffers returned to store");    
        }
        return new InvalidOperationException("Reused CommandBuffer after Playback(). ReuseBuffer: false");
    }
    
    private ComponentCommands[] GetComponentCommands()
    {
        var commands    = new ComponentCommands[intern._changedComponentTypes.Count];
        int pos         = 0;
        var commandTypes =  intern._componentCommandTypes;
        foreach (var commandType in intern._changedComponentTypes)
        {
            commands[pos++] = commandTypes[commandType.StructIndex];
        }
        return commands;
    }
    
    #endregion
        
#region component
    /// <summary>
    /// Add the <see cref="IComponent"/> with type <typeparamref name="T"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddComponent<T>(int entityId)
        where T : struct, IComponent
    {
        ChangeComponent<T>(default, entityId,ComponentChangedAction.Add);
    }
    
    /// <summary>
    /// Add the given <paramref name="component"/> with type <typeparamref name="T"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        ChangeComponent(component,  entityId, ComponentChangedAction.Add);
    }
    
    /// <summary>
    /// Set the given <paramref name="component"/> with type <typeparamref name="T"/> of the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void SetComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        ChangeComponent(component,  entityId, ComponentChangedAction.Update);
    }
    
    /// <summary>
    /// Remove the <see cref="IComponent"/> with type <typeparamref name="T"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveComponent<T>(int entityId)
        where T : struct, IComponent
    {
        ChangeComponent<T>(default, entityId, ComponentChangedAction.Remove);
    }
    
    private void ChangeComponent<T>(in T component, int entityId, ComponentChangedAction change)
        where T : struct, IComponent
    {
        if (intern.returnedBuffer) {
            throw CannotReuseCommandBuffer();   
        }
        var structIndex = StructHeap<T>.StructIndex;
        intern._changedComponentTypes.bitSet.SetBit(structIndex);
        var commands    = (ComponentCommands<T>)intern._componentCommandTypes[structIndex];
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
    /// <summary>
    /// Add the <see cref="ITag"/> with type <typeparamref name="T"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddTag<T>(int entityId)
        where T : struct, ITag
    {
        ChangeTag(entityId, TagType<T>.TagIndex, TagChange.Add);
    }
    
    /// <summary>
    /// Add the <paramref name="tags"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddTags(int entityId, in Tags tags)
    {
        foreach (var tag in tags) {
            ChangeTag(entityId, tag.TagIndex, TagChange.Add);
        }
    }
    
    /// <summary>
    /// Remove the <see cref="ITag"/> with type <typeparamref name="T"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveTag<T>(int entityId)
        where T : struct, ITag
    {
        ChangeTag(entityId, TagType<T>.TagIndex, TagChange.Remove);
    }
    
    /// <summary>
    /// Remove the <paramref name="tags"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveTags(int entityId, in Tags tags)
    {
        foreach (var tag in tags) {
            ChangeTag(entityId, tag.TagIndex, TagChange.Remove);
        }
    }
    
    private void ChangeTag(int entityId, int tagIndex, TagChange change)
    {
        if (intern.returnedBuffer) {
            throw CannotReuseCommandBuffer();
        }
        var count = intern._tagCommandsCount;
        if (count == intern._tagCommands.Length) {
            ArrayUtils.Resize(ref intern._tagCommands, Math.Max(4, 2 * count));
        }
        intern._tagCommandsCount   = count + 1;
        ref var tagCommand  = ref intern._tagCommands[count];
        tagCommand.tagIndex = (byte)tagIndex;
        tagCommand.entityId = entityId;
        tagCommand.change   = change;
    }
#endregion

#region entity
    /// <summary>
    /// Creates a new entity on <see cref="Playback"/> which will have the returned entity id.
    /// </summary>
    public int CreateEntity()
    {
        if (intern.returnedBuffer) {
            throw CannotReuseCommandBuffer();
        }
        var id = intern.store.NewId();
        var count = intern._entityCommandCount; 

        if (count == intern._entityCommands.Length) {
            ArrayUtils.Resize(ref intern._entityCommands, Math.Max(4, 2 * count));
        }
        intern._entityCommandCount  = count + 1;
        ref var command             = ref intern._entityCommands[count];
        command.entityId            = id;
        command.action              = EntityCommandAction.Create;
        return id;
    }
    
    /// <summary>
    /// Deletes the entity with the passed <paramref name="entityId"/> on <see cref="Playback"/>.
    /// </summary>
    public void DeleteEntity(int entityId)
    {
        if (intern.returnedBuffer) {
            throw CannotReuseCommandBuffer();
        }
        var count =  intern._entityCommandCount; 
        if (count == intern._entityCommands.Length) {
            ArrayUtils.Resize(ref intern._entityCommands, Math.Max(4, 2 * count));
        }
        intern._entityCommandCount  = count + 1;
        ref var command             = ref intern._entityCommands[count];
        command.entityId            = entityId;
        command.action              = EntityCommandAction.Delete;
    }
    #endregion
}


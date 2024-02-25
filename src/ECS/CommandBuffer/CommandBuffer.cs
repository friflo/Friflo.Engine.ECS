// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable TooWideLocalVariableScope
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A command buffer enables recording entity changes on <b>arbitrary</b> threads.<br/>
/// These changes are executed by calling <see cref="Playback"/> on the <b>main</b> thread.
/// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#commandbuffer">Example.</a>
/// </summary>
// Note: CommandBuffer is not a struct. Reasons:
// - struct need to be passed as a ref parameter => easy to forget
// - ref parameters cannot be used in lambdas
public sealed class CommandBuffer
{
#region public properties
    /// <summary> Return the number of recorded components commands. </summary>
    [Browse(Never)] public  int             ComponentCommandsCount  => GetComponentCommandsCount(intern.componentCommandTypes);
    
    /// <summary> Return the number of recorded tag commands. </summary>
    [Browse(Never)] public  int             TagCommandsCount        => intern.tagCommandsCount;
    
    /// <summary> Return the number of recorded script commands. </summary>
    [Browse(Never)] public  int             ScriptCommandsCount     => intern.scriptCommandsCount;
    
    /// <summary> Return the number of recorded add / remove child commands. </summary>
    [Browse(Never)] public  int             ChildCommandsCount      => intern.childCommandsCount;
    
    /// <summary> Return the number of recorded entity commands. </summary>
    [Browse(Never)] public  int             EntityCommandsCount     => intern.entityCommandCount;

    /// <summary>
    /// Set <see cref="ReuseBuffer"/> = true to reuse a <see cref="CommandBuffer"/> instance for multiple <see cref="Playback"/>'s.
    /// </summary>
    public                  bool            ReuseBuffer             { get => intern.reuseBuffer; set => intern.reuseBuffer = value; }
    
    public override     string              ToString() => $"component commands: {ComponentCommandsCount}  tag commands: {TagCommandsCount}";
    #endregion
    
#region internal debugging properties
    internal ReadOnlySpan<TagCommand>       TagCommands             => new (intern.tagCommands,    0, intern.tagCommandsCount);
    internal ReadOnlySpan<ScriptCommand>    ScriptCommands          => new (intern.scriptCommands, 0, intern.scriptCommandsCount);
    internal ReadOnlySpan<ChildCommand>     ChildCommands           => new (intern.childCommands,  0, intern.childCommandsCount);
    internal ReadOnlySpan<EntityCommand>    EntityCommands          => new (intern.entityCommands, 0, intern.entityCommandCount);
    internal ComponentCommands[]            ComponentCommands       => GetComponentCommands();
    #endregion
    
#region private fields
    private  Intern intern;
    
    // MUST be private by all means. Used to reduce noise of fields in debugger.
    private struct Intern {
        internal            ComponentTypes      changedComponentTypes;
        internal readonly   ComponentCommands[] componentCommandTypes;
        //
        internal            TagCommand[]        tagCommands;
        internal            int                 tagCommandsCount;
        //
        internal            ScriptCommand[]     scriptCommands;
        internal            int                 scriptCommandsCount;
        //
        internal            ChildCommand[]      childCommands;
        internal            int                 childCommandsCount;
        //
        internal            EntityCommand[]     entityCommands;
        internal            int                 entityCommandCount;
        //
        internal readonly   EntityStore         store;
        internal            bool                reuseBuffer;
        internal            bool                returnedBuffer;
        
        internal Intern(EntityStore store, ComponentCommands[] componentCommandTypes) {
            this.store                  = store;
            this.componentCommandTypes  = componentCommandTypes;
        }
        
        internal void Reset(bool hasComponentChanges)
        {
            if (hasComponentChanges)
            {
                var componentCommands = componentCommandTypes;
                foreach (var componentType in changedComponentTypes) {
                    componentCommands[componentType.StructIndex].commandCount = 0;
                }
            }
            changedComponentTypes    = default;
            tagCommandsCount         = 0;
            scriptCommandsCount      = 0;
            childCommandsCount       = 0;
            entityCommandCount       = 0;
        }
    }
    
    // use nested class to minimize noise in debugger
    private static class Static
    {
        internal static readonly ComponentType[] ComponentTypes = EntityStoreBase.Static.EntitySchema.components;
    }
    #endregion
    
#region general methods
    internal CommandBuffer(EntityStore store)
    {
        var schema      = EntityStoreBase.Static.EntitySchema;
        var commands    = new ComponentCommands[schema.maxStructIndex];
        intern = new Intern(store, commands) {
            tagCommands     = Array.Empty<TagCommand>(),
            scriptCommands  = Array.Empty<ScriptCommand>(),
            childCommands   = Array.Empty<ChildCommand>(),
            entityCommands  = Array.Empty<EntityCommand>()
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
        var playback            = intern.store.GetPlayback();
        var hasComponentChanges = intern.changedComponentTypes.Count > 0;
        try {
            if (intern.entityCommandCount > 0) {
                ExecuteEntityCommands();
            }
            if (intern.tagCommandsCount > 0) {
                ExecuteTagCommands      (playback);
            }
            if (hasComponentChanges) {
                PrepareComponentCommands(playback);
            }
            UpdateEntityArchetypes      (playback);
            if (hasComponentChanges) {
                ExecuteComponentCommands(playback);
            }
            if (intern.scriptCommandsCount > 0) {
                ExecuteScriptCommands();
            }
            if (intern.childCommandsCount > 0) {
                ExecuteChildCommands();
            }
        }
        finally {
            intern.Reset(hasComponentChanges);
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
    
    private void ExecuteEntityCommands()
    {
        var commands    = intern.entityCommands.AsSpan(0, intern.entityCommandCount);
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
    
    private void ExecuteTagCommands(Playback playback)
    {
        var entityChanges   = playback.entityChanges;
        var nodes           = playback.store.nodes.AsSpan(); 
        var commands        = intern.tagCommands.AsSpan(0, intern.tagCommandsCount);
        bool exists;
        
        foreach (var tagCommand in commands)
        {
            var entityId = tagCommand.entityId;
#if NET6_0_OR_GREATER
            ref var change = ref CollectionsMarshal.GetValueRefOrAddDefault(entityChanges, entityId, out exists);
#else
            exists         = entityChanges.TryGetValue(entityId, out var change);
#endif
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
            MapUtils.Set(entityChanges, entityId, change);
        }
    }
    
    private void ExecuteScriptCommands()
    {
        var commands    = intern.scriptCommands.AsSpan(0, intern.scriptCommandsCount);
        var store       = intern.store;

        foreach (var command in commands)
        {
            var entity = new Entity(store, command.entityId);
            switch (command.action)
            {
                case ScriptChangedAction.Add:
                    EntityUtils.AddScript(entity, command.script);
                    break;
                case ScriptChangedAction.Remove:
                    EntityUtils.RemoveScript(entity, command.scriptIndex);
                    break;
            }
        }
    }
    
    private void ExecuteChildCommands()
    {
        var commands    = intern.childCommands.AsSpan(0, intern.childCommandsCount);
        var store       = intern.store;

        foreach (var command in commands)
        {
            switch (command.action)
            {
                case ChildEntitiesChangedAction.Add:
                    store.AddChild(command.parentId, command.childId);
                    break;
                case ChildEntitiesChangedAction.Remove:
                    store.RemoveChild(command.parentId, command.childId);
                    break;
            }
        }
    }
    
    private void PrepareComponentCommands(Playback playback)
    {
        var componentCommands = intern.componentCommandTypes;
        foreach (var componentType in intern.changedComponentTypes)
        {
            var commands = componentCommands[componentType.StructIndex];
            commands.UpdateComponentTypes(playback);
        }
    }
    
    private void ExecuteComponentCommands(Playback playback)
    {
        var componentCommands   = intern.componentCommandTypes;
        foreach (var componentType in intern.changedComponentTypes)
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
        var entityChanges       = playback.entityChanges;
        
        foreach (var entityId in entityChanges.Keys)
        {
            var change          = entityChanges[entityId]; 
            ref var node        = ref nodes[entityId];
            var curArchetype    = node.Archetype;
            if (curArchetype.componentTypes.bitSet.Equals(change.componentTypes.bitSet) &&
                curArchetype.tags.          bitSet.Equals(change.tags.          bitSet)) {
                continue;
            }
            // case: archetype changed 
            var newArchetype    = store.GetArchetype(change.componentTypes, change.tags);
            node.archetype      = newArchetype;
            node.compIndex      = Archetype.MoveEntityTo(curArchetype, entityId, node.compIndex, newArchetype);
        }
    }
    
    private int GetComponentCommandsCount(ComponentCommands[] componentCommands) {
        int count = 0;
        foreach (var componentType in intern.changedComponentTypes) {
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
        var commands    = new ComponentCommands[intern.changedComponentTypes.Count];
        int pos         = 0;
        var commandTypes =  intern.componentCommandTypes;
        foreach (var commandType in intern.changedComponentTypes)
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
        intern.changedComponentTypes.bitSet.SetBit(structIndex);
        var componentCommands   = intern.componentCommandTypes[structIndex];
        componentCommands     ??= CreateComponentCommands(structIndex);
        var commands            = (ComponentCommands<T>)componentCommands;
        var count               = commands.commandCount; 
        if (count == commands.componentCommands.Length) {
            ArrayUtils.Resize(ref commands.componentCommands, 2 * count);
        }
        commands.commandCount   = count + 1;
        ref var command         = ref commands.componentCommands[count];
        command.change          = change;
        command.entityId        = entityId;
        command.component       = component;
    }
    
    private  ComponentCommands CreateComponentCommands(int structIndex) {
        var componentType = Static.ComponentTypes[structIndex];
        return intern.componentCommandTypes[structIndex] = componentType.CreateComponentCommands();
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
        var count = intern.tagCommandsCount;
        if (count == intern.tagCommands.Length) {
            ArrayUtils.Resize(ref intern.tagCommands, Math.Max(4, 2 * count));
        }
        intern.tagCommandsCount = count + 1;
        ref var command     = ref intern.tagCommands[count];
        command.tagIndex    = (byte)tagIndex;
        command.entityId    = entityId;
        command.change      = change;
    }
#endregion

#region script
    /// <summary>
    /// Add the given <paramref name="script"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddScript<T>(int entityId, T script)
        where T : Script, new()
    {
        ChangeScript(entityId, script, ScriptType<T>.Index, ScriptChangedAction.Add);
    }
        
    /// <summary>
    /// Remove the <see cref="Script"/> of the specified type <typeparamref name="T"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveScript<T>(int entityId)
        where T : Script, new()
    {
        ChangeScript(entityId, null, ScriptType<T>.Index, ScriptChangedAction.Remove);
    }
    
    private void ChangeScript(int entityId, Script script, int scriptIndex, ScriptChangedAction action)
    {
        if (intern.returnedBuffer) {
            throw CannotReuseCommandBuffer();
        }
        var count =  intern.scriptCommandsCount;
        if (count == intern.scriptCommands.Length) {
            ArrayUtils.Resize(ref intern.scriptCommands, Math.Max(4, 2 * count));
        }
        intern.scriptCommandsCount = count + 1;
        ref var command     = ref intern.scriptCommands[count];
        command.scriptIndex = (byte)scriptIndex;
        command.action      = action;
        command.entityId    = entityId;
        command.script      = script;
    }
    #endregion
    
#region child entity
    /// <summary>
    /// Add the entity with the given <paramref name="childId"/> as a child to the entity with the passed <paramref name="parentId"/>.
    /// </summary>
    public void AddChild(int parentId, int childId)
    {
        ChangeChild (parentId, childId, ChildEntitiesChangedAction.Add);
    }
        
    /// <summary>
    /// Remove the child entity with given <paramref name="childId"/> from the parent entity with the the passed <paramref name="parentId"/>.
    /// </summary>
    public void RemoveChild(int parentId, int childId)
    {
        ChangeChild (parentId, childId, ChildEntitiesChangedAction.Remove);
    }
    
    private void ChangeChild(int parentId, int childId, ChildEntitiesChangedAction action)
    {
        if (intern.returnedBuffer) {
            throw CannotReuseCommandBuffer();
        }
        var count =  intern.childCommandsCount;
        if (count == intern.childCommands.Length) {
            ArrayUtils.Resize(ref intern.childCommands, Math.Max(4, 2 * count));
        }
        intern.childCommandsCount = count + 1;
        ref var command     = ref intern.childCommands[count];
        command.parentId    = parentId;
        command.childId     = childId;
        command.action      = action;
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
        var id      = intern.store.NewId();
        var count   = intern.entityCommandCount; 

        if (count == intern.entityCommands.Length) {
            ArrayUtils.Resize(ref intern.entityCommands, Math.Max(4, 2 * count));
        }
        intern.entityCommandCount   = count + 1;
        ref var command             = ref intern.entityCommands[count];
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
        var count =  intern.entityCommandCount; 
        if (count == intern.entityCommands.Length) {
            ArrayUtils.Resize(ref intern.entityCommands, Math.Max(4, 2 * count));
        }
        intern.entityCommandCount  = count + 1;
        ref var command             = ref intern.entityCommands[count];
        command.entityId            = entityId;
        command.action              = EntityCommandAction.Delete;
    }
    #endregion
}


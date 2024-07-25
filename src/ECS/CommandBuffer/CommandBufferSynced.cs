// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Same functionality as <see cref="CommandBuffer"/> but thread safe.<br/>
/// Use this command buffer to record entity changes in parallel queries executed via <see cref="QueryJob.RunParallel"/>.  
/// </summary>
public sealed class CommandBufferSynced : ICommandBuffer
{
#region private fields
    private readonly   CommandBuffer   commandBuffer;
    #endregion
    
    internal CommandBufferSynced(CommandBuffer commandBuffer) {
        this.commandBuffer = commandBuffer;
    }
    
#region general methods
    // ReSharper disable once InconsistentlySynchronizedField
    public void Clear() => commandBuffer.Clear();

    /// <summary>
    /// Execute recorded entity changes. <see cref="Playback"/> must be called on the <b>main</b> thread.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#commandbuffer">Example.</a>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// When recording commands after calling <see cref="Playback"/>.<br/>
    /// </exception>
    // ReSharper disable once InconsistentlySynchronizedField
    public void Playback() => commandBuffer.Playback();
    #endregion
        
#region component
    /// <summary>
    /// Add the <see cref="IComponent"/> with type <typeparamref name="T"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddComponent<T>(int entityId)
        where T : struct, IComponent
    {
        lock (this) { commandBuffer.AddComponent<T>(entityId); }
    }
    
    /// <summary>
    /// Add the given <paramref name="component"/> with type <typeparamref name="T"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        lock (this) { commandBuffer.AddComponent(entityId, component); }
    }
    
    /// <summary>
    /// Set the given <paramref name="component"/> with type <typeparamref name="T"/> of the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void SetComponent<T>(int entityId, in T component)
        where T : struct, IComponent
    {
        lock (this) { commandBuffer.SetComponent(entityId, component); }
    }
    
    /// <summary>
    /// Remove the <see cref="IComponent"/> with type <typeparamref name="T"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveComponent<T>(int entityId)
        where T : struct, IComponent
    {
        lock (this) { commandBuffer.RemoveComponent<T>(entityId); }
    }
    #endregion
    
#region tag
    /// <summary>
    /// Add the <see cref="ITag"/> with type <typeparamref name="T"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddTag<T>(int entityId)
        where T : struct, ITag
    {
        lock (this) { commandBuffer.AddTag<T>(entityId); }
    }
    
    /// <summary>
    /// Add the <paramref name="tags"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddTags(int entityId, in Tags tags)
    {
        lock (this) { commandBuffer.AddTags(entityId, tags); }
    }
    
    /// <summary>
    /// Remove the <see cref="ITag"/> with type <typeparamref name="T"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveTag<T>(int entityId)
        where T : struct, ITag
    {
        lock (this) { commandBuffer.RemoveTag<T>(entityId); }
    }
    
    /// <summary>
    /// Remove the <paramref name="tags"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveTags(int entityId, in Tags tags)
    {
        lock (this) { commandBuffer.RemoveTags(entityId, tags); }
    }
#endregion

#region script
    /// <summary>
    /// Add the given <paramref name="script"/> to the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void AddScript<T>(int entityId, T script)
        where T : Script, new()
    {
        lock (this) { commandBuffer.AddScript(entityId, script); }
    }
        
    /// <summary>
    /// Remove the <see cref="Script"/> of the specified type <typeparamref name="T"/> from the entity with the passed <paramref name="entityId"/>.
    /// </summary>
    public void RemoveScript<T>(int entityId)
        where T : Script, new()
    {
        lock (this) { commandBuffer.RemoveScript<T>(entityId); }
    }
    #endregion
    
#region child entity
    /// <summary>
    /// Add the entity with the given <paramref name="childId"/> as a child to the entity with the passed <paramref name="parentId"/>.
    /// </summary>
    public void AddChild(int parentId, int childId)
    {
        lock (this) { commandBuffer.AddChild(parentId, childId); }
    }
        
    /// <summary>
    /// Remove the child entity with given <paramref name="childId"/> from the parent entity with the the passed <paramref name="parentId"/>.
    /// </summary>
    public void RemoveChild(int parentId, int childId)
    {
        lock (this) { commandBuffer.RemoveChild(parentId, childId); }
    }
    #endregion

#region entity
    /// <summary>
    /// Creates a new entity on <see cref="Playback"/> which will have the returned entity id.
    /// </summary>
    public int CreateEntity()
    {
        lock (this) { return commandBuffer.CreateEntity(); }
    }
    
    /// <summary>
    /// Deletes the entity with the passed <paramref name="entityId"/> on <see cref="Playback"/>.
    /// </summary>
    public void DeleteEntity(int entityId)
    {
        lock (this) { commandBuffer.DeleteEntity(entityId); }
    }
    #endregion
}


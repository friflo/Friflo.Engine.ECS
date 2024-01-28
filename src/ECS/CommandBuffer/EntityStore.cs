// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public partial class EntityStore
{
#pragma warning disable CS0618 // Type or member is obsolete
    public CommandBuffer GetCommandBuffer()
    {
        var pool = intern.commandBufferPool ??= new Stack<CommandBuffer>();
        lock (pool)
        {
            if (pool.TryPop(out var buffer)) {
                buffer.returnedBuffer   = false;
                buffer.reuseBuffer      = false;
                return buffer;
            }
        }
        return new CommandBuffer(this);
    }
    
    internal void ReturnCommandBuffer(CommandBuffer commandBuffer)
    {
        var pool = intern.commandBufferPool;
        lock (pool) {
            pool.Push(commandBuffer);
        }
    }
    
    internal Playback GetPlayback()
    {
        if (intern.playback.entityChanges == null) {
            intern.playback = new Playback(this);
        }
        return intern.playback;
    }
}
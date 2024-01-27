// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStore
{
    internal ComponentCommands[] GetCommandBuffers()
    {
        var pool = intern.commandBufferPool ??= new Stack<ComponentCommands[]>();
        lock (pool)
        {
            if (pool.TryPop(out var buffers)) {
                return buffers;
            }
        }
        var schema          = Static.EntitySchema;
        var maxStructIndex  = schema.maxStructIndex;
        var componentTypes  = schema.components;

        var commands = new ComponentCommands[maxStructIndex];
        for (int n = 1; n < maxStructIndex; n++) {
            commands[n] = componentTypes[n].CreateComponentCommands();
        }
        return commands;
    }
    
    internal void ReturnCommandBuffers(ComponentCommands[] componentCommands)
    {
        var pool = intern.commandBufferPool;
        lock (pool) {
            pool.Push(componentCommands);
        }
    }
    
    internal Playback GetPlayback()
    {
        if (intern.playback.entities == null) {
            intern.playback = new Playback(this);
        }
        return intern.playback;
    }
}
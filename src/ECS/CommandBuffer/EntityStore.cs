// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal struct CommandBuffers
{
    internal    ComponentCommands[] componentCommands;
    internal    TagCommand[]        tagCommands;
    internal    EntityCommand[]     entityCommands;
}

public partial class EntityStore
{
    internal CommandBuffers GetCommandBuffers()
    {
        var pool = intern.commandBufferPool ??= new Stack<CommandBuffers>();
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
        return new CommandBuffers {
            componentCommands   = commands,
            tagCommands         = Array.Empty<TagCommand>(),
            entityCommands      = Array.Empty<EntityCommand>()
        };
    }
    
    internal void ReturnCommandBuffers(
        ComponentCommands[] componentCommands,
        TagCommand[]        tagCommands,
        EntityCommand[]     entityCommands)    
    {
        var pool = intern.commandBufferPool;
        lock (pool) {
            pool.Push(new CommandBuffers {
                componentCommands   = componentCommands,
                tagCommands         = tagCommands,
                entityCommands      = entityCommands
            });
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
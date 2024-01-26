// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

#pragma warning disable CS0618 // Type or member is obsolete


internal struct StoreCommandBuffers
{
    internal    ComponentCommands[] componentCommands;
    internal    EntityChanges       entityChanges;
    
    internal static readonly int            MaxStructIndex = EntityStoreBase.Static.EntitySchema.maxStructIndex;
    internal static readonly ComponentType[] ComponentTypes = EntityStoreBase.Static.EntitySchema.components;
}

public partial class EntityStore
{
    private readonly Stack<StoreCommandBuffers> commandBufferPool = new ();
    
    internal StoreCommandBuffers GetCommandBuffers()
    {
        lock (commandBufferPool)
        {
            if (commandBufferPool.TryPop(out var buffers)) {
                return buffers;
            }
        }

        var commands = new ComponentCommands[StoreCommandBuffers.MaxStructIndex];
        for (int n = 1; n < StoreCommandBuffers.MaxStructIndex; n++) {
            commands[n] = StoreCommandBuffers.ComponentTypes[n].CreateComponentCommands();
        }
        return new StoreCommandBuffers {
            componentCommands   = commands,
            entityChanges       = new EntityChanges(this)
        };
    }
    
    internal void ReturnCommandBuffers(ComponentCommands[] componentCommands, in EntityChanges entityChanges)
    {
        lock (commandBufferPool) {
            commandBufferPool.Push(new StoreCommandBuffers {
                componentCommands   = componentCommands,
                entityChanges       = entityChanges
            });
        }
    }
}
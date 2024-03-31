// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
    /// <summary> Remove the specified component and tags from the entity. </summary>
    public static void Remove<T1>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= GetTypes<T1>(stackalloc int[1]);
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }
    
    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= GetTypes<T1,T2>(stackalloc int[2]);
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }
    
    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= GetTypes<T1,T2,T3>(stackalloc int[3]);
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }
    
    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3, T4>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= GetTypes<T1,T2,T3,T4>(stackalloc int[4]);
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }
    
    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3, T4, T5>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= GetTypes<T1,T2,T3,T4,T5>(stackalloc int[5]);
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }
} 
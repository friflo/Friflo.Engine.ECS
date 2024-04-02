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
        var removeTypes     = GetComponentTypes<T1>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1>();
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
        var removeTypes     = GetComponentTypes<T1,T2>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2>();
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
        var removeTypes     = GetComponentTypes<T1,T2,T3>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3>();
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
        var removeTypes     = GetComponentTypes<T1,T2,T3,T4>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3,T4>();
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
        var removeTypes     = GetComponentTypes<T1,T2,T3,T4,T5>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3,T4,T5>();
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }
    
    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3, T4, T5, T6>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = GetComponentTypes<T1,T2,T3,T4,T5,T6>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3,T4,T5,T6>();
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }

    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3, T4, T5, T6, T7>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = GetComponentTypes<T1,T2,T3,T4,T5,T6,T7>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3,T4,T5,T6,T7>();
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }

    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3, T4, T5, T6, T7, T8>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = GetComponentTypes<T1,T2,T3,T4,T5,T6,T7,T8>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3,T4,T5,T6,T7,T8>();
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }

    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
        where T9 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = GetComponentTypes<T1,T2,T3,T4,T5,T6,T7,T8,T9>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3,T4,T5,T6,T7,T8,T9>();
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }

    /// <summary> Remove the specified components and tags from the entity. </summary>
    public static void Remove<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Entity entity, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
        where T9 : struct, IComponent
        where T10: struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeTypes     = GetComponentTypes<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>();
        var newType         = store.GetArchetypeRemove(oldType, removeTypes, tags);
        
        var removeComponents= GetIndexes<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>();
        StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, removeComponents, newType, oldType);
    }
}
















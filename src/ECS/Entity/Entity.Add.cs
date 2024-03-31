// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
    /// <summary> Add the passed component and tags to the entity. </summary>
    public static void Add<T1>(
        this Entity entity,
        in T1   component1,
        in Tags tags = default)
            where T1 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = GetTypes<T1>(stackalloc int[1]);
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = GetTypes<T1,T2>(stackalloc int[2]);
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
    {
        var store           = entity.store;
        var id              = entity.Id;
        ref var node        = ref store.nodes[id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = GetTypes<T1,T2,T3>(stackalloc int[3]);
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3, T4>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in Tags tags = default)
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
        var addComponents   = GetTypes<T1,T2,T3,T4>(stackalloc int[4]);
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3, T4, T5>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in Tags tags = default)
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
        var addComponents   = GetTypes<T1,T2,T3,T4,T5>(stackalloc int[5]);
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    

} 
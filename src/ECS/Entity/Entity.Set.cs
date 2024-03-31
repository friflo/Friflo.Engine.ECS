// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static partial class EntityExtensions
{
    /// <summary> Set the passed component on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1>(
        this Entity entity,
        in T1   component1)
            where T1 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1>(stackalloc int[1]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2>(
        this Entity entity,
        in T1   component1,
        in T2   component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2>(stackalloc int[2]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3>(stackalloc int[3]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3, T4>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3, T4>(stackalloc int[4]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3, T4, T5>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3, T4, T5>(stackalloc int[5]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
} 
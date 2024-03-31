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
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3, T4, T5, T6>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3, T4, T5, T6>(stackalloc int[6]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3, T4, T5, T6, T7>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6,
        in T7   component7)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
            where T7 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3, T4, T5, T6, T7>(stackalloc int[7]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3, T4, T5, T6, T7, T8>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6,
        in T7   component7,
        in T8   component8)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
            where T7 : struct, IComponent
            where T8 : struct, IComponent
    {
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3, T4, T5, T6, T7, T8>(stackalloc int[8]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7, component8);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6,
        in T7   component7,
        in T8   component8,
        in T9   component9)
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
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9>(stackalloc int[9]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
    
    /// <summary> Set the passed components on the entity. </summary>
    /// <exception cref="ECS.MissingComponentException"> if the entity does not contain a passed component. </exception>
    public static void Set<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6,
        in T7   component7,
        in T8   component8,
        in T9   component9,
        in T10  component10)
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
        ref var node        = ref entity.store.nodes[entity.Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = GetTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(stackalloc int[10]);
        CheckComponents(entity, components, type, componentIndex);
        
        AssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9, component10);
        
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
} 
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
        var components      = Generic<T1>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2, component3)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3,T4>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2, component3, component4)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3,T4,T5>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2, component3, component4, component5)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3,T4,T5,T6>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3,T4,T5,T6,T7>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3,T4,T5,T6,T7,T8>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7, component8)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);
        
        if (!SetAssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9)) {
            throw MissingComponentException(entity, components, type);
        }
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
        var components      = Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>.SignatureIndexes;
        StashSetComponents(entity, components, type, componentIndex);

        if (!SetAssignComponents(type, componentIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9, component10)) {
            throw MissingComponentException(entity, components, type);
        }
        // Send event. See: SEND_EVENT notes
        SendSetEvents(entity, components, type);
    }
}











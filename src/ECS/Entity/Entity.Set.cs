// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial struct  Entity
{
    public void Set<T1>(
        in T1   component1)
            where T1 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = ComponentTypes.Get<T1>();
        EntityGeneric.StashSetComponents(store, components, type, componentIndex);
        
        EntityGeneric.AssignComponents(type, componentIndex, component1);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendSetEvents(store, Id, components, type);
    }
    
    public void Set<T1, T2>(
        in T1   component1,
        in T2   component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = ComponentTypes.Get<T1, T2>();
        EntityGeneric.StashSetComponents(store, components, type, componentIndex);
        
        EntityGeneric.AssignComponents(type, componentIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendSetEvents(store, Id, components, type);
    }
    
    public void Set<T1, T2, T3>(
        in T1   component1,
        in T2   component2,
        in T3   component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = ComponentTypes.Get<T1, T2, T3>();
        EntityGeneric.StashSetComponents(store, components, type, componentIndex);
        
        EntityGeneric.AssignComponents(type, componentIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendSetEvents(store, Id, components, type);
    }
    
    public void Set<T1, T2, T3, T4>(
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = ComponentTypes.Get<T1, T2, T3, T4>();
        EntityGeneric.StashSetComponents(store, components, type, componentIndex);
        
        EntityGeneric.AssignComponents(type, componentIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendSetEvents(store, Id, components, type);
    }
    
    public void Set<T1, T2, T3, T4, T5>(
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
        ref var node        = ref store.nodes[Id];
        var type            = node.archetype;
        var componentIndex  = node.compIndex;
        var components      = ComponentTypes.Get<T1, T2, T3, T4, T5>();
        EntityGeneric.StashSetComponents(store, components, type, componentIndex);
        
        EntityGeneric.AssignComponents(type, componentIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendSetEvents(store, Id, components, type);
    }
} 
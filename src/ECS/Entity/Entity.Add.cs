// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial struct  Entity
{
    public void Add<T1>(
        in T1   component1,
        in Tags tags = default)
            where T1 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = ComponentTypes.Get<T1>();
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        EntityGeneric.StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addComponents, newType, oldType);
    }
    
    public void Add<T1, T2>(
        in T1   component1,
        in T2   component2,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = ComponentTypes.Get<T1,T2>();
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        EntityGeneric.StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addComponents, newType, oldType);
    }
    
    public void Add<T1, T2, T3>(
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = ComponentTypes.Get<T1,T2,T3>();
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        EntityGeneric.StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addComponents, newType, oldType);
    }
    
    public void Add<T1, T2, T3, T4>(
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
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = ComponentTypes.Get<T1,T2,T3,T4>();
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        EntityGeneric.StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addComponents, newType, oldType);
    }
    
    public void Add<T1, T2, T3, T4, T5>(
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
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var addComponents   = ComponentTypes.Get<T1,T2,T3,T4,T5>();
        var newType         = store.GetArchetypeAdd(oldType, addComponents, tags);
        EntityGeneric.StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addComponents, newType, oldType);
    }
    

} 
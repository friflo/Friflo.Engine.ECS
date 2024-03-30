// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Utils;

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
        var addTypes        = new BitSet();
        addTypes.SetBit(StructHeap<T1>.StructIndex);
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        EntityGeneric.StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addTypes, newType, oldType);
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
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2>(ref addTypes);
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        EntityGeneric.StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addTypes, newType, oldType);
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
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2,T3>(ref addTypes);
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        EntityGeneric.StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addTypes, newType, oldType);
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
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2,T3,T4>(ref addTypes);
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        EntityGeneric.StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addTypes, newType, oldType);
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
        var addTypes        = new BitSet();
        EntityGeneric.SetBits<T1,T2,T3,T4,T5>(ref addTypes);
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        EntityGeneric.StashAddComponents(store, addTypes, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        EntityGeneric.SetComponents(newType, newCompIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendAddEvents(store, Id, addTypes, newType, oldType);
    }
    

} 
// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial struct  Entity
{
    public void Remove<T1>(in Tags tags = default)
        where T1 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= ComponentTypes.Get<T1>();
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        EntityGeneric.StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendRemoveEvents(store, Id, removeComponents, newType, oldType);
    }
    
    public void Remove<T1, T2>(in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= ComponentTypes.Get<T1,T2>();
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        EntityGeneric.StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendRemoveEvents(store, Id, removeComponents, newType, oldType);
    }
    
    public void Remove<T1, T2, T3>(in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= ComponentTypes.Get<T1,T2,T3>();
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        EntityGeneric.StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendRemoveEvents(store, Id, removeComponents, newType, oldType);
    }
    
    public void Remove<T1, T2, T3, T4>(in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= ComponentTypes.Get<T1,T2,T3,T4>();
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        EntityGeneric.StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendRemoveEvents(store, Id, removeComponents, newType, oldType);
    }
    
    public void Remove<T1, T2, T3, T4, T5>(in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        ref var node        = ref store.nodes[Id];
        var oldType         = node.archetype;
        var oldCompIndex    = node.compIndex;
        var removeComponents= ComponentTypes.Get<T1,T2,T3,T4,T5>();
        var newType         = store.GetArchetypeRemove(oldType, removeComponents, tags);
        EntityGeneric.StashRemoveComponents(store, removeComponents, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, Id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        EntityGeneric.SendRemoveEvents(store, Id, removeComponents, newType, oldType);
    }
} 
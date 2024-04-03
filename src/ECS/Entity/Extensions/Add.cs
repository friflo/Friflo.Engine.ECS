// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
        var addTypes        = ComponentTypes.Get<T1>();
        var addComponents   = GetIndexes<T1>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
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
        var addTypes        = ComponentTypes.Get<T1,T2>();
        var addComponents   = GetIndexes<T1,T2>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3>();
        var addComponents   = GetIndexes<T1,T2,T3>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3,T4>();
        var addComponents   = GetIndexes<T1,T2,T3,T4>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3,T4,T5>();
        var addComponents   = GetIndexes<T1,T2,T3,T4,T5>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3, T4, T5, T6>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6,
        in Tags tags = default)
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3,T4,T5,T6>();
        var addComponents   = GetIndexes<T1,T2,T3,T4,T5,T6>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4, component5, component6);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3, T4, T5, T6, T7>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6,
        in T7   component7,
        in Tags tags = default)
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3,T4,T5,T6,T7>();
        var addComponents   = GetIndexes<T1,T2,T3,T4,T5,T6,T7>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4, component5, component6, component7);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3, T4, T5, T6, T7, T8>(
        this Entity entity,
        in T1   component1,
        in T2   component2,
        in T3   component3,
        in T4   component4,
        in T5   component5,
        in T6   component6,
        in T7   component7,
        in T8   component8,
        in Tags tags = default)
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3,T4,T5,T6,T7,T8>();
        var addComponents   = GetIndexes<T1,T2,T3,T4,T5,T6,T7,T8>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4, component5, component6, component7, component8);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
        in Tags tags = default)
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3,T4,T5,T6,T7,T8,T9>();
        var addComponents   = GetIndexes<T1,T2,T3,T4,T5,T6,T7,T8,T9>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }

    
    /// <summary> Add the passed components and tags to the entity. </summary>
    public static void Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
        in T10  component10,
        in Tags tags = default)
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
        var addTypes        = ComponentTypes.Get<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>();
        var addComponents   = GetIndexes<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>();
        var newType         = store.GetArchetypeAdd(oldType, addTypes, tags);
        
        StashAddComponents(store, addComponents, oldType, oldCompIndex);

        var newCompIndex    = node.compIndex = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        AssignComponents(newType, newCompIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9, component10);
        
        // Send event. See: SEND_EVENT notes
        SendAddEvents(entity, addComponents, newType, oldType);
    }
}












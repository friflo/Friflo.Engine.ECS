// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3,T4>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3,T4>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3,T4>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3,T4,T5>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3,T4,T5>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3,T4,T5>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3,T4,T5,T6>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3,T4,T5,T6>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3,T4,T5,T6>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3,T4,T5,T6,T7>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3,T4,T5,T6,T7>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3,T4,T5,T6,T7>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3,T4,T5,T6,T7,T8>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3,T4,T5,T6,T7,T8>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3,T4,T5,T6,T7,T8>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9>.SignatureIndexes, newType, oldType);
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
        var newType         = store.GetArchetypeRemove(oldType, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>.ComponentTypes, tags);
        
        StashRemoveComponents(store, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>.SignatureIndexes, oldType, oldCompIndex);

        node.compIndex      = Archetype.MoveEntityTo(oldType, id, oldCompIndex, newType);
        node.archetype      = newType;
        
        // Send event. See: SEND_EVENT notes
        SendRemoveEvents(entity, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>.SignatureIndexes, newType, oldType);
    }
}
















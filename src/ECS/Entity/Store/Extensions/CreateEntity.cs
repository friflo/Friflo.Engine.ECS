// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

using static EntityExtensions;

/// <summary>
/// Provide generic <c>CreateEntity()</c> overloads to create entities with passed components without any structural change.
/// </summary>
public static partial class EntityStoreExtensions {

#region generic create overloads
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity(this EntityStore store, in Tags tags)
    {
        var archetype   = store.GetArchetype(default, tags);
        var id          = store.NewId();
        store.CreateEntityInternal(archetype, id, out var revision);
        var entity      = new Entity(store, id, revision);
        
        // Send event. See: SEND_EVENT notes
        store.CreateEntityEvent(entity);
        var tagsChanged = store.TagsChanged;
        if (tagsChanged != null) tagsChanged(new TagsChanged(store, id, tags, default));
        return entity;
    }
   
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed component and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1>(
        this EntityStore store,
        T1      component,
        in Tags tags = default)
            where T1 : struct, IComponent
    {
        var entity  = CreateEntityGeneric(store, Generic<T1>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1>.SignatureIndexes);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        var entity  = CreateEntityGeneric(store, Generic<T1,T2>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2>.SignatureIndexes);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
    {
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3>.SignatureIndexes);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3, T4>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        T4      component4,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
    {
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3,T4>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3,T4>.SignatureIndexes);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3, T4, T5>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        T4      component4,
        T5      component5,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
    {
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3,T4,T5>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3,T4,T5>.SignatureIndexes);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3, T4, T5, T6>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        T4      component4,
        T5      component5,
        T6      component6,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
    {
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3,T4,T5,T6>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3, component4, component5, component6);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3,T4,T5,T6>.SignatureIndexes);
        return entity;
    }

    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        T4      component4,
        T5      component5,
        T6      component6,
        T7      component7,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
            where T7 : struct, IComponent
    {
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3,T4,T5,T6,T7>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3, component4, component5, component6, component7);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3,T4,T5,T6,T7>.SignatureIndexes);
        return entity;
    }

    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        T4      component4,
        T5      component5,
        T6      component6,
        T7      component7,
        T8      component8,
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
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3,T4,T5,T6,T7,T8>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3, component4, component5, component6, component7, component8);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3,T4,T5,T6,T7,T8>.SignatureIndexes);
        return entity;
    }

    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        T4      component4,
        T5      component5,
        T6      component6,
        T7      component7,
        T8      component8,
        T9      component9,
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
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9>.SignatureIndexes);
        return entity;
    }

    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        this EntityStore store,
        T1      component1,
        T2      component2,
        T3      component3,
        T4      component4,
        T5      component5,
        T6      component6,
        T7      component7,
        T8      component8,
        T9      component9,
        T10     component10,
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
        var entity  = CreateEntityGeneric(store, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>.ComponentTypes, tags, out var archetype, out int compIndex);
        AssignComponents(archetype, compIndex, component1, component2, component3, component4, component5, component6, component7, component8, component9, component10);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype, Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>.SignatureIndexes);
        return entity;
    }
    
    #endregion
    
    // ------------------------------------ generic create entity utils ------------------------------------
#region generic create utils
    private static Entity CreateEntityGeneric(
        EntityStore         store,
        in  ComponentTypes  componentTypes,
        in  Tags            tags,
        out Archetype       archetype,
        out int             compIndex)
    {
        archetype   = store.GetArchetype(componentTypes, tags);
        var id      = store.NewId();
        compIndex   = store.CreateEntityInternal(archetype, id, out var revision);
        return new Entity(store, id, revision);
    }
    
    private static void SendCreateEvents(Entity entity, Archetype archetype, in SignatureIndexes componentTypes)
    {
        var store = entity.store;
        // --- create entity event
        store.CreateEntityEvent(entity);
        
        // --- tag event
        var tagsChanged = store.TagsChanged;
        if (tagsChanged != null) {
            tagsChanged(new TagsChanged(store, entity.Id, archetype.tags, default));
        }
        // --- component events 
        var componentAdded = store.ComponentAdded;
        if (componentAdded == null) {
            return;
        }
        foreach (var structIndex in componentTypes) {
            componentAdded(new ComponentChanged (store, entity.Id, ComponentChangedAction.Add, structIndex, null));    
        }
    }
    #endregion
}

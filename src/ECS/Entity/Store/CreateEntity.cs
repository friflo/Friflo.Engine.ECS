// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using static Friflo.Engine.ECS.StoreOwnership;
using static Friflo.Engine.ECS.TreeMembership;

// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStore
{
    /// <summary>
    /// Create and return a new <see cref="Entity"/> in the entity store.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#entity">Example.</a>
    /// </summary>
    /// <returns>An <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public Entity CreateEntity()
    {
        var id = NewId();
        CreateEntityInternal(defaultArchetype, id);
        var entity = new Entity(this, id);
        
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(entity);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed <paramref name="id"/> in the entity store.
    /// </summary>
    /// <returns>an <see cref="attached"/> and <see cref="floating"/> entity</returns>
    public Entity CreateEntity(int id)
    {
        CheckEntityId(id);
        CreateEntityInternal(defaultArchetype, id);
        var entity = new Entity(this, id); 
        
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(entity);
        return entity;
    }
}

public static class EntityStoreExtensions {
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed <paramref name="tags"/>.
    /// </summary>
    public static Entity CreateEntity(this EntityStore store, in Tags tags)
    {
        var archetype   = store.GetArchetype(default, tags);
        var id          = store.NewId();
        store.CreateEntityInternal(archetype, id);
        var entity      = new Entity(store, id);
        
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
        var componentTypes  = ComponentTypes.Get<T1>();
        var entity          = CreateEntityGeneric(store, componentTypes, tags, out var archetype, out int compIndex);
        EntityExtensions.AssignComponents(archetype, compIndex, component);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
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
        var componentTypes  = ComponentTypes.Get<T1,T2>();
        var entity          = CreateEntityGeneric(store, componentTypes, tags, out var archetype, out int compIndex);
        EntityExtensions.AssignComponents(archetype, compIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
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
        var componentTypes  = ComponentTypes.Get<T1,T2,T3>();
        var entity          = CreateEntityGeneric(store, componentTypes, tags, out var archetype, out int compIndex);
        EntityExtensions.AssignComponents(archetype, compIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
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
        var componentTypes  = ComponentTypes.Get<T1,T2,T3,T4>();
        var entity          = CreateEntityGeneric(store, componentTypes, tags, out var archetype, out int compIndex);
        EntityExtensions.AssignComponents(archetype, compIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
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
        var componentTypes  = ComponentTypes.Get<T1,T2,T3,T4,T5>();
        var entity          = CreateEntityGeneric(store, componentTypes, tags, out var archetype, out int compIndex);
        EntityExtensions.AssignComponents(archetype, compIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
        return entity;
    }
    
    private static Entity CreateEntityGeneric(EntityStore store, in ComponentTypes componentTypes, in Tags tags, out Archetype archetype, out int compIndex)
    {
        archetype   = store.GetArchetype(componentTypes, tags);
        var id      = store.NewId();
        compIndex   = store.CreateEntityInternal(archetype, id);
        return new Entity(store, id);
    }
    
    private static void SendCreateEvents(Entity entity, Archetype archetype)
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
        foreach (var heap in archetype.structHeaps) {
            componentAdded(new ComponentChanged (store, entity.Id, ComponentChangedAction.Add, heap.structIndex, null));    
        }
    }
}

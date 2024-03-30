// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Utils;
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
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed <paramref name="tags"/>.
    /// </summary>
    public Entity CreateEntity(in Tags tags)
    {
        var archetype   = GetArchetype(default, tags);
        var id          = NewId();
        CreateEntityInternal(archetype, id);
        var entity      = new Entity(this, id);
        
        // Send event. See: SEND_EVENT notes
        CreateEntityEvent(entity);
        var tagsChanged = TagsChanged;
        if (tagsChanged != null) tagsChanged(new TagsChanged(this, id, tags, default));
        return entity;
    }
   
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed component and <paramref name="tags"/>.
    /// </summary>
    public Entity CreateEntity<T1>(
        T1 component,
        in Tags tags = default)
            where T1 : struct, IComponent
    {
        var bitSet          = new BitSet();
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        
        var entity  = CreateEntityGeneric(bitSet, tags, out var archetype, out int compIndex);
        EntityGeneric.SetComponents1(archetype.heapMap, compIndex, component);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public Entity CreateEntity<T1, T2>(
        T1 component1,
        T2 component2,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
    {
        var bitSet          = new BitSet();
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        
        var entity  = CreateEntityGeneric(bitSet, tags, out var archetype, out int compIndex);
        EntityGeneric.SetComponents2(archetype.heapMap, compIndex, component1, component2);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public Entity CreateEntity<T1, T2, T3>(
        T1 component1,
        T2 component2,
        T3 component3,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
    {
        var bitSet          = new BitSet();
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        
        var entity = CreateEntityGeneric(bitSet, tags, out var archetype, out int compIndex);
        EntityGeneric.SetComponents3(archetype.heapMap, compIndex, component1, component2, component3);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public Entity CreateEntity<T1, T2, T3, T4>(
        T1 component1,
        T2 component2,
        T3 component3,
        T4 component4,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
    {
        var bitSet          = new BitSet();
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        
        var entity  = CreateEntityGeneric(bitSet, tags, out var archetype, out int compIndex);
        EntityGeneric.SetComponents4(archetype.heapMap, compIndex, component1, component2, component3, component4);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
        return entity;
    }
    
    /// <summary>
    /// Create and return a new <see cref="Entity"/> with the passed components and <paramref name="tags"/>.
    /// </summary>
    public Entity CreateEntity<T1, T2, T3, T4, T5>(
        T1 component1,
        T2 component2,
        T3 component3,
        T4 component4,
        T5 component5,
        in Tags tags = default)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
    {
        var bitSet          = new BitSet();
        bitSet.SetBit(StructHeap<T1>.StructIndex);
        bitSet.SetBit(StructHeap<T2>.StructIndex);
        bitSet.SetBit(StructHeap<T3>.StructIndex);
        bitSet.SetBit(StructHeap<T4>.StructIndex);
        bitSet.SetBit(StructHeap<T5>.StructIndex);
        
        var entity  = CreateEntityGeneric(bitSet, tags, out var archetype, out int compIndex);
        EntityGeneric.SetComponents5(archetype.heapMap, compIndex, component1, component2, component3, component4, component5);
        
        // Send event. See: SEND_EVENT notes
        SendCreateEvents(entity, archetype);
        return entity;
    }
    
    private Entity CreateEntityGeneric(in BitSet bitSet, in Tags tags, out Archetype archetype, out int compIndex)
    {
        archetype   = GetArchetype(new ComponentTypes { bitSet = bitSet }, tags);
        var id      = NewId();
        compIndex   = CreateEntityInternal(archetype, id);
        return new Entity(this, id);
    }
    
    private void SendCreateEvents(Entity entity, Archetype archetype)
    {
        // --- create entity event
        CreateEntityEvent(entity);
        
        // --- tag event
        var tagsChanged = TagsChanged;
        if (tagsChanged != null) {
            tagsChanged(new TagsChanged(this, entity.Id, archetype.tags, default));
        }
        // --- component events 
        var componentAdded = ComponentAdded;
        if (componentAdded == null) {
            return;
        }
        foreach (var heap in archetype.structHeaps) {
            componentAdded(new ComponentChanged (this, entity.Id, ComponentChangedAction.Add, heap.structIndex, null));    
        }
    }
}

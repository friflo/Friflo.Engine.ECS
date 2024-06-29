// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Engine.ECS.Relations;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Return the <see cref="IComponent"/>'s added to an <see cref="Entity"/>.
/// </summary>
[DebuggerTypeProxy(typeof(EntityComponentsDebugView))]
public readonly struct EntityComponents : IEnumerable<EntityComponent>
{
    // --- internal fields
    [Browse(Never)]
    internal readonly   Entity  entity;     // 16

    /// <summary>Return the number of <see cref="IComponent"/>'s of an entity.</summary>
    public              int     Count       => GetCount();
    public   override   string  ToString()  => entity.archetype.componentTypes.GetString();
    
    private int GetCount()
    {
        var type            = entity.archetype;
        var count           = type.componentCount; 
        var relationsMap    = entity.store.relationsMap;
        foreach (var componentType in EntityStoreBase.Static.EntitySchema.relationTypes)
        {
            var relations = relationsMap[componentType.StructIndex];
            if (relations == null) continue;
            count += relations.GetRelationCount(entity);
        }
        return count;
    }
    
    internal IComponent[] GetComponentArray()
    {
        int count = Count;
        if (count == 0) {
            return Array.Empty<IComponent>();
        }
        var components = new IComponent[count];
        int n = 0;
        foreach (var component in this) {
            components[n++] = component.GetValue();
        }
        return components;
    }

    // --- IEnumerable<>
    IEnumerator<EntityComponent>   IEnumerable<EntityComponent>.GetEnumerator() => new ComponentEnumerator(this);
    
    // --- IEnumerable
    IEnumerator                                     IEnumerable.GetEnumerator() => new ComponentEnumerator(this);
    
    // --- new
    public ComponentEnumerator                                  GetEnumerator() => new ComponentEnumerator(this);

    internal EntityComponents(Entity entity) {
        this.entity          = entity;
    }
}

/// <summary>
/// Enumerate the components of an entity by iterating <see cref="EntityComponents"/>. 
/// </summary>
public struct ComponentEnumerator : IEnumerator<EntityComponent>
{
    // --- internal fields
    private  readonly   EntityComponent[]   components;
    private             int                 index;
    
    internal ComponentEnumerator(in EntityComponents entityComponents)
    {
        var entity  = entityComponents.entity;
        components  = new EntityComponent[entityComponents.Count];
        int compIndex = 0;
        foreach (var componentType in entity.archetype.componentTypes) {
            components[compIndex++] = new EntityComponent(entity, componentType);
        }
        var relationsMap = entity.store.relationsMap;
        foreach (var componentType in EntityStoreBase.Static.EntitySchema.relationTypes)
        {
            var relations = relationsMap[componentType.StructIndex];
            if (relations == null) continue;
            int relationCount =  relations.GetRelationCount(entity);
            for (int n = 0; n < relationCount; n++) {
                components[compIndex++] = new EntityComponent(entity, componentType, relations, n);
            }
        }
    }
    
    // --- IEnumerator<>
    public readonly EntityComponent Current   => components[index - 1];
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < components.Length) {
            index++;
            return true;
        }
        return false;
    }

    public void Reset() {
        index = 0;
    }
    
    object IEnumerator.Current => components[index - 1];

    // --- IDisposable
    public void Dispose() { }
}

/// <summary>An item in <see cref="EntityComponents"/> containing an entity <see cref="IComponent"/>.</summary>
public readonly struct EntityComponent
{
    // --- public fields
    [Browse(Never)] private readonly    Entity              entity;             // 16
    [Browse(Never)] private readonly    ComponentType       type;               //  8
    [Browse(Never)] private readonly    RelationsArchetype  relationsArchetype; //  8
    [Browse(Never)] private readonly    int                 relationsIndex;     //  4
    
    // --- public properties
    /// <summary>
    /// Property is mainly used to display a component value in the Debugger.<br/>
    /// It has poor performance as is boxes the returned component. 
    /// </summary>
    /// <remarks>
    /// To access a component use <see cref="Entity.GetComponent{T}"/>
    /// </remarks>
    [Obsolete($"use {nameof(Entity)}.{nameof(Entity.GetComponent)}<T>() to access a component")]
    public              IComponent      Value       => GetValue();
    
    /// <summary>Return the <see cref="System.Type"/> of an entity component.</summary>
    public              ComponentType   Type        => type;
    
    public  override    string          ToString()  => type.ToString();

    internal EntityComponent (Entity entity, ComponentType componentType) {
        this.entity = entity;
        type        = componentType;
    }
    
    internal EntityComponent (Entity entity, ComponentType componentType, RelationsArchetype relationsArchetype, int relationsIndex) {
        this.entity             = entity;
        type                    = componentType;
        this.relationsArchetype = relationsArchetype;
        this.relationsIndex     = relationsIndex;
    }
    
    internal IComponent GetValue() {
        if (relationsArchetype == null) {
            return entity.archetype.heapMap[type.StructIndex].GetComponentDebug(entity.compIndex);
        }
        return relationsArchetype.GetRelation(entity, relationsIndex);
    }
}

internal class EntityComponentsDebugView
{
    [Browse(RootHidden)]
    public              IComponent[]        Components => entityComponents.GetComponentArray();

    [Browse(Never)]
    private readonly    EntityComponents    entityComponents;
        
    internal EntityComponentsDebugView(EntityComponents entityComponents)
    {
        this.entityComponents = entityComponents;
    }
}

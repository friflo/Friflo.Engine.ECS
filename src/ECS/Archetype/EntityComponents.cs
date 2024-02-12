// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Return the <see cref="IComponent"/>'s added to an <see cref="Entity"/>.
/// </summary>
public readonly struct EntityComponents : IEnumerable<EntityComponent>
{
    // --- internal fields
    private  readonly   Entity  entity;     // 16

    /// <summary>Return the number of <see cref="IComponent"/>'s of an entity.</summary>
    public              int     Count       => entity.archetype.componentCount;
    public   override   string  ToString()  => entity.archetype.componentTypes.GetString();

    // --- IEnumerable<>
    IEnumerator<EntityComponent>   IEnumerable<EntityComponent>.GetEnumerator() => new ComponentEnumerator(entity);
    
    // --- IEnumerable
    IEnumerator                                     IEnumerable.GetEnumerator() => new ComponentEnumerator(entity);
    
    // --- new
    public ComponentEnumerator                                  GetEnumerator() => new ComponentEnumerator(entity);

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
    private  readonly   Entity                      entity;             // 16
    private             ComponentTypesEnumerator    typesEnumerator;    // 48
    
    internal ComponentEnumerator(in Entity entity) {
        this.entity     = entity;
        typesEnumerator = new ComponentTypesEnumerator (entity.archetype.componentTypes);
    }
    
    // --- IEnumerator<>
    public readonly EntityComponent Current   => new EntityComponent(entity, typesEnumerator.Current);
    
    // --- IEnumerator
    public bool MoveNext() {
        return typesEnumerator.bitSetEnumerator.MoveNext();
    }

    public void Reset() {
        typesEnumerator.bitSetEnumerator.Reset();
    }
    
    object IEnumerator.Current => new EntityComponent(entity, typesEnumerator.Current);

    // --- IDisposable
    public void Dispose() { }
}

/// <summary>An item in <see cref="EntityComponents"/> containing an entity <see cref="IComponent"/>.</summary>
public readonly struct EntityComponent
{
    // --- public fields
    [Browse(Never)] private readonly    Entity          entity;     // 16
    [Browse(Never)] private readonly    ComponentType   type;       //  8
    
    // --- public properties
    /// <summary>
    /// Property is mainly used to display a component value in the Debugger.<br/>
    /// It has poor performance as is boxes the returned component. 
    /// </summary>
    /// <remarks>
    /// To access a component use <see cref="Entity.GetComponent{T}"/>
    /// </remarks>
    [Obsolete($"use {nameof(Entity)}.{nameof(Entity.GetComponent)}<T>() to access a component")]
    public              IComponent      Value       => entity.archetype.heapMap[type.StructIndex].GetComponentDebug(entity.compIndex);
    
    /// <summary>Return the <see cref="System.Type"/> of an entity component.</summary>
    public              ComponentType   Type        => type;
    
    public  override    string          ToString()  => type.ToString();

    internal EntityComponent (Entity entity, ComponentType componentType) {
        this.entity = entity;
        type        = componentType;
    }
}

/// <summary>
/// Use only for convenience to display entity components in the debugger. 
/// </summary>
internal readonly struct Components
{
    [Browse(RootHidden)]
    internal            IComponent[]    Array => GetComponentArray();
    
    [Browse(Never)]
    private readonly    Entity          entity;

    public override     string          ToString() => $"IComponent[{entity.EntityComponents.Count}]";

    internal Components(in Entity entity) {
        this.entity = entity;
    }
    
    private IComponent[] GetComponentArray()
    {
        var componentEntities = entity.EntityComponents;
        var components = new IComponent[componentEntities.Count];
        int n = 0;
        foreach (var component in componentEntities) {
#pragma warning disable CS0618 // Type or member is obsolete
            components[n++] = component.Value;
#pragma warning restore CS0618 // Type or member is obsolete
        }
        return components;
    }
}

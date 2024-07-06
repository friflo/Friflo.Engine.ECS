// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Friflo.Engine.ECS.Relations;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseCollectionExpression
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
    private  readonly   Entity  entity;     // 16

    /// <summary>Return the number of <see cref="IComponent"/>'s of an entity.</summary>
    public              int     Count       => GetCount();
    public   override   string  ToString()  => GetString();
    
    private int GetCount()
    {
        var type        = entity.archetype;
        var count       = type.componentCount; 
        if (!GetRelationTypes(entity, out var relationTypes)) {
            return count;
        }
        var relationsMap = entity.store.extension.relationsMap;
        foreach (var componentType in relationTypes)
        {
            var relations = relationsMap[componentType.StructIndex]; // not null - ensured by GetRelationTypes()
            count        += relations.GetRelationCount(entity);
        }
        return count;
    }
    
    private EntityComponent[] CreateComponents()
    {
        if (!EntityComponents.GetRelationTypes(entity, out var relationTypes)) {
            return null;
        }
        var components  = new EntityComponent[GetCount()];
        int compIndex   = 0;
        foreach (var componentType in entity.archetype.componentTypes) {
            components[compIndex++] = new EntityComponent(entity, componentType);
        }
        var relationsMap = entity.store.extension.relationsMap;
        foreach (var componentType in relationTypes)
        {
            var relations       = relationsMap[componentType.StructIndex]; // not null - ensured by GetRelationTypes()
            int relationCount   = relations.GetRelationCount(entity);
            for (int n = 0; n < relationCount; n++) {
                components[compIndex++] = new EntityComponent(entity, componentType, relations, n);
            }
        }
        return components;
    }
    
    private static bool GetRelationTypes(Entity entity, out ComponentTypes relationTypes)
    {
        relationTypes  = default;
        var isOwner = entity.store.nodes[entity.Id].isOwner; 
        if (isOwner == 0) {
            return false;
        }
        var intersect = isOwner & EntityStoreBase.Static.EntitySchema.relationTypes.bitSet.l0;
        relationTypes.bitSet.l0 = intersect;
        return intersect != 0;
    }
    
    internal IComponent[] GetComponentArray()
    {
        int count = GetCount();
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
    
    private string GetString()
    {
        var sb          = new StringBuilder();
        var archetype   = entity.archetype;
        archetype.componentTypes.AppendTo(sb);
        var relationCount = GetCount() - archetype.componentTypes.Count;
        if (relationCount > 0) {
            sb.Append(" +");
            sb.Append(relationCount);
            sb.Append(" relations");
        }
        return sb.ToString();
    }

    // --- IEnumerable<>
    IEnumerator<EntityComponent>   IEnumerable<EntityComponent>.GetEnumerator() => new ComponentEnumerator(entity, CreateComponents());
    
    // --- IEnumerable
    IEnumerator                                     IEnumerable.GetEnumerator() => new ComponentEnumerator(entity, CreateComponents());
    
    // --- new
    public ComponentEnumerator                                  GetEnumerator() => new ComponentEnumerator(entity, CreateComponents());

    internal EntityComponents(Entity entity) {
        this.entity          = entity;
    }
}

/// <summary>
/// Enumerate the components of an entity by iterating <see cref="EntityComponents"/>. 
/// </summary>
public struct ComponentEnumerator : IEnumerator<EntityComponent>
{
#region fields
    // --- used when entity does not contain relations
    private  readonly   Entity                      entity;             // 16
    private             ComponentTypesEnumerator    typesEnumerator;    // 48
    // --- used when entity contains relations
    private  readonly   EntityComponent[]           components;         //  8
    private             int                         index;              //  4
    #endregion
    
    internal ComponentEnumerator(Entity entity, EntityComponent[] components) {
        this.entity     = entity;
        typesEnumerator = new ComponentTypesEnumerator (entity.archetype.componentTypes);
        this.components = components;
    }
    
    // --- IEnumerator<>
    public readonly EntityComponent Current   => components == null
        ? new EntityComponent(entity, typesEnumerator.Current)
        : components[index - 1];
    
    // --- IEnumerator
    object IEnumerator.Current => Current;
    
    public bool MoveNext()
    {
        if (components == null) {
            return typesEnumerator.bitSetEnumerator.MoveNext();
        }
        if (index < components.Length) {
            index++;
            return true;
        }
        return false;
    }

    public void Reset() {
        typesEnumerator.bitSetEnumerator.Reset();
        index = 0;
    }

    // --- IDisposable
    public void Dispose() { }
}

/// <summary>An item in <see cref="EntityComponents"/> containing an entity <see cref="IComponent"/>.</summary>
public readonly struct EntityComponent
{
    // --- public fields
    [Browse(Never)] private readonly    Entity          entity;             // 16
    [Browse(Never)] private readonly    ComponentType   type;               //  8
    [Browse(Never)] private readonly    EntityRelations entityRelations;    //  8
    [Browse(Never)] private readonly    int             relationsIndex;     //  4
    
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
    
    internal EntityComponent (Entity entity, ComponentType componentType, EntityRelations entityRelations, int relationsIndex) {
        this.entity             = entity;
        type                    = componentType;
        this.entityRelations    = entityRelations;
        this.relationsIndex     = relationsIndex;
    }
    
    internal IComponent GetValue() {
        if (entityRelations == null) {
            return entity.archetype.heapMap[type.StructIndex].GetComponentDebug(entity.compIndex);
        }
        return entityRelations.GetRelationAt(entity.Id, relationsIndex);
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

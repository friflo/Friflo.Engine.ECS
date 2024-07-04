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
    
    [Browse(Never)]
    internal readonly   bool    onlyReferences;

    /// <summary>Return the number of <see cref="IComponent"/>'s of an entity.</summary>
    public              int     Count       => onlyReferences ? GetReferences(null) : GetCountAll();
    public   override   string  ToString()  => GetString();
    
    private int GetCountAll()
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
    
    internal EntityComponent[] GetAllComponents()
    {
        var components  = new EntityComponent[GetCountAll()];
        int compIndex = 0;
        foreach (var componentType in entity.archetype.componentTypes) {
            components[compIndex++] = new EntityComponent(entity, componentType);
        }
        if (!EntityComponents.GetRelationTypes(entity, out var relationTypes)) {
            return components;
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
    
    internal int GetReferences(EntityComponent[] components)
    {
        var id          = entity.Id;
        var store       = entity.store;
        var isLinked    = store.nodes[id].isLinked;
        if (isLinked == 0) {
            return 0;
        }
        var indexTypes          = new ComponentTypes();
        var relationTypes       = new ComponentTypes();
        var schema              = EntityStoreBase.Static.EntitySchema;
        indexTypes.bitSet.l0    = schema.indexTypes.   bitSet.l0 & isLinked; // intersect
        relationTypes.bitSet.l0 = schema.relationTypes.bitSet.l0 & isLinked; // intersect

        var relationsMap = store.extension.relationsMap;
        if (components == null) {
            int count = indexTypes.Count;
            foreach (var componentType in relationTypes) {
                var references  = EntityRelations.GetRelationReferences(entity.store, entity.Id, componentType.StructIndex);
                count          += references.Count;
            }
            return count;
        }
        var index   = 0;
        foreach (var componentType in indexTypes) {
            components[index++] = new EntityComponent(entity, componentType);
        }
        foreach (var componentType in relationTypes) {
            var references      = EntityRelations.GetRelationReferences(entity.store, entity.Id, componentType.StructIndex);
            var relations       = relationsMap[componentType.StructIndex];
            for (int n = 0; n < references.Count; n++) {
                components[index++] = new EntityComponent(entity, componentType, relations, n);
            }
        }
        return index;
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
    
    private string GetString()
    {
        var sb          = new StringBuilder();
        var archetype   = entity.archetype;
        archetype.componentTypes.AppendTo(sb);
        var relationCount = Count - archetype.componentTypes.Count;
        if (relationCount > 0) {
            sb.Append(" +");
            sb.Append(relationCount);
            sb.Append(" relations");
        }
        return sb.ToString();
    }

    // --- IEnumerable<>
    IEnumerator<EntityComponent>   IEnumerable<EntityComponent>.GetEnumerator() => new ComponentEnumerator(this);
    
    // --- IEnumerable
    IEnumerator                                     IEnumerable.GetEnumerator() => new ComponentEnumerator(this);
    
    // --- new
    public ComponentEnumerator                                  GetEnumerator() => new ComponentEnumerator(this);

    internal EntityComponents(Entity entity, bool onlyReferences) {
        this.entity         = entity;
        this.onlyReferences = onlyReferences;
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
        if (!entityComponents.onlyReferences) {
            components = entityComponents.GetAllComponents();
            return;
        }
        var count   = entityComponents.GetReferences(null);
        if (count == 0) {
            components = Array.Empty<EntityComponent>();
        }
        components  = new EntityComponent[count];
        entityComponents.GetReferences(components);
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

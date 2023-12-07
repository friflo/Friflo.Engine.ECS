// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct EntityComponents : IEnumerable<EntityComponent>
{
    // --- internal fields
    internal readonly   Entity  entity;     // 16

    public   override   string  ToString()  => $"Count: {entity.archetype.componentCount}";

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

public struct ComponentEnumerator : IEnumerator<EntityComponent>
{
    // --- internal fields
    private             ComponentTypesEnumerator    typesEnumerator;    // 48
    private  readonly   Entity                      entity;             // 16
    
    internal ComponentEnumerator(in EntityComponents components) {
        typesEnumerator = components.entity.archetype.componentTypes.GetEnumerator();
        entity          = components.entity;
    }
    
    // --- IEnumerator<>
    public readonly EntityComponent Current   => new EntityComponent(entity, typesEnumerator.Current);
    
    // --- IEnumerator
    public bool MoveNext() {
        return typesEnumerator.MoveNext();
    }

    public void Reset() {
        typesEnumerator.Reset();
    }
    
    object IEnumerator.Current => Current;

    // --- IDisposable
    public void Dispose() { }
}


public readonly struct EntityComponent
{
    // --- public fields
    public  readonly    Entity          entity;     // 16
    public  readonly    ComponentType   type;       //  8
    
    // --- public properties
    public              object          Value       => entity.archetype.heapMap[type.structIndex].GetComponentDebug(entity.compIndex);
    
    
    public  override    string          ToString()  => type.ToString();

    internal EntityComponent (Entity entity, ComponentType componentType) {
        this.entity = entity;
        type        = componentType;
    }
}
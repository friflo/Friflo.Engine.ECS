// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

/// <summary>
/// Base class to enable implementing a custom component index.<br/>
/// A custom component index can be implemented to optimize indexing or component queries for a specific component type.   
/// </summary>
public abstract class AbstractComponentIndex
{
#region properties    
    internal  abstract  int             Count { get; }
    public    override  string          ToString() => $"{componentType.Name} - {GetType().Name} count: {Count}";
    #endregion
    
#region fields
    internal  readonly  IdArrayHeap     idHeap   = new();
    internal  readonly  EntityStore     store;
    internal  readonly  ComponentType   componentType;
    internal  readonly  int             structIndex;
    internal  readonly  int             indexBit;
    internal            bool            modified;
    #endregion
    
    internal abstract void Add   <TComponent>   (int id, in TComponent component)                              where TComponent : struct;
    internal abstract void Update<TComponent>   (int id, in TComponent component, StructHeap<TComponent> heap) where TComponent : struct;
    internal abstract void Remove<TComponent>   (int id,                          StructHeap<TComponent> heap) where TComponent : struct;
    
    /// Remove entity id from indexed component value.<br/>
    /// The component is removed by <see cref="Entity.DeleteEntity"/> shortly after.
    internal abstract void RemoveEntityFromIndex(int id, Archetype archetype, int compIndex);
    
    internal NotSupportedException NotSupportedException(string name) {
        return new NotSupportedException($"{name} not supported by {GetType().Name}");
    }
    
    internal AbstractComponentIndex(EntityStore store, ComponentType componentType) {
        this.store          = store;
        this.componentType  = componentType;
        structIndex         = componentType.StructIndex;
        var types           = new ComponentTypes(componentType);
        indexBit            = (int)types.bitSet.l0;
    }
}

/// <summary>
/// Generic base class required to implement a custom component index.
/// </summary>
public abstract class GenericComponentIndex<TValue> : AbstractComponentIndex
{
    internal            TValue[]                    sortBuffer  = Array.Empty<TValue>();
    
    internal abstract   IReadOnlyCollection<TValue> IndexedComponentValues        { get; }
    internal abstract   Entities                    GetHasValueEntities    (TValue value);
    internal virtual    void                        AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet) => throw NotSupportedException("ValueInRange()");
    
    internal GenericComponentIndex(EntityStore store, ComponentType componentType) : base(store, componentType) { }
}

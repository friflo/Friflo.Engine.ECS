// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal abstract class ComponentIndex
{
#region properties    
    internal  abstract  int             Count { get; }
    public    override  string          ToString() => $"{componentType.Name} - {GetType().Name} count: {Count}";
    #endregion
    
#region fields
    internal  readonly  IdArrayHeap     arrayHeap   = new();
    internal            EntityStore     store;          // could be made readonly
    internal            ComponentType   componentType;  // only for debugging
    internal            bool            modified;
    #endregion
    
    internal abstract void Add   <TComponent>(int id, in TComponent component)                              where TComponent : struct, IComponent;
    internal abstract void Update<TComponent>(int id, in TComponent component, StructHeap<TComponent> heap) where TComponent : struct, IComponent;
    internal abstract void Remove<TComponent>(int id,                          StructHeap<TComponent> heap) where TComponent : struct, IComponent;
    
    internal NotSupportedException NotSupportedException(string name) {
        return new NotSupportedException($"{name} not supported by {GetType().Name}");
    }
}

internal abstract class ComponentIndex<TValue> : ComponentIndex
{
    internal            TValue[]                    sortBuffer  = Array.Empty<TValue>();
    
    internal abstract   IReadOnlyCollection<TValue> IndexedComponentValues        { get; }
    internal abstract   Entities                    GetHasValueEntities    (TValue value);
    internal virtual    void                        AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet) => throw NotSupportedException("ValueInRange()");
}

// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal abstract class ComponentIndex
{
    internal abstract int Count { get; }

    internal EntityStore store; // could be made readonly
    
    internal abstract void Add   <TComponent>(int id, in TComponent component)                  where TComponent : struct, IComponent;
    internal abstract void Update<TComponent>(int id, in TComponent component, StructHeap heap) where TComponent : struct, IComponent;
    internal abstract void Remove<TComponent>(int id,                          StructHeap heap) where TComponent : struct, IComponent;
    
    internal NotSupportedException NotSupportedException(string name) {
        return new NotSupportedException($"{name} not supported by {GetType().Name}");
    }
}

internal abstract class ComponentIndex<TValue> : ComponentIndex
{
    internal abstract   Entities    GetHasValueEntities    (TValue value);
    internal virtual    void        AddValueInRangeEntities(TValue min, TValue max, HashSet<int> idSet) => throw NotSupportedException("ValueInRange()");
}

[AttributeUsage(AttributeTargets.Struct)]
internal sealed class ComponentIndexAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local
    public ComponentIndexAttribute(Type type) { }
    
    internal static Type GetComponentIndex(Type type)
    {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(ComponentIndexAttribute)) {
                continue;
            }
            var arg = attr.ConstructorArguments;
            return (Type) arg[0].Value;
        }
        return null;
    }
}

internal struct StoreIndex
{
    /// <summary> component index created on demand. </summary>
    private             ComponentIndex          index;
    internal readonly   IndexedComponentType    type;

    public override string ToString() {
        if (type.componentType == null) {
            return null;
        }
        var name = type.componentType.Name;
        if (index == null) {
            return name;
        }
        return $"{name} - {index.GetType().Name} count: {index.Count}";
    }

    internal StoreIndex(IndexedComponentType type) {
        this.type   = type;
    }
    
    internal ComponentIndex GetIndex(EntityStore store) {
        return index ??= type.CreateComponentIndex(store);
    }
}
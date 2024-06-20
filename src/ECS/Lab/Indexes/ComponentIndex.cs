// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal abstract class ComponentIndex
{
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
    internal abstract   Entities    GetMatchingEntities    (TValue value);
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
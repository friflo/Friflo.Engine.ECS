// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class ComponentIndex
{
    internal EntityStore store; // could be made readonly
    
    internal abstract void Add   <TComponent>(int id, in TComponent component)          where TComponent : struct, IComponent;
    internal abstract void Remove<TComponent>(int id, StructHeap heap, int compIndex)   where TComponent : struct, IComponent;
}

internal abstract class ComponentIndex<TValue> : ComponentIndex
{
    internal abstract void AddMatchingEntities(in TValue value, HashSet<int> set);
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class InvertedIndex {
    internal abstract void Add<TComponent>(in TComponent component, int id);
}

// internal sealed class InvertedIndex<TComponent, TValue> : InverseIndex where TComponent : struct, IIndexedComponent<TValue> 
internal sealed class InvertedIndex<TValue>  : InvertedIndex
{
    internal readonly    Dictionary<TValue, int[]>   map = new ();
    
    internal override void Add<TComponent>(in TComponent component, int id)
    {
        var indexedComponent = (IIndexedComponent<TValue>)component;    // TODO avoid boxing 
        map.Add(indexedComponent.Value, new int[] { id });              // TODO fix array creation
    }
}
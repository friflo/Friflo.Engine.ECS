// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class InvertedIndex
{
    internal abstract void Add<TComponent>(in TComponent component, int id) where TComponent : struct, IComponent;
}

internal sealed class InvertedIndex<TValue>  : InvertedIndex
{
    internal readonly    Dictionary<TValue, int[]>   map = new ();
    
    internal override void Add<TComponent>(in TComponent component, int id)
    {
        var value = IndexedComponentUtils<TComponent,TValue>.GetIndexedValue(component);
        // var indexedComponent    = (IIndexedComponent<TValue>)component; // boxing implementation of IIndexedComponent<>.GetValue()
        // var value               = indexedComponent.GetValue();
        if (!map.TryGetValue(value, out var ids)) {
            map.Add(value, new int[] { id });                           // TODO avoid array creation
            return;
        }
        if (Array.IndexOf(ids, id) != -1) {
            return;
        }
        var newIds = new int[ids.Length + 1];                           // TODO avoid array creation
        ids.CopyTo(newIds, 0);
        newIds[ids.Length]  = id;
        map[value]          = newIds;
    }
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal struct MapItem<TValue> : IEquatable<MapItem<TValue>>
{
    internal            TValue  key;
    internal            IdArray ids;

    public   override   string  ToString() => $"{key} - {ids}";

    internal MapItem(TValue key) {
        this.key    = key;
    }
        
    internal MapItem(TValue key, IdArray ids) {
        this.key    = key;
        this.ids   = ids;
    }

    public bool Equals(MapItem<TValue> other) {
        return EqualityComparer<TValue>.Default.Equals(key , other.key);
    }
}

internal sealed class MapItemComparer<TValue> : IComparer<MapItem<TValue>>
{
    public int Compare(MapItem<TValue> x, MapItem<TValue> y) {
        return Comparer<TValue>.Default.Compare(x.key , y.key);
    }
}
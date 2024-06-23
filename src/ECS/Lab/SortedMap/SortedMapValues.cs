// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class SortedMapValues<TValue> : IReadOnlyCollection<TValue>
{
    public              int                         Count => set.Count;

    internal readonly   SortedSet<MapItem<TValue>>  set;
    
    internal SortedMapValues (SortedSet<MapItem<TValue>> set) {
        this.set = set;
    }

    public IEnumerator<TValue> GetEnumerator() => new SortedSetValuesEnumerator<TValue>(this);
    IEnumerator    IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

internal sealed class SortedSetValuesEnumerator<TValue> : IEnumerator<TValue>
{
    private SortedSet<MapItem<TValue>>.Enumerator   enumerator;
    
    internal SortedSetValuesEnumerator(SortedMapValues<TValue> values) {
        enumerator  = values.set.GetEnumerator();
    }

    // --- IDisposable
    public  void    Dispose()           => enumerator.Dispose();

    // --- IEnumerator
    public  bool    MoveNext()          => enumerator.MoveNext();
    public  void    Reset()             => throw new NotImplementedException();
            object  IEnumerator.Current => throw new NotImplementedException();

    // --- IEnumerator<>
    public  TValue  Current             => enumerator.Current.key;
}

// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;


internal sealed class SortedValuesKeys<TValue> : IReadOnlyCollection<TValue>
{
    public              int                     Count => sortedValues.Count;
    
    internal readonly   SortedValues<TValue>    sortedValues;

    
    internal SortedValuesKeys (SortedValues<TValue> sortedValues) {
        this.sortedValues = sortedValues;
    }

    public IEnumerator<TValue> GetEnumerator() => new SortedValuesKeysEnumerator<TValue>(this);
    IEnumerator    IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

internal sealed class SortedValuesKeysEnumerator<TValue> : IEnumerator<TValue>
{
    private readonly    TValue[]    values;
    private readonly    int         last;
    private             int         index;
    
    internal SortedValuesKeysEnumerator(SortedValuesKeys<TValue> keys) {
        values  = keys.sortedValues.values;
        last    = keys.Count - 1;
        index   = -1;
    }

    // --- IDisposable
    public void Dispose() { }

    // --- IEnumerator
    public bool MoveNext() {
        if (index < last) {
            index++;
            return true;
        }
        return false;
    }

    public  void    Reset() {
        index = -1;
    }
    
    object  IEnumerator.Current => values[index];

    // --- IEnumerator<>
    public  TValue  Current     => values[index];
}



// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable UseCollectionExpression
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class SortedValues<TValue>
{
#region properties
    internal    int                     Count       => count;
    internal    ReadOnlySpan<TValue>    KeySpan     => new (values,   0, count);
    internal    ReadOnlySpan<IdArray>   ValueSpan   => new (idArrays, 0, count);
    #endregion
    
#region fields
    internal    TValue[]    values;
    private     IdArray[]   idArrays;
    private     int         count;
    #endregion
    
    internal SortedValues()
    {
        values      = Array.Empty<TValue>();
        idArrays    = Array.Empty<IdArray>();
    }
    
#region Dictionary<>
    internal void TryGetValue(TValue value, out IdArray array)
    {
        var index = Array.BinarySearch(values, 0, count, value, Comparer);
        if (index >= 0) {
            array = idArrays[index];
            return;
        }
        array = default;
    }
    
    internal IdArray this[TValue key]
    {
        get {
            var index = Array.BinarySearch(values, 0, count, key, Comparer);
            return idArrays[index];
        }
        set {
            var index = Array.BinarySearch(values, 0, count, key, Comparer);
            if (index >= 0) {
                idArrays[index] = value;
                return;
            }
            if (count == values.Length) {
                Resize();
            }
            index               = -1 - index;
            var len             = count++ - index;
            var valuesSource    = new ReadOnlySpan<TValue> (values,   index,     len);
            var valuesTarget    = new Span<TValue>         (values,   index + 1, len);
            valuesSource.CopyTo(valuesTarget);
            values[index]   = key;
            
            var arraysSource    = new ReadOnlySpan<IdArray>(idArrays, index,      len);
            var arraysTarget    = new Span<IdArray>        (idArrays, index + 1,  len);
            arraysSource.CopyTo(arraysTarget);
            idArrays[index] = value;
        }
    }
    
    private void Resize()
    {
        var newLength = Math.Max(4, 2 * count);
        ArrayUtils.Resize(ref values,   newLength);
        ArrayUtils.Resize(ref idArrays, newLength);
    }
    
    internal bool Remove(TValue value)
    {
        var index = Array.BinarySearch(values, 0, count, value, Comparer);
        if (index >= 0) {
            var len             = --count - index;
            var valuesSource    = new ReadOnlySpan<TValue> (values,   index + 1, len);
            var valuesTarget    = new Span<TValue>         (values,   index,     len);
            valuesSource.CopyTo(valuesTarget);
            var arraysSource    = new ReadOnlySpan<IdArray>(idArrays, index + 1, len);
            var arraysTarget    = new Span<IdArray>        (idArrays, index,     len);
            arraysSource.CopyTo(arraysTarget);
            return true;
        }
        return false;
    }
    #endregion

#region lower / upper bounds

    private static readonly Comparer<TValue> Comparer = Comparer<TValue>.Default; 
    
    // https://stackoverflow.com/questions/23806296/what-is-the-fastest-way-to-get-all-the-keys-between-2-keys-in-a-sortedlist
    internal int LowerBound(TValue value)
    {
        int lower = 0, upper = count - 1;
        var localValues = values;

        while (lower <= upper)
        {
            int middle = lower + (upper - lower) / 2;
            int comparisonResult = Comparer.Compare(value, localValues[middle]);

            // slightly adapted here
            if (comparisonResult <= 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }
        return lower;
    }
    
    internal int UpperBound(TValue value)
    {
        int lower = 0, upper = count - 1;
        var localValues = values;

        while (lower <= upper)
        {
            int middle = lower + (upper - lower) / 2;
            int comparisonResult = Comparer.Compare(value, localValues[middle]);

            // slightly adapted here
            if (comparisonResult < 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }
        return lower;
    }
    #endregion
}


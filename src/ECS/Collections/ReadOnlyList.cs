// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Represents a strongly typed readonly list of objects that can be accessed by index.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerTypeProxy(typeof(ReadOnlyListDebugView<>))]
public struct ReadOnlyList<T> : IReadOnlyList<T> where T : class
{
#region public properties
    /// <summary> Returns the number of elements contained in the list. </summary>
    public          int             Count           => count;
    
    /// <summary> Returns an <see cref="ReadOnlySpan{T}"/> of the list elements. </summary>
    public          ReadOnlySpan<T> Span            => new (array, 0, count);
    
    public override string          ToString()      => $"{typeof(T).Name}[{count}]";
    
    /// <summary> Gets the element at the specified index. </summary>
    // No set by intention. public interface is read only
    public          T               this[int index] => array[index];
    #endregion
    
#region public methods
    /// <summary>
    /// Returns the zero-based index of the first occurrence of a value within the entire list.
    /// </summary>
    public int IndexOf(T element)
    {
        var local = array;
        for (int index = 0; index < count; index++) {
            if (local[index] == element) return index;
        }
        return -1;
    }
    #endregion

#region private fields
    internal T[] array;     //  8
    internal int count;     //  4
    #endregion
    
#region internal mutations
    // internal by intention. public interface is read only
    internal ReadOnlyList(T[] array) {
        count       = 0;
        this.array  = array;
    }
    
    // internal by intention. public interface is read only
    internal void Clear() {
        for (int i = 0; i < count; i++) {
            array[i] = null;
        }
        count = 0;
    }
    
    // internal by intention. public interface is read only
    internal void Add(T item)
    {
        if (count == array.Length) { 
            Resize(ref array, Math.Max(4, 2 * count));
        }
        array[count++] = item;
    }
    
    // internal by intention. public interface is read only
    internal void Insert(int index, T item)
    {
        if (count == array.Length) { 
            Resize(ref array, Math.Max(4, 2 * count));
        }
        for (int n = count; n > index; n--) {
            array[n] = array[n - 1];    
        }
        array[index] = item;
        count++;
    }
    
    // internal by intention. public interface is read only
    internal int Remove(T item)
    {
        var arr = array;
        for (int n = 0; n < count; n++) {
            if (!ReferenceEquals(item, arr[n])) {
                continue;
            }
            count--;
            for (int i = n; i < count; i++) {
                arr[i] = arr[i + 1];   
            }
            array[count] = null;
            return n;
        }
        return -1;
    }
    
    internal void RemoveAt(int index)
    {
        var arr = array;
        count--;
        for (int i = index; i < count; i++) {
            arr[i] = arr[i + 1];   
        }
        array[count] = null;
    }
    #endregion
    
#region IEnumerator
    /// <summary>
    /// Returns an enumerator that iterates through the list.
    /// </summary>
    public      ReadOnlyListEnumerator<T> GetEnumerator() => new ReadOnlyListEnumerator<T>(this);

    // --- IEnumerable
    IEnumerator        IEnumerable.GetEnumerator() => new ReadOnlyListEnumerator<T>(this);

    // --- IEnumerable<>
    IEnumerator<T>  IEnumerable<T>.GetEnumerator() => new ReadOnlyListEnumerator<T>(this);
    #endregion
    
    private static void Resize(ref T[] array, int len)
    {
        var newArray    = new T[len];
        var curLength   = array.Length;
        var source      = new ReadOnlySpan<T>(array, 0, curLength);
        var target      = new Span<T>(newArray,      0, curLength);
        source.CopyTo(target);
        array           = newArray;
    }
}

/// <summary>
/// Enumerates the elements of a <see cref="ReadOnlyList{T}"/>.
/// </summary>
public struct ReadOnlyListEnumerator<T> : IEnumerator<T> where T : class
{
#region private fields
    private readonly    T[]     array;  //  8
    private readonly    int     count;  //  4
    private             int     index;  //  4
    #endregion

    internal ReadOnlyListEnumerator(ReadOnlyList<T> list) {
        array  = list.array;
        count  = list.count - 1;
        index       = -1;
    }

#region IEnumerator
    // --- IEnumerator
    /// <summary> Sets the enumerator to its initial position, which is before the first element in the list. </summary>
    public          void         Reset()    => index = -1;

    readonly object  IEnumerator.Current    => array[index];

    /// <summary> Gets the element at the current position of the enumerator. </summary>
    public   T                   Current    => array[index];

    // --- IEnumerator
    /// <summary> Advances the enumerator to the next element of the collection. </summary>
    public bool MoveNext()
    {
        if (index < count) {
            index++;
            return true;
        }
        return false;
    }

    /// <summary> Releases all resources used by the list enumerator. </summary>
    public void Dispose() { }
    #endregion
}

internal class ReadOnlyListDebugView<T> where T : class
{
    [Browse(RootHidden)]
    public              T[]             Items => GetItems();

    [Browse(Never)]
    private readonly    ReadOnlyList<T> readOnlyList;
        
    internal ReadOnlyListDebugView(ReadOnlyList<T> readOnlyList)
    {
        this.readOnlyList = readOnlyList;
    }
    
    private T[] GetItems()
    {
        var count       = readOnlyList.count;
        var result      = new T[count];
        Span<T> source  = new (readOnlyList.array, 0, count);
        Span<T> target  = result;
        source.CopyTo(target);
        return result;
    }
}

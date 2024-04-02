// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Op   = System.Collections.Specialized.NotifyCollectionChangedAction;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

namespace Friflo.Editor.UI.Explorer.Lab;

/// <summary>Implement interfaces of <see cref="ObservableCollection{T}"/></summary>
// ObservableCollection : Collection<T>,                            INotifyCollectionChanged, INotifyPropertyChanged
// Collection<T>               : IList<T>, IList, IReadOnlyList<T>
public class ObservableList<T> : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged //, INotifyPropertyChanged
{
    private             NotifyCollectionChangedEventHandler collectionChanged;
    
    public  event       NotifyCollectionChangedEventHandler CollectionChanged {
        add     => collectionChanged += value;
        remove  => collectionChanged -= value;
    }
//  public  event       PropertyChangedEventHandler         PropertyChanged;
    private readonly    List<T>                             collection;
    
    // ReSharper disable once ConvertConstructorToMemberInitializers
    public ObservableList() {
        collection = new List<T>();
    }
    
    // --- public methods
    public void Add(T item) {
        collection.Add(item);
        var index   = collection.Count - 1;
        OnCollectionChanged(Op.Add, item, index);
    }
    
    // --- private methods
    private void OnCollectionChanged(Op action, object item, int index) {
        var args = new NotifyCollectionChangedEventArgs(action, item, index);
        collectionChanged?.Invoke(this, args);
    }
    
    private void OnCollectionChanged(Args args) {
        collectionChanged?.Invoke(this, args);
    }
    
    // --- private implementations
    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return collection.GetEnumerator();
    }

    void ICollection<T>.Add(T item) {
        collection.Add(item);
        var index   = collection.Count - 1;
        OnCollectionChanged(Op.Add, item, index);
    }

    void ICollection<T>.Clear() {
        collection.Clear();
        OnCollectionChanged(Op.Reset, null, -1);
    }

    bool ICollection<T>.Contains(T item) {
        return collection.Contains(item);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
        collection.CopyTo(array, arrayIndex);
        throw new NotImplementedException();
    }

    bool ICollection<T>.Remove(T item) {
        throw new NotImplementedException();
    }

    int ICollection<T>.Count => collection.Count;

    bool ICollection<T>.IsReadOnly => false;

    int IList<T>.IndexOf(T item) {
        return collection.IndexOf(item);
    }

    void IList<T>.Insert(int index, T item) {
        collection.Insert(index, item);
        OnCollectionChanged(Op.Add, item, index);
    }

    /// <summary>
    /// <see cref="ObservableCollection{T}.RemoveAt"/>
    /// </summary>
    /// <param name="index"></param>
    void IList<T>.RemoveAt(int index) {
        var item = collection[index];
        collection.RemoveAt(index);
        OnCollectionChanged(Op.Remove, item, index);
    }

    T IList<T>.this[int index] {
        get => collection[index];
        set {
            var oldItem         = collection[index];
            collection[index]   = value;
            var args            = new Args(Op.Replace, value, oldItem, index);
            OnCollectionChanged(args);
        }
    }
    
    T IReadOnlyList<T>.this[int index] => collection[index];

    int IReadOnlyCollection<T>.Count => collection.Count;
    
    // --------------------------------------- IList crab :) ---------------------------------------
    void IList.Clear()  {
        collection.Clear();
        OnCollectionChanged(Op.Reset, null, -1);
    }
    void IList.RemoveAt(int index) {
        var item = collection[index];
        collection.RemoveAt(index);
        OnCollectionChanged(Op.Remove, item, index);
    }
    
    int IList.Add(object value) {
        collection.Add((T)value);
        var index   = collection.Count - 1;
        OnCollectionChanged(Op.Add, value, index);
        return index;
    }

    object IList.this[int index] {
        get => collection[index];
        set {
            var oldItem         = collection[index];
            collection[index]   = (T)value;
            var args = new Args(Op.Replace, value, oldItem, index);
            OnCollectionChanged(args);
        }
    }
    
    bool IList.Contains(object value) {
        return collection.Contains((T)value);
    }

    int IList.IndexOf(object value) {
        return collection.IndexOf((T)value);
    }

    void IList.Insert(int index, object value) {
        collection.Insert(index, (T)value);
        OnCollectionChanged(Op.Add, value, index);
    }

    void IList.Remove(object value) {
        collection.Remove((T)value);
        throw new NotImplementedException();
    }
    
    void ICollection.CopyTo(Array array, int index) {
        throw new NotImplementedException();
    }
    
    bool    IList.IsFixedSize           => false;
    bool    IList.IsReadOnly            => false;    
    int     ICollection.Count           => collection.Count;
    bool    ICollection.IsSynchronized  => false;
    object  ICollection.SyncRoot        => collection;
}



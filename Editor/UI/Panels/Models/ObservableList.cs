using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Op   = System.Collections.Specialized.NotifyCollectionChangedAction;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Models;


/// <summary>Implement interfaces of <see cref="ObservableCollection{T}"/></summary>
// ObservableCollection : Collection<T>,                            INotifyCollectionChanged, INotifyPropertyChanged
// Collection<T>               : IList<T>, IList, IReadOnlyList<T>
public class ObservableList<T> : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    public  event       NotifyCollectionChangedEventHandler CollectionChanged;
    public  event       PropertyChangedEventHandler         PropertyChanged;
    private readonly    Collection<T>                       collection;
    
    public ObservableList() {
        collection = new Collection<T>();
        CollectionChanged += (sender, args) => {
            _ = 1;
        };
        PropertyChanged += (sender, args) => {
            _ = 2;  
        };
    }

    public IEnumerator<T> GetEnumerator() {
        return collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(T item) {
        
        collection.Add(item);
        var index   = collection.Count - 1;
        var args    = new Args(Op.Add, index);
        CollectionChanged?.Invoke(this, args);
    }

    public int Add(object value) {
        collection.Add((T)value);
        var index   = collection.Count - 1;
        var args    = new Args(Op.Add, index);
        CollectionChanged?.Invoke(this, args);
        return index;
    }

    void IList.Clear()  {
        collection.Clear();
        var args = new Args(Op.Reset);
        CollectionChanged?.Invoke(this, args);
    }

    public bool Contains(object value) {
        return collection.Contains((T)value);
    }

    public int IndexOf(object value) {
        return collection.IndexOf((T)value);
    }

    public void Insert(int index, object value) {
        collection.Insert(index, (T)value);
        var args    = new Args(Op.Add, index);
        CollectionChanged?.Invoke(this, args);
    }

    public void Remove(object value) {
        collection.Remove((T)value);
        throw new NotImplementedException();
    }

    void IList.RemoveAt(int index) {
        collection.RemoveAt(index);
        var args    = new Args(Op.Remove, index);
        CollectionChanged?.Invoke(this, args);
    }

    public bool IsFixedSize => false;

    bool IList.IsReadOnly => false;

    object IList.this[int index] {
        get => collection[index];
        set => collection[index] = (T)value;
    }

    void ICollection<T>.Clear() {
        collection.Clear();
        var args = new Args(Op.Reset);
        CollectionChanged?.Invoke(this, args);
    }

    public bool Contains(T item) {
        return collection.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        collection.CopyTo(array, arrayIndex);
        throw new NotImplementedException();
    }

    public bool Remove(T item) {
        throw new NotImplementedException();
        return collection.Remove(item);
    }

    public void CopyTo(Array array, int index) {
        throw new NotImplementedException();
    }

    int ICollection.Count => collection.Count;

    public bool IsSynchronized  => false;
    public object SyncRoot      => collection;

    int ICollection<T>.Count => collection.Count;

    bool ICollection<T>.IsReadOnly => false;

    public int IndexOf(T item) {
        return collection.IndexOf(item);
    }

    public void Insert(int index, T item) {
        collection.Insert(index, item);
        var args    = new Args(Op.Add, index);
        CollectionChanged?.Invoke(this, args);
    }

    void IList<T>.RemoveAt(int index) {
        collection.RemoveAt(index);
        var args    = new Args(Op.Remove, index);
        CollectionChanged?.Invoke(this, args);
    }

    public T this[int index] {
        get => collection[index];
        set => collection[index] = value;
    }

    int IReadOnlyCollection<T>.Count => collection.Count;
}
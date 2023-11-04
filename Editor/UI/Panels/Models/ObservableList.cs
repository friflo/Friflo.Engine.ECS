using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Op   = System.Collections.Specialized.NotifyCollectionChangedAction;

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
    static void Test() {
        var col = new ObservableCollection<object>();
        col.Insert(1, null);
    }
    
    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index) {
        var args = new NotifyCollectionChangedEventArgs(action, item, index);
        CollectionChanged?.Invoke(this, args);
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
        OnCollectionChanged(Op.Add, item, index);
    }

    public int Add(object value) {
        collection.Add((T)value);
        var index   = collection.Count - 1;
        OnCollectionChanged(Op.Add, value, index);
        return index;
    }

    void IList.Clear()  {
        collection.Clear();
        OnCollectionChanged(Op.Reset, null, 0);
    }

    public bool Contains(object value) {
        return collection.Contains((T)value);
    }

    public int IndexOf(object value) {
        return collection.IndexOf((T)value);
    }

    public void Insert(int index, object value) {
        collection.Insert(index, (T)value);
        OnCollectionChanged(Op.Add, value, index);
    }

    public void Remove(object value) {
        collection.Remove((T)value);
        throw new NotImplementedException();
    }

    void IList.RemoveAt(int index) {
        var item = collection[index];
        collection.RemoveAt(index);
        OnCollectionChanged(Op.Remove, item, index);
    }

    public bool IsFixedSize => false;

    bool IList.IsReadOnly => false;

    object IList.this[int index] {
        get => collection[index];
        set => collection[index] = (T)value;
    }

    void ICollection<T>.Clear() {
        collection.Clear();
        OnCollectionChanged(Op.Reset, null, 0);
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

    public T this[int index] {
        get => collection[index];
        set => collection[index] = value;
    }

    int IReadOnlyCollection<T>.Count => collection.Count;
}
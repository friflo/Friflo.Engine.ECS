// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Op   = System.Collections.Specialized.NotifyCollectionChangedAction;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class GameEntity: IList<GameEntity>, IList, IReadOnlyList<GameEntity>, INotifyCollectionChanged, INotifyPropertyChanged
{
    public  event       NotifyCollectionChangedEventHandler CollectionChanged;
    public  event       PropertyChangedEventHandler         PropertyChanged;
    private readonly    List<GameEntity>                    collection;
    
    // ReSharper disable once ConvertConstructorToMemberInitializers
    
    // --- public methods
    public void Add(GameEntity item) {
        collection.Add(item);
        var index   = collection.Count - 1;
        OnCollectionChanged(Op.Add, item, index);
    }
    
    // --- private methods
    IEnumerator<GameEntity> IEnumerable<GameEntity>.GetEnumerator() {
        return collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return collection.GetEnumerator();
    }

    void ICollection<GameEntity>.Add(GameEntity item) {
        collection.Add(item);
        var index   = collection.Count - 1;
        OnCollectionChanged(Op.Add, item, index);
    }

    void ICollection<GameEntity>.Clear() {
        collection.Clear();
        OnCollectionChanged(Op.Reset, null, -1);
    }

    bool ICollection<GameEntity>.Contains(GameEntity item) {
        return collection.Contains(item);
    }

    void ICollection<GameEntity>.CopyTo(GameEntity[] array, int arrayIndex) {
        collection.CopyTo(array, arrayIndex);
        throw new NotImplementedException();
    }

    bool ICollection<GameEntity>.Remove(GameEntity item) {
        throw new NotImplementedException();
    }

    int ICollection<GameEntity>.Count => collection.Count;

    bool ICollection<GameEntity>.IsReadOnly => false;

    int IList<GameEntity>.IndexOf(GameEntity item) {
        return collection.IndexOf(item);
    }

    void IList<GameEntity>.Insert(int index, GameEntity item) {
        collection.Insert(index, item);
        OnCollectionChanged(Op.Add, item, index);
    }

    void IList<GameEntity>.RemoveAt(int index) {
        var item = collection[index];
        collection.RemoveAt(index);
        OnCollectionChanged(Op.Remove, item, index);
    }

    GameEntity IList<GameEntity>.this[int index] {
        get => collection[index];
        set {
            var oldItem         = collection[index];
            collection[index]   = value;
            var args            = new Args(Op.Replace, value, oldItem, index);
            OnCollectionChanged(args);
        }
    }
    
    GameEntity IReadOnlyList<GameEntity>.this[int index] => collection[index];

    int IReadOnlyCollection<GameEntity>.Count => collection.Count;
    
    // --- private methods
    private void OnCollectionChanged(Op action, object item, int index) {
        var args = new NotifyCollectionChangedEventArgs(action, item, index);
        CollectionChanged?.Invoke(this, args);
    }
    
    private void OnCollectionChanged(Args args) {
        CollectionChanged?.Invoke(this, args);
    }
    
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
        collection.Add((GameEntity)value);
        var index   = collection.Count - 1;
        OnCollectionChanged(Op.Add, value, index);
        return index;
    }

    object IList.this[int index] {
        get => collection[index];
        set {
            var oldItem         = collection[index];
            collection[index]   = (GameEntity)value;
            var args = new Args(Op.Replace, value, oldItem, index);
            OnCollectionChanged(args);
        }
    }
    
    bool IList.Contains(object value) {
        return collection.Contains((GameEntity)value);
    }

    int IList.IndexOf(object value) {
        return collection.IndexOf((GameEntity)value);
    }

    void IList.Insert(int index, object value) {
        collection.Insert(index, (GameEntity)value);
        OnCollectionChanged(Op.Add, value, index);
    }

    void IList.Remove(object value) {
        collection.Remove((GameEntity)value);
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

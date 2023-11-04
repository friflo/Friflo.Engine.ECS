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
#region event handler
    public  event       NotifyCollectionChangedEventHandler CollectionChanged;
    public  event       PropertyChangedEventHandler         PropertyChanged;
    #endregion
    private readonly    List<GameEntity>                    collection; // todo remove
    
#region private methods
    private void OnCollectionChanged(Op action, object item, int index) {
        var args = new NotifyCollectionChangedEventArgs(action, item, index);
        CollectionChanged?.Invoke(this, args);
    }
    
    private void OnCollectionChanged(Args args) {
        CollectionChanged?.Invoke(this, args);
    }
    
    private GameEntity GetChildByIndex(int index) {
        var childIds = archetype.gameEntityStore.GetNodeById(id).childIds;
        return archetype.gameEntityStore.GetNodeById(childIds[index]).entity;
    }
    #endregion
    
#region IEnumerable<>
    IEnumerator<GameEntity> IEnumerable<GameEntity>.GetEnumerator() {
        return ChildNodes.GetChildEntityEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ChildNodes.GetChildEntityEnumerator();
    }
    #endregion

#region ICollection<>
    void ICollection<GameEntity>.Add(GameEntity item) {
        AddChild(item);
        OnCollectionChanged(Op.Add, item, ChildCount - 1);
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

    int ICollection<GameEntity>.Count => ChildCount;

    bool ICollection<GameEntity>.IsReadOnly => false;
    #endregion

#region IList<>
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
        get => GetChildByIndex(index);
        set {
            var oldItem         = collection[index];
            collection[index]   = value;
            var args            = new Args(Op.Replace, value, oldItem, index);
            OnCollectionChanged(args);
        }
    }

    #endregion
    
#region IReadOnlyCollection<>
    GameEntity  IReadOnlyList<GameEntity>.this[int index]   => GetChildByIndex(index);
    int         IReadOnlyCollection<GameEntity>.Count       => ChildCount;
    #endregion
    
    // --------------------------------------- crab interface :) ---------------------------------------
#region IList
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
        AddChild((GameEntity)value);
        var index   = ChildCount - 1;
        OnCollectionChanged(Op.Add, value, index);
        return index;
    }

    object IList.this[int index] {
        get => GetChildByIndex(index);
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
    
    bool    IList.IsFixedSize           => false;
    bool    IList.IsReadOnly            => false;
    #endregion
    
#region ICollection
    int     ICollection.Count           => ChildCount;
    bool    ICollection.IsSynchronized  => false;
    object  ICollection.SyncRoot        => this;
    
    void ICollection.CopyTo(Array array, int index) {
        throw new NotImplementedException();
    }
    #endregion
}

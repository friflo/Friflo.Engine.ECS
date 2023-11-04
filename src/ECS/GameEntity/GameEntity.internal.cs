// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Op   = System.Collections.Specialized.NotifyCollectionChangedAction;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// Implements same interfaces as System.Collections.ObjectModel.ObservableCollection{T} to enable
// using GameEntity's in UI controls typically using an ObservableCollection{T}.
public sealed partial class GameEntity :
    IList<GameEntity>,
    IList,
    IReadOnlyList<GameEntity>,
    INotifyCollectionChanged
 // INotifyPropertyChanged
{
 // public  event       PropertyChangedEventHandler PropertyChanged; not required
    
    // ReSharper disable once InconsistentNaming
    private             List<GameEntity>            collection => null; // todo remove
    
#region private methods
    private void OnCollectionChanged(Op action, object entity, int index) {
        var args = new NotifyCollectionChangedEventArgs(action, entity, index);
        collectionChanged?.Invoke(this, args);
    }
    
    private void OnCollectionChanged(Args args) {
        collectionChanged?.Invoke(this, args);
    }
    
    private GameEntity GetChildByIndex(int index) {
        var childIds = archetype.gameEntityStore.GetNodeById(id).childIds;
        return archetype.gameEntityStore.GetNodeById(childIds[index]).entity;
    }
    
    private void ClearChildEntities() {
        throw new NotImplementedException();
        OnCollectionChanged(Op.Reset, null, -1);
    }
    
    private void RemoveChildEntityAt(int index) {
        var entity = collection[index];
        collection.RemoveAt(index);
        OnCollectionChanged(Op.Remove, entity, index);
    }
    
    private void InsertChildEntityAt(int index, GameEntity entity) {
        collection.Insert(index, entity);
        OnCollectionChanged(Op.Add, entity, index);
    }
    
    private void ReplaceChildEntityAt(int index, GameEntity entity) {
        var oldItem         = collection[index];
        collection[index]   = entity;
        var args            = new Args(Op.Replace, entity, oldItem, index);
        OnCollectionChanged(args);
    }
    
    private int GetChildIndex(GameEntity entity) {
        var store       = archetype.gameEntityStore;
        var childIds    = store.GetNodeById(id).childIds;
        var count       = ChildCount;
        var searchId    = entity.id;
        for (int n = 0; n < count; n++) {
            if (searchId != childIds[n]) {
                continue;
            }
            return n;
        }
        return -1;
    }
    #endregion
    
// -------------------------------------- interface implementations --------------------------------------
#region INotifyCollectionChanged
    event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
    {
        add     => collectionChanged += value;
        remove  => collectionChanged -= value;
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
    void ICollection<GameEntity>.Add(GameEntity entity) {
        AddChild(entity);
        OnCollectionChanged(Op.Add, entity, ChildCount - 1);
    }

    void ICollection<GameEntity>.Clear() {
        ClearChildEntities();
    }

    bool ICollection<GameEntity>.Contains(GameEntity entity) {
        return GetChildIndex(entity) != - 1;
    }

    void ICollection<GameEntity>.CopyTo(GameEntity[] array, int arrayIndex) {
        // collection.CopyTo(array, arrayIndex);
        throw new NotImplementedException();
    }

    bool ICollection<GameEntity>.Remove(GameEntity entity) {
        int index = GetChildIndex(entity);
        if (index == -1) {
            return false;
        }
        RemoveChildEntityAt(index);
        return true;
    }

    int ICollection<GameEntity>.Count => ChildCount;

    bool ICollection<GameEntity>.IsReadOnly => false;
    #endregion

#region IList<>
    int IList<GameEntity>.IndexOf(GameEntity entity) {
        return GetChildIndex(entity);
    }

    void IList<GameEntity>.Insert(int index, GameEntity entity) {
        InsertChildEntityAt(index, entity);
    }

    void IList<GameEntity>.RemoveAt(int index) {
        RemoveChildEntityAt(index);
    }

    GameEntity IList<GameEntity>.this[int index] {
        get => GetChildByIndex(index);
        set => ReplaceChildEntityAt(index, value);
    }

    #endregion
    
#region IReadOnlyCollection<>
    GameEntity  IReadOnlyList<GameEntity>.this[int index]   => GetChildByIndex(index);
    int         IReadOnlyCollection<GameEntity>.Count       => ChildCount;
    #endregion
    
// ---------------------------------- crab interface implementations :) ----------------------------------
#region IList
    void IList.Clear()  {
        ClearChildEntities();
    }
    
    void IList.RemoveAt(int index) {
        RemoveChildEntityAt(index);
    }
    
    int IList.Add(object value) {
        AddChild((GameEntity)value);
        var index   = ChildCount - 1;
        OnCollectionChanged(Op.Add, value, index);
        return index;
    }

    object IList.this[int index] {
        get => GetChildByIndex(index);
        set => ReplaceChildEntityAt(index, (GameEntity)value);
    }

    bool IList.Contains(object value) {
        return GetChildIndex((GameEntity)value) != -1;
    }

    int IList.IndexOf(object value) {
        return GetChildIndex((GameEntity)value);
    }

    void IList.Insert(int index, object entity) {
        InsertChildEntityAt(index, (GameEntity)entity);
    }

    void IList.Remove(object value) {
        int index = GetChildIndex((GameEntity)value);
        RemoveChildEntityAt(index);
    }
    
    bool    IList.IsFixedSize           => false;
    bool    IList.IsReadOnly            => false;
    #endregion
    
#region ICollection
    int     ICollection.Count           => ChildCount;
    bool    ICollection.IsSynchronized  => false;
    object  ICollection.SyncRoot        => this;
    
    void    ICollection.CopyTo(Array array, int index) {
        throw new NotImplementedException();
    }
    #endregion
}

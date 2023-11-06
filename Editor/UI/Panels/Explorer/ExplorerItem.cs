// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Friflo.Fliox.Engine.ECS;
using Op   = System.Collections.Specialized.NotifyCollectionChangedAction;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Explorer;

// Implements same interfaces as System.Collections.ObjectModel.ObservableCollection{T} to enable
// using ExplorerItem's in UI controls typically using an ObservableCollection{T}.
public sealed class ExplorerItem :
    IList<ExplorerItem>,
    IList,
    IReadOnlyList<ExplorerItem>,
    INotifyCollectionChanged
 // INotifyPropertyChanged                                              not required. Implemented by ObservableCollection{T}
{
#region internal fields
    private  readonly   int                         id;
    internal readonly   GameEntity                  entity;
    internal readonly   ExplorerTree                tree;
     
    private    NotifyCollectionChangedEventHandler  collectionChanged;
 // public  event       PropertyChangedEventHandler PropertyChanged;    not required. Implemented by ObservableCollection{T}
    // ReSharper disable once InconsistentNaming
    private             List<ExplorerItem>          collection => null; // todo remove
    private             int                         ChildCount => entity.ChildCount;
    #endregion

#region constructor
    internal ExplorerItem (ExplorerTree tree, GameEntity entity) {
        this.tree   = tree;
        this.entity = entity;
        id          = entity.Id;
    }
    #endregion
    
#region private methods
    private void OnCollectionChanged(Op action, object entity, int index) {
        if (collectionChanged == null) {
            return;
        }
        var args = new NotifyCollectionChangedEventArgs(action, entity, index);
        collectionChanged.Invoke(this, args);
    }
    
    private ExplorerItem GetChildByIndex(int index) {
        int childId = entity.GetChildByIndex(index);
        return tree.items[childId];
    }
    
    private void ClearChildEntities() {
        throw new NotImplementedException();
        OnCollectionChanged(Op.Reset, null, -1);
    }
    
    private void RemoveChildEntityAt(int index) {
        var childEntity = collection[index];
        collection.RemoveAt(index);
        OnCollectionChanged(Op.Remove, childEntity, index);
    }
    
    private void AddChildEntity(ExplorerItem entity) {
        var index = ChildCount;
        collection.Insert(index, entity);
        OnCollectionChanged(Op.Add, entity, index);
    }
    
    private void InsertChildEntityAt(int index, ExplorerItem entity) {
        collection.Insert(index, entity);
        OnCollectionChanged(Op.Add, entity, index);
    }
    
    private void ReplaceChildEntityAt(int index, ExplorerItem entity) {
        if (collectionChanged == null) {
            collection[index] = entity;
            return;
        }
        var oldItem         = collection[index];
        collection[index]   = entity;
        var args            = new Args(Op.Replace, entity, oldItem, index);
        collectionChanged.Invoke(this, args);
    }
    
    private int GetChildIndex(ExplorerItem item) {
        return entity.GetChildIndex(item.entity.Id);
    }
    #endregion
    
// -------------------------------------- interface implementations --------------------------------------
#region object
    public override int GetHashCode() {
        return id;
    }

    public override bool Equals(object obj) {
        return ReferenceEquals(this, obj);
    }
    #endregion

#region INotifyCollectionChanged
    event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
    {
        add     => collectionChanged += value;
        remove  => collectionChanged -= value;
    }
    #endregion
    
#region IEnumerable<>
    IEnumerator<ExplorerItem> IEnumerable<ExplorerItem>.GetEnumerator() {
        return new ExplorerItemEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return new ExplorerItemEnumerator(this);
    }
    #endregion

#region ICollection<>
    void ICollection<ExplorerItem>.Add(ExplorerItem entity) {
        AddChildEntity(entity);
    }

    void ICollection<ExplorerItem>.Clear() {
        ClearChildEntities();
    }

    bool ICollection<ExplorerItem>.Contains(ExplorerItem entity) {
        return GetChildIndex(entity) != - 1;
    }

    void ICollection<ExplorerItem>.CopyTo(ExplorerItem[] array, int arrayIndex) {
        // collection.CopyTo(array, arrayIndex);
        throw new NotImplementedException();
    }

    bool ICollection<ExplorerItem>.Remove(ExplorerItem entity) {
        int index = GetChildIndex(entity);
        if (index == -1) {
            return false;
        }
        RemoveChildEntityAt(index);
        return true;
    }

    int ICollection<ExplorerItem>.Count => ChildCount;

    bool ICollection<ExplorerItem>.IsReadOnly => false;
    #endregion

#region IList<>
    int IList<ExplorerItem>.IndexOf(ExplorerItem entity) {
        return GetChildIndex(entity);
    }

    void IList<ExplorerItem>.Insert(int index, ExplorerItem entity) {
        InsertChildEntityAt(index, entity);
    }

    void IList<ExplorerItem>.RemoveAt(int index) {
        RemoveChildEntityAt(index);
    }

    ExplorerItem IList<ExplorerItem>.this[int index] {
        get => GetChildByIndex(index);
        set => ReplaceChildEntityAt(index, value);
    }

    #endregion
    
#region IReadOnlyCollection<>
    ExplorerItem  IReadOnlyList<ExplorerItem>.this[int index]   => GetChildByIndex(index);
    int         IReadOnlyCollection<ExplorerItem>.Count       => ChildCount;
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
        AddChildEntity((ExplorerItem)value);
        var index   = ChildCount - 1;
        OnCollectionChanged(Op.Add, value, index);
        return index;
    }

    object IList.this[int index] {
        get => GetChildByIndex(index);
        set => ReplaceChildEntityAt(index, (ExplorerItem)value);
    }

    bool IList.Contains(object value) {
        return GetChildIndex((ExplorerItem)value) != -1;
    }

    int IList.IndexOf(object value) {
        return GetChildIndex((ExplorerItem)value);
    }

    void IList.Insert(int index, object entity) {
        InsertChildEntityAt(index, (ExplorerItem)entity);
    }

    void IList.Remove(object value) {
        int index = GetChildIndex((ExplorerItem)value);
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

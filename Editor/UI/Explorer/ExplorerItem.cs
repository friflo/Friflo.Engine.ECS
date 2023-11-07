// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Friflo.Fliox.Engine.ECS;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable PossibleNullReferenceException
namespace Friflo.Fliox.Editor.UI.Explorer;

/// <summary>
/// Implements same interfaces as <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> to act as a replacement
/// for <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> with generic type <see cref="ExplorerItem"/>.
/// </summary>
public sealed class ExplorerItem :
    IList<ExplorerItem>,
    IList,
    IReadOnlyList<ExplorerItem>,
    INotifyCollectionChanged
 // INotifyPropertyChanged                                                      not required. Implemented by ObservableCollection{T}
{
#region internal properties
    internal            string                              Name                => entity.Name.value;
    public              bool                                flag;
    private             int                                 ChildCount          => entity.ChildCount;
    internal            NotifyCollectionChangedEventHandler CollectionChanged   => collectionChanged;

    public   override   string                              ToString()          => entity.ToString();

    #endregion
    
#region internal fields
    internal readonly   GameEntity                          entity;
    internal readonly   ExplorerTree                        tree;
    private             NotifyCollectionChangedEventHandler collectionChanged;
 // public  event       PropertyChangedEventHandler         PropertyChanged;    not required. Implemented by ObservableCollection{T}
    #endregion

#region constructor
    private ExplorerItem (ExplorerTree tree, GameEntity entity) {
        this.tree   = tree;
        this.entity = entity;
    }
    #endregion
    
    internal static ExplorerItem CreateExplorerItems(ExplorerTree tree, GameEntity entity)
    {
        var item = new ExplorerItem(tree, entity);
        tree.items.Add(entity.Id, item);
        
        IList<ExplorerItem> list    = item;
        var childNodes              = entity.ChildNodes;
        foreach (var node in childNodes) {
            var childItem = CreateExplorerItems(tree, node.Entity);
            list.Add(childItem);
        }
        return item;
    }
    
#region private methods
    private ExplorerItem GetChildByIndex(int index) {
        int childId = entity.GetChildNodeByIndex(index).Id;
        return tree.items[childId];
    }
    
    private void ClearChildEntities() {
        throw new NotImplementedException();
    }
    
    private void RemoveChildEntityAt(int index) {
        var childId = entity.GetChildIndex(index);
        var child   = entity.Store.GetNodeById(childId).Entity;
        entity.RemoveChild(child);
    }
    
    private void ReplaceChildEntityAt(int index, ExplorerItem item) {
        throw new NotImplementedException();
    }
    
    private int GetChildIndex(ExplorerItem item) {
        return entity.GetChildIndex(item.entity.Id);
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
    IEnumerator<ExplorerItem> IEnumerable<ExplorerItem>.GetEnumerator() {
        return new ExplorerItemEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return new ExplorerItemEnumerator(this);
    }
    #endregion

#region ICollection<>
    void ICollection<ExplorerItem>.Add(ExplorerItem item) {
        entity.AddChild(item.entity);
    }

    void ICollection<ExplorerItem>.Clear() {
        ClearChildEntities();
    }

    bool ICollection<ExplorerItem>.Contains(ExplorerItem item) {
        return GetChildIndex(item) != - 1;
    }

    void ICollection<ExplorerItem>.CopyTo(ExplorerItem[] array, int arrayIndex) {
        // collection.CopyTo(array, arrayIndex);
        throw new NotImplementedException();
    }

    bool ICollection<ExplorerItem>.Remove(ExplorerItem item) {
        return entity.RemoveChild(item.entity);
    }

    int ICollection<ExplorerItem>.Count => ChildCount;

    bool ICollection<ExplorerItem>.IsReadOnly => false;
    #endregion

#region IList<>
    int IList<ExplorerItem>.IndexOf(ExplorerItem item) {
        return GetChildIndex(item);
    }

    void IList<ExplorerItem>.Insert(int index, ExplorerItem item) {
        entity.InsertChild(index, item.entity);
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
        var childEntity = ((ExplorerItem)value).entity;
        return entity.AddChild(childEntity);
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

    void IList.Insert(int index, object item) {
        var childEntity = ((ExplorerItem)item).entity; 
        childEntity.InsertChild(index, childEntity);
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

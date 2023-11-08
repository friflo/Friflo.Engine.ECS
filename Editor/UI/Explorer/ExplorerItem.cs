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
    internal            int                                 Id                  => entity.Id;
    public              bool                                flag;
    public              GameEntity                          Entity              => entity;
    internal            NotifyCollectionChangedEventHandler CollectionChanged   => collectionChanged;
    internal            bool                                AllowDrag           => !IsRootItem();
    internal            bool                                IsRoot              => IsRootItem();

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

    private bool IsRootItem() {
        return tree.store.StoreRoot == entity;
    }
    
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
        var child = entity.GetChildNodeByIndex(index).Entity;   // called by TreeDataGrid 
        entity.RemoveChild(child);  // todo add GameEntity.RemoveChild(int index)
    }
    
    private void ReplaceChildEntityAt(int _, ExplorerItem __) {
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
        entity.AddChild(item.entity);                           // called by TreeDataGrid
    }

    void ICollection<ExplorerItem>.Clear() {
        ClearChildEntities();
    }

    bool ICollection<ExplorerItem>.Contains(ExplorerItem item) {
        return GetChildIndex(item) != - 1;
    }

    void ICollection<ExplorerItem>.CopyTo(ExplorerItem[] array, int arrayIndex) {
        var childIds = entity.ChildIds;
        for (int n = 0; n < childIds.Length; n++)
        {
            int id                  = childIds[n];
            array[n + arrayIndex]   = tree.items[id];            
        }
    }

    bool ICollection<ExplorerItem>.Remove(ExplorerItem item) {
        return entity.RemoveChild(item.entity);
    }

    int ICollection<ExplorerItem>.Count => entity.ChildCount;   // called by TreeDataGrid

    bool ICollection<ExplorerItem>.IsReadOnly => false;
    #endregion

#region IList<>
    int IList<ExplorerItem>.IndexOf(ExplorerItem item) {
        return GetChildIndex(item);
    }

    void IList<ExplorerItem>.Insert(int index, ExplorerItem item) {
        entity.InsertChild(index, item.entity);                 // called by TreeDataGrid (DRAG)
    }

    void IList<ExplorerItem>.RemoveAt(int index) {
        RemoveChildEntityAt(index);                             // called by TreeDataGrid (DRAG)
    }

    ExplorerItem IList<ExplorerItem>.this[int index] {
        get => GetChildByIndex(index);                          // called by TreeDataGrid
        set => ReplaceChildEntityAt(index, value);
    }

    #endregion
    
#region IReadOnlyCollection<>
    ExplorerItem    IReadOnlyList<ExplorerItem>.this[int index]   => GetChildByIndex(index);    // called by TreeDataGrid
    int             IReadOnlyCollection<ExplorerItem>.Count       => entity.ChildCount;         // called by TreeDataGrid
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
        get => GetChildByIndex(index);                          // called by TreeDataGrid
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
    int     ICollection.Count           => entity.ChildCount;   // called by TreeDataGrid
    bool    ICollection.IsSynchronized  => false;
    object  ICollection.SyncRoot        => this;
    
    void    ICollection.CopyTo(Array array, int index)
    {
        var childIds = entity.ChildIds;
        for (int n = 0; n < childIds.Length; n++)
        {
            int id      = childIds[n];
            var item    = tree.items[id];
            array.SetValue(item, n + index);
        }
    }
    #endregion
}

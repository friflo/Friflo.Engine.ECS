// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable PossibleNullReferenceException
namespace Friflo.Engine.ECS.Collections;

/// <summary>
/// Implements same interfaces as <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> to act as a replacement
/// for this container class using the generic type <see cref="ExplorerItem"/>.<br/>
/// <br/>
/// This enables displaying and mutation of <see cref="Entity"/>'s in a
/// <a href="https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid">Avalonia.Controls.TreeDataGrid</a>.<br/>
/// A specialized implementation of this control is the <b>ExplorerTreeDataGrid</b> in this repository.
/// </summary>
/// <remarks>
/// It seems a common and reasonable approach that other UI frameworks like <b>MAUI</b> or <b>UNO Platform</b> provide<br/>
/// TreeView controls by using TreeView items implementing <see cref="INotifyCollectionChanged"/> and
/// optionally <see cref="INotifyPropertyChanged"/>.<br/>
/// <br/>
/// Major advantages of this approach.
/// <list type="bullet">
///   <item>
///     Support millions of items within a TreeView hierarchy without any UI stuttering.
///   </item>
///   <item>
///     Enabling binding hierarchical data to a UI without the need of 3rd party libraries in the data layer.<br/>
///     <see cref="INotifyCollectionChanged"/> and optionally <see cref="INotifyPropertyChanged"/> of the BCL are sufficient.
///   </item>
/// </list>
/// </remarks>
[DebuggerTypeProxy(typeof(ExplorerItemDebugView))]
public sealed class ExplorerItem :
    IList<ExplorerItem>,
    IList,
    IReadOnlyList<ExplorerItem>,
    INotifyCollectionChanged,
    INotifyPropertyChanged      //  only required to notify EntityName changes to Avalonia > TreeDataGrid
{
#region internal properties
    public              int     Id              => entity.Id;
    public              Entity  Entity          => entity;
    public              bool    IsRoot          => IsRootItem();
    public              bool    AllowDrag       => !IsRootItem();
    public              string  Name            { get => GetName();   set => SetName    (value); }
    public              bool    IsExpanded      { get => isExpanded;  set => SetExpanded(value);         }
    public              string  DebugTreeName   => tree.debugName;
    
    public              bool    flag;           // todo remove
    
    public   override   string  ToString()      => GetString();
    #endregion
    
#region internal fields
    private             bool                                isExpanded;
    internal readonly   Entity                              entity;                 // 16   - the corresponding entity
    internal readonly   ExplorerItemTree                    tree;                   //  8   - the ExplorerItemTree containing this ExplorerItem
    internal            NotifyCollectionChangedEventHandler collectionChanged;      //  8   - event handlers are called in case entity children are modified
    public              PropertyChangedEventHandler         propertyChangedHandler; //  8   - used to notify EntityName changes to Avalonia > TreeDataGrid 
    #endregion

#region constructor
    internal ExplorerItem (ExplorerItemTree tree, Entity entity) {
        this.tree   = tree      ?? throw new ArgumentNullException(nameof(tree));
        if (entity.IsNull)         throw new ArgumentNullException(nameof(entity));
        this.entity = entity;
    }
    #endregion

#region private methods
    private bool IsRootItem() {
        return tree.rootItem.entity == entity;
    }
    
    private string GetName() {
        if (entity.HasName) {
            return entity.Name.value;
        }
        return tree.defaultEntityName;
    }
    
    private void SetName(string value)
    {
        entity.TryGetComponent<EntityName>(out var name);
        if (name.value == value) {
            return;
        }
        if (string.IsNullOrEmpty(value) || value == tree.defaultEntityName) {
            entity.RemoveComponent<EntityName>();
            return;
        }
        entity.AddComponent(new EntityName(value));
    }
    
    private void SetExpanded(bool value) {
        if (isExpanded == value) {
            return;
        }
        isExpanded = value;
        var args = new PropertyChangedEventArgs(nameof(IsExpanded));
        propertyChangedHandler?.Invoke(this, args);
    }
    
    private ExplorerItem GetChildByIndex(int index) {
        int childId = entity.ChildEntities[index].Id;
        // Console.WriteLine($"GetChildByIndex {entity.Id} {index} - child {childId}");
        return tree.GetItemById(childId);
    }
    
    [ExcludeFromCodeCoverage]
    private void ClearChildEntities() {
        throw new NotImplementedException();
    }
    
    private void RemoveChildEntityAt(int index) {
        var child = entity.ChildEntities[index];   // called by TreeDataGrid 
        entity.RemoveChild(child);  // todo add Entity.RemoveChild(int index)
    }
    
    // ReSharper disable twice UnusedParameter.Local
    [ExcludeFromCodeCoverage]
    private void ReplaceChildEntityAt(int index, ExplorerItem item) {
        throw new NotImplementedException();
    }
    
    private int GetChildIndex(ExplorerItem item) {
        return entity.GetChildIndex(item.entity);
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        EntityUtils.EntityToString(entity.Id, entity.archetype, sb);
        var childCount = entity.ChildCount; 
        if (childCount > 0) {
            sb.Append("   children: ");
            sb.Append(childCount);
        }
        return sb.ToString();
    }
    #endregion
    
// -------------------------------------- interface implementations --------------------------------------
#region INotifyCollectionChanged
    public event NotifyCollectionChangedEventHandler CollectionChanged
    {
        add     => collectionChanged += value;
        remove  => collectionChanged -= value;
    }
    #endregion
    
#region INotifyPropertyChanged
    public event    PropertyChangedEventHandler PropertyChanged { add => propertyChangedHandler     += value;   remove => propertyChangedHandler    -= value; }
    #endregion
    
#region IEnumerable<>
    public IEnumerator<ExplorerItem> GetEnumerator() {
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

    [ExcludeFromCodeCoverage]
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
            array[n + arrayIndex]   = tree.GetItemById(id);            
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
        [ExcludeFromCodeCoverage]
        set => ReplaceChildEntityAt(index, value);
    }

    #endregion
    
#region IReadOnlyCollection<>
    ExplorerItem    IReadOnlyList<ExplorerItem>.this[int index]   => GetChildByIndex(index);    // called by TreeDataGrid
    int             IReadOnlyCollection<ExplorerItem>.Count       => entity.ChildCount;         // called by TreeDataGrid
    #endregion
    
// ---------------------------------- crab interface implementations :) ----------------------------------
#region IList
    [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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
        entity.InsertChild(index, childEntity);
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
    object  ICollection.SyncRoot        => null!;
    
    void    ICollection.CopyTo(Array array, int index)
    {
        var childIds = entity.ChildIds;
        for (int n = 0; n < childIds.Length; n++)
        {
            int id      = childIds[n];
            var item    = tree.GetItemById(id);
            array.SetValue(item, n + index);
        }
    }
    #endregion
}

// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

// ReSharper disable InlineTemporaryVariable
namespace Friflo.Fliox.Engine.ECS.Collections;

public class ExplorerTree
{
#region internal properties
    public              ExplorerItem                    RootItem    => rootItem;
    public   override   string                          ToString()  => debugName;
    #endregion
    
#region internal fields
    private  readonly   GameEntityStore                 store;
    internal readonly   ExplorerItem                    rootItem;
    private  readonly   Dictionary<int, ExplorerItem>   items;
    private  readonly   string                          debugName;
    #endregion
    
#region public methods
    public ExplorerTree (GameEntity rootEntity, string debugName)
    {
        this.debugName              = debugName ?? "ExplorerTree";
        store                       = rootEntity.Store;
        items                       = new Dictionary<int, ExplorerItem>();
        store.ChildNodesChanged    += ChildNodesChangedHandler;
        rootItem                    = CreateExplorerItem(rootEntity);
    }
    
    /// <summary>Get <see cref="ExplorerItem"/> by id</summary>
    /// <remarks>
    /// <c>Avalonia.Controls.TreeDataGridItemsSourceView"</c> create items on demand => create <see cref="ExplorerItem"/> if not present. 
    /// </remarks>
    public ExplorerItem GetItemById(int id)
    {
        if (!items.TryGetValue(id, out var explorerItem)) {
            var entity      = store.GetNodeById(id).Entity;
            explorerItem    = CreateExplorerItem(entity);
        }
        return explorerItem;
    }
    #endregion
    
#region internal methods
    private ExplorerItem CreateExplorerItem(GameEntity entity)
    {
        var item = new ExplorerItem(this, entity);
        items.Add(entity.Id, item);
        return item;
    }
    
    private void ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs args)
    {
        var treeItems = items;
        if (!treeItems.TryGetValue(args.parentId, out var parent)) {
            return;
        }
        // Console.WriteLine($"ExplorerTree event: {args}       parent: {parent}");
        var collectionChanged = parent.collectionChanged;
        if (collectionChanged == null) {
            return;
        }
        ExplorerItem explorerItem;
        switch (args.action) {
            case ChildNodesChangedAction.Add:
                if (!treeItems.TryGetValue(args.childId, out explorerItem)) {
                    var entity      = store.GetNodeById(args.childId).Entity;
                    explorerItem    = CreateExplorerItem(entity);
                }
                break;
            case ChildNodesChangedAction.Remove:
                if (!treeItems.TryGetValue(args.childId, out explorerItem)) {
                    return;
                }
                // Note: Don't remove from treeItems to preserve UI ExplorerItem state. E.g. the state of a checkbox. 
                // treeItems.Remove(args.childId);
                // explorerItem.collectionChanged = null;
                break;
            default:
                throw InvalidActionException(args);
        }
        object child        = explorerItem ?? throw new NullReferenceException("explorerItem");
        var action          = (NotifyCollectionChangedAction)args.action;
        
        var collectionArgs  = new NotifyCollectionChangedEventArgs(action, child, args.childIndex);
        // NOTE:
        // Passing parent as NotifyCollectionChangedEventHandler.sender enables the Avalonia UI event handlers called
        // below to check if the change event (Add / Remove) is caused by the containing Control or other code.  
        collectionChanged(parent, collectionArgs);
    }
    
    private static Exception InvalidActionException(in ChildNodesChangedArgs args) {
        
        return new InvalidOperationException($"unexpected action: {args.action}");
    }
    
    #endregion
}
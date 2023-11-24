// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

// ReSharper disable InlineTemporaryVariable
namespace Friflo.Fliox.Engine.ECS.Collections;

public class ExplorerItemTree
{
#region internal properties
    public              ExplorerItem                    RootItem    => rootItem;
    public   override   string                          ToString()  => debugName;
    #endregion
    
#region internal fields
    private  readonly   EntityStore                     store;      //  8   - the corresponding EntityStore
    internal readonly   ExplorerItem                    rootItem;   //  8   - the root ExplorerItem of the tree
    private  readonly   Dictionary<int, ExplorerItem>   items;      //  8   - map of ExplorerItem's created on demand for corresponding entities
    internal readonly   string                          debugName;  //  8   - name to identify an ExplorerItemTree instance when debugging 
    #endregion
    
#region public methods
    public ExplorerItemTree (Entity rootEntity, string debugName)
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
    private ExplorerItem CreateExplorerItem(Entity entity)
    {
        var item = new ExplorerItem(this, entity);
        items.Add(entity.Id, item);
        return item;
    }
    
    /// <summary>
    /// Fires a <see cref="INotifyCollectionChanged.CollectionChanged"/> event based on the
    /// given <see cref="ChildNodesChangedArgs"/>.
    /// </summary>
    // only internal because of unit test
    internal void ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs args)
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
        // explorerItem never null - see above
        var action      = (NotifyCollectionChangedAction)args.action;
        var eventArgs   = new NotifyCollectionChangedEventArgs(action, (object)explorerItem, args.childIndex);
        // NOTE:
        // Passing parent as NotifyCollectionChangedEventHandler.sender enables the Avalonia UI event handlers called
        // below to check if the change event (Add / Remove) is caused by the containing Control or other code.  
        collectionChanged(parent, eventArgs);
    }
    
    private static Exception InvalidActionException(in ChildNodesChangedArgs args) {
        
        return new InvalidOperationException($"unexpected action: {args.action}");
    }
    
    #endregion
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.UI.Explorer;

public class ExplorerTree
{
    private  readonly   GameEntityStore                 store;
    internal readonly   ExplorerItem                    rootItem;
    private  readonly   Dictionary<int, ExplorerItem>   items; // todo make private
    
    public ExplorerTree (GameEntity rootEntity)
    {
        store                       = rootEntity.Store;
        items                       = new Dictionary<int, ExplorerItem>();
        store.ChildNodesChanged    += ChildNodesChangedHandler;
        rootItem                    = CreateExplorerItem(rootEntity);
    }
    
    private ExplorerItem CreateExplorerItem(GameEntity entity)
    {
        var item = new ExplorerItem(this, entity);
        items.Add(entity.Id, item);
        return item;
    }
    
    internal static GameEntityStore CreateDefaultStore()
    {
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("root - default"));
        store.SetStoreRoot(root);
        return store;
    }
    
    private void ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs args)
    {
        var parent              = GetItemById(args.parentId);
        // Console.WriteLine($"ExplorerTree event: {args}       parent: {parent}");
        var collectionChanged   = parent.CollectionChanged;
        if (collectionChanged == null) {
            return;
        }
        var action          = (NotifyCollectionChangedAction)args.action;
        if (!items.TryGetValue(args.childId, out var explorerItem))
        {
            switch (args.action) {
                case ChildNodesChangedAction.Add:
                    var entity = store.GetNodeById(args.childId).Entity;
                    explorerItem = new ExplorerItem(this, entity);
                    items.Add(args.childId, explorerItem);
                    break;
                case ChildNodesChangedAction.Remove:
                    Console.WriteLine("ChildNodesChangedHandler - ChildNodesChangedAction.Remove");
                    if (items.TryGetValue(args.childId, out var removedItem)) {
                        items.Remove(args.childId);
                        removedItem.ClearCollectionChanged();
                    }
                    break;
            }
        }
        object child        = explorerItem;
        if (child == null) {
            throw new NullReferenceException("explorerItem");
        }        
        var collectionArgs  = new NotifyCollectionChangedEventArgs(action, child, args.childIndex);
        // NOTE:
        // Passing parent as NotifyCollectionChangedEventHandler.sender enables the Avalonia UI event handlers called
        // below to check if the change event (Add / Remove) is caused by the containing Control or other code.  
        collectionChanged(parent, collectionArgs);
    }
    
    /// <summary>Get <see cref="ExplorerItem"/> by id</summary>
    /// <remarks>
    /// <see cref="TreeDataGridItemsSourceView"/> create items on demand => create <see cref="ExplorerItem"/> if not present. 
    /// </remarks>
    internal ExplorerItem GetItemById(int id)
    {
        if (!items.TryGetValue(id, out var explorerItem)) {
            var entity = store.GetNodeById(id).Entity;
            explorerItem = new ExplorerItem(this, entity);
            items.Add(id, explorerItem);
        }
        return explorerItem;
    }
    
    internal static readonly GameEntityStore TestStore = CreateTestStore(); 
    
    private static GameEntityStore CreateTestStore()
    {
        var store   = new GameEntityStore();
        var root =    CreateEntity(store, 1, "root");
        root.AddChild(CreateEntity(store, 2, "child 2"));
        root.AddChild(CreateEntity(store, 3, "child 3"));
        root.AddChild(CreateEntity(store, 4, "child 4"));
        root.AddChild(CreateEntity(store, 5, "child 5"));
        root.AddChild(CreateEntity(store, 6, "child 6"));
        store.SetStoreRoot(root);
        return store;
    }
    
    private static GameEntity CreateEntity(GameEntityStore store, int id, string name)
    {
        var entity = store.CreateEntity(id);
        entity.AddComponent(new EntityName(name));
        return entity;
    }
}
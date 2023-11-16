using System;
using System.Collections.Generic;
using System.Collections.Specialized;

// ReSharper disable InlineTemporaryVariable
namespace Friflo.Fliox.Engine.ECS.Collections;

public class ExplorerTree
{
    public              ExplorerItem                    RootItem => rootItem;
    
    private  readonly   GameEntityStore                 store;
    internal readonly   ExplorerItem                    rootItem;
    private  readonly   Dictionary<int, ExplorerItem>   items;
    private  readonly   string                          debugName;
    
    public   override   string                          ToString() => debugName;

    private static      int                             _treeCount;
    
    public ExplorerTree (GameEntity rootEntity)
    {
        debugName                   = $"tree-{_treeCount++}";
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
    
    private void ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs args)
    {
        var treeItems = items;
        if (!treeItems.TryGetValue(args.parentId, out var parent)) {
            return;
        }
        // Console.WriteLine($"ExplorerTree event: {args}       parent: {parent}");
        var collectionChanged = parent.CollectionChanged;
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
                // explorerItem.ClearCollectionChanged();
                break;
            default:
                throw new InvalidOperationException($"unexpected case: {args.action}");
        }
        object child        = explorerItem ?? throw new NullReferenceException("explorerItem");
        var action          = (NotifyCollectionChangedAction)args.action;
        
        var collectionArgs  = new NotifyCollectionChangedEventArgs(action, child, args.childIndex);
        // NOTE:
        // Passing parent as NotifyCollectionChangedEventHandler.sender enables the Avalonia UI event handlers called
        // below to check if the change event (Add / Remove) is caused by the containing Control or other code.  
        collectionChanged(parent, collectionArgs);
    }
    
    /// <summary>Get <see cref="ExplorerItem"/> by id</summary>
    /// <remarks>
    /// <c>Avalonia.Controls.TreeDataGridItemsSourceView"</c> create items on demand => create <see cref="ExplorerItem"/> if not present. 
    /// </remarks>
    internal ExplorerItem GetItemById(int id)
    {
        if (!items.TryGetValue(id, out var explorerItem)) {
            var entity      = store.GetNodeById(id).Entity;
            explorerItem    = CreateExplorerItem(entity);
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
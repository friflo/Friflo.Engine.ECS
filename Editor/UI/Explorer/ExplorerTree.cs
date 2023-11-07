using System.Collections.Generic;
using System.Collections.Specialized;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.UI.Explorer;

public class ExplorerTree
{
    internal readonly   Dictionary<int, ExplorerItem>   items;
    
    public ExplorerTree (GameEntityStore store)
    {
        items                    = new Dictionary<int, ExplorerItem>();
        store.ChildNodesChanged += ChildNodesChangedHandler;
    }
    
    private void ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs args)
    {
        var parent              = items[args.parentId];
        var collectionChanged   = parent.CollectionChanged;
        if (collectionChanged == null) {
            return;
        }
        var action          = (NotifyCollectionChangedAction)args.action;
        object child        = items[args.childId];
        var collectionArgs  = new NotifyCollectionChangedEventArgs(action, child, args.childIndex);
        collectionChanged(sender, collectionArgs);
    }
    
    internal ExplorerItem CreateExplorerItems(GameEntityStore store) {
        var root = store.StoreRoot;
        return CreateExplorerItems(root);
    }
    
    private ExplorerItem CreateExplorerItems(GameEntity entity)
    {
        var item = new ExplorerItem(this, entity);    
        foreach (var node in entity.ChildNodes) {
            CreateExplorerItems(node.Entity);
        }
        return item;
    }
    
    internal static readonly GameEntityStore TestStore = CreateTestStore(); 
    
    private static GameEntityStore CreateTestStore()
    {
        var store   = new GameEntityStore();
        var root = store.CreateEntity();
        root.AddChild(store.CreateEntity());
        root.AddChild(store.CreateEntity());
        root.AddChild(store.CreateEntity());
        root.AddChild(store.CreateEntity());
        store.SetStoreRoot(root);
        
        return store;
    }
}
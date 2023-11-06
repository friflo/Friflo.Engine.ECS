using System.Collections.Generic;
using System.Collections.Specialized;
using Friflo.Fliox.Engine.ECS;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Explorer;

public class ExplorerTree
{
    internal readonly Dictionary<int, ExplorerItem> items;
    
    public ExplorerTree (GameEntityStore store)
    {
        items = new Dictionary<int, ExplorerItem>();
        store.CollectionChanged += ChildNodesChangedHandler;
    }
    
    private void ChildNodesChangedHandler(object sender, in NotifyChildNodesChangedEventArgs args)
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
}
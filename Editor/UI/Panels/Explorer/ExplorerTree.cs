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
        var action          = (NotifyCollectionChangedAction)args.action;
        var item            = items[args.entityId];
        var collectionArgs  = new NotifyCollectionChangedEventArgs(action, (object)item, args.index);
        item.collectionChanged(sender, collectionArgs);
    }
}
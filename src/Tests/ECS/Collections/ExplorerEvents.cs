using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Friflo.Engine.ECS.Collections;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Tests.ECS.Collections {

internal class ExplorerEvents
{
    internal int seq;
    
    internal static ExplorerEvents AddHandler(ExplorerItem item, Action<NotifyCollectionChangedEventArgs> action)
    {
        var events = new ExplorerEvents();
        // item.ChildEntitiesChangedHandler = (object _, in ChildEntitiesChangedArgs args) => {
        item.CollectionChanged += (_, args) => {
            events.seq++;
            action(args);
        };
        return events;
    }
    
    internal static ExplorerEvents AddHandlerSeq(ExplorerItem item, Action<NotifyCollectionChangedEventArgs, int> action)
    {
        var events = new ExplorerEvents();
        // store.ChildEntitiesChangedHandler = (object _, in ChildEntitiesChangedArgs args) => {
        item.CollectionChanged += (_, args) => {
            action(args, events.seq++);
        };
        return events;
    }
    
        
    [Test]
    public static void Test_ObservableCollection_Reference()
    {
        var col = new ObservableCollection<int>();
        col.CollectionChanged += (sender, args) => {
            AreSame(col, sender);
            switch (args.Action) {
                case NotifyCollectionChangedAction.Add:
                    AreEqual(1, col.Count);
                    AreEqual(0, args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    AreEqual(0, col.Count);
                    AreEqual(0, args.OldStartingIndex);
                    break;
            }
        }; 
        col.Add(1);
        col.RemoveAt(0);
    }
}

}
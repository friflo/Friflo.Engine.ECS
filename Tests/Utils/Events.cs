using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Tests.Utils;

internal class ChildEntitiesChangedEvents {
    internal    int                                     Seq => seq;
    private     int                                     seq;
    private     EventHandler<ChildEntitiesChangedArgs>  handler;
    private     EntityStore                             store;
    
    internal static ChildEntitiesChangedEvents AddHandler(EntityStore store, Action<ChildEntitiesChangedArgs> action)
    {
        var events = new ChildEntitiesChangedEvents();
        events.store = store;
        store.OnChildEntitiesChanged += events.handler = (_, args) => {
            events.seq++;
            action(args);
        };
        return events;
    }
    
    internal static ChildEntitiesChangedEvents SetHandlerSeq(EntityStore store, Action<ChildEntitiesChangedArgs, int> action)
    {
        var events = new ChildEntitiesChangedEvents();
        store.OnChildEntitiesChanged += events.handler = (_, args) => {
            action(args, events.seq++);
        };
        return events;
    }
    
    internal void RemoveHandler() {
        store.OnChildEntitiesChanged -= handler;
    }
}

internal static class Events
{
    internal static ChildEntitiesChangedEvents AddHandler(EntityStore store, Action<ChildEntitiesChangedArgs> action)
    {
        return ChildEntitiesChangedEvents.AddHandler(store, action);
    }
    
    internal static ChildEntitiesChangedEvents SetHandlerSeq(EntityStore store, Action<ChildEntitiesChangedArgs, int> action)
    {
        return ChildEntitiesChangedEvents.SetHandlerSeq(store, action);
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
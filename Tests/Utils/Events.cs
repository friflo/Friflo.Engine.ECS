using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Tests.Utils;

internal class Events
{
    internal int seq;
    
    /// <summary>
    /// <see cref="ChildNodesChangedHandler"/>'s are used to create <see cref="NotifyCollectionChangedEventArgs"/> events.<br/>
    /// See: <see cref="Test_ObservableCollection_Reference"/>
    /// </summary>
    internal static Events SetHandler(GameEntityStore store, Action<ChildNodesChangedArgs> action)
    {
        var events = new Events();
        store.ChildNodesChangedHandler = (object _, in ChildNodesChangedArgs args) => {
            events.seq++;
            action(args);
        };
        return events;
    }
    
    /// <summary>
    /// <see cref="ChildNodesChangedHandler"/>'s are used to create <see cref="NotifyCollectionChangedEventArgs"/> events.<br/>
    /// See: <see cref="Test_ObservableCollection_Reference"/>
    /// </summary>
    internal static Events SetHandlerSeq(GameEntityStore store, Action<ChildNodesChangedArgs, int> action)
    {
        var events = new Events();
        store.ChildNodesChangedHandler = (object _, in ChildNodesChangedArgs args) => {
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Collections;

public static class Test_ExplorerItem
{
    [Test]
    public static void Test_ExplorerItem_Basics()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity(1);
        var tree        = new ExplorerTree(root);
        
        var rootEvents  = ExplorerEvents.SetHandlerSeq(tree.RootItem, (args, seq) => {
            switch (seq) {
                case 0: AreEqual("Add ChildIds[0] = 2",     args.AsString());   return;
            }
        });
        var child2          = store.CreateEntity(2);
        var child2Item      = tree.GetItemById(child2.Id);
        var child2Events    = ExplorerEvents.SetHandlerSeq(child2Item, (args, seq) => {
            switch (seq) {
                case 0: AreEqual("Add ChildIds[0] = 3",     args.AsString());   return;
                case 1: AreEqual("Remove ChildIds[0] = 3",  args.AsString());   return;
            }
        });
        root.AddChild(child2);
        
        var subChild3       = store.CreateEntity(3);
        child2.AddChild(subChild3);
        child2.RemoveChild(subChild3);
        
        AreEqual(1, rootEvents.seq);
        AreEqual(2, child2Events.seq);
    }
    
    [Test]
    public static void Test_ExplorerItemEnumerator()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity(1);
        var tree        = new ExplorerTree(root);
        
        root.AddChild(store.CreateEntity(2));
        root.AddChild(store.CreateEntity(3));
        root.AddChild(store.CreateEntity(4));

        int n = 2;
        foreach (var child in tree.RootItem) {
            AreEqual(n++, child.Id);
        }
        AreEqual(5, n);
        
        n = 2;
        IEnumerator<ExplorerItem> enumerator = tree.RootItem.GetEnumerator();
        IEnumerator enumerator2 = enumerator;
        enumerator.Reset();
        while (enumerator.MoveNext()) {
            AreEqual(n, enumerator.Current!.Id);
            var current2 = enumerator2.Current as ExplorerItem; // test coverage
            AreEqual(n, current2!.Id);
            n++;
        }
        AreEqual(5, n);
        
        enumerator.Dispose();
    }
    
    private static string AsString(this NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action) {
            case NotifyCollectionChangedAction.Add:
                var newItem     = args.NewItems![0] as ExplorerItem;
                return $"Add ChildIds[{args.NewStartingIndex}] = {newItem!.Id}";
            case NotifyCollectionChangedAction.Remove:
                var removeItem = args.OldItems![0] as ExplorerItem;
                return $"Remove ChildIds[{args.OldStartingIndex}] = {removeItem!.Id}";
            default:
                throw new InvalidOperationException("unexpected");
        }
    }
}


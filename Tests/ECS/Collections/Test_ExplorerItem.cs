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
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        var tree    = new ExplorerTree(root);
        
        var events = ExplorerEvents.SetHandlerSeq(tree.RootItem, (args, seq) => {
            switch (seq) {
                case 0: AreEqual("Add", args.AsString()); break;
            }
        });
        var child2  = store.CreateEntity(2);
        root.AddChild(child2);
        AreEqual(1, events.seq);
    }
    
    public static string AsString(this NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action) {
            case NotifyCollectionChangedAction.Add:
                return "Add";
            case NotifyCollectionChangedAction.Remove:
                return "Remove";
        }
        return null;
    }
}


using System;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_ExplorerItem
{
    /// <summary>Cover <see cref="ExplorerTree.ChildNodesChangedHandler"/></summary>
    [Test]
    public static void Test_ExplorerItem_assertion()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity(1);
        var tree        = new ExplorerTree(root, "test");
        
        var rootItem    = tree.rootItem;
        root.AddChild(store.CreateEntity(2));
        rootItem.CollectionChanged += (_, _) => {
            Fail("unexpected)");
        };
        var args = new ChildNodesChangedArgs((ChildNodesChangedAction)99, 1, 2, 0);
        var e = Throws<InvalidOperationException>(() => {
            tree.ChildNodesChangedHandler(null, args);    
        });
        AreEqual("unexpected action: 99", e!.Message);
    }
}
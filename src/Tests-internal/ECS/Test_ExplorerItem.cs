using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Collections;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_ExplorerItem
{
    /// <summary>Cover <see cref="ExplorerItemTree.ChildEntitiesChangedHandler"/></summary>
    [Test]
    public static void Test_ExplorerItem_assertion()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity(1);
        var tree        = new ExplorerItemTree(root, "test");
        
        var rootItem    = tree.rootItem;
        root.AddChild(store.CreateEntity(2));
        rootItem.CollectionChanged += (_, _) => {
            Fail("unexpected)");
        };
        var args = new ChildEntitiesChanged((ChildEntitiesChangedAction)99, store, 1, 2, 0);
        var e = Throws<InvalidOperationException>(() => {
            tree.ChildEntitiesChangedHandler(args);
        });
        AreEqual("unexpected action: 99", e!.Message);
    }
    
    [Test]
    public static void Test_ExplorerItem_null_Entity()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity(1);
        var tree        = new ExplorerItemTree(root, "test");
        var entity = new Entity();
        
        var e = Throws<ArgumentNullException>(() => {
            _ = new ExplorerItem(tree, entity);
        });
        AreEqual("entity", e!.ParamName);
        
        e = Throws<ArgumentNullException>(() => {
            _ = new ExplorerItem(null, entity);
        });
        AreEqual("tree", e!.ParamName);
    }
    
    [Test]
    public static void Test_ExplorerItem_DebugView()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity(1);
        root.AddChild(store.CreateEntity(2));
        root.AddChild(store.CreateEntity(3));
        var tree        = new ExplorerItemTree(root, "test");
        
        var rootItem    = tree.GetItemById(1);
        var rootDebug   = new ExplorerItemDebugView(rootItem);
        
        var rootItems   = rootDebug.Items;
        AreEqual(2, rootItems.Length);
        AreEqual(2, rootItems[0].Id);
        AreEqual(3, rootItems[1].Id);
    }
}

}
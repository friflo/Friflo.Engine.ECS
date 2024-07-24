using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_EntityStore
{
    /// <summary>Cover <see cref="EntityStore.DeleteNode"/></summary>
    [Ignore("check EntityStore.RemoveChildNode()")][Test]
    public static void Test_EntityStore_DeleteEntity()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var entity = store.CreateEntity(10);
    //  store.extension.parentMap[10] = 5;
        store.SetTreeParent(10, 5);
        
        var e = Throws<InvalidOperationException>(() => {
            entity.DeleteEntity();
        });
        AreEqual("unexpected state: child id not found. parent id: 5, child id: 10", e!.Message);
    }
    
    /// <summary>Cover <see cref="EntityStoreBase.AddArchetype"/></summary>
    [Test]
    public static void Test_Tags_cover_AddArchetype() {
        var store       = new EntityStore(PidType.RandomPids);
        var archetype   = store.GetArchetype(ComponentTypes.Get<Position>());
        
        archetype.SetInternalField(nameof(archetype.archIndex), 5);
        
        var e = Throws<InvalidOperationException>(() => {
            EntityStoreBase.AddArchetype(store, archetype);
        });
        AreEqual("invalid archIndex. expect: 2, was: 5", e!.Message);
    }
    
    /// <summary>Cover invariant assertion in <see cref="StoreExtension.RemoveScript"/></summary>
    [Test]
    public static void Test_EntityStore_RemoveScript() {
        var store       = new EntityStore(PidType.RandomPids);
        var entity      = store.CreateEntity();
        entity.AddScript(new TestScript1());
        store.extension.entityScriptCount = 1;

        var e = Throws<InvalidOperationException>(() => {
            entity.RemoveScript<TestScript1>();    
        });
        AreEqual("invariant: entityScriptCount > 0", e!.Message);
    }
    /*
    /// <summary>Test id assignment in <see cref="EntityStore.EnsureNodesLength"/></summary>
    [Test]
    public static void Test_EntityStore_EnsureNodesLength()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        for (int n = 0; n < 10; n++) {
            var nodes = store.nodes;
            int i = 0;
            for (; i <= store.Count; i++) {
                AreEqual(i, nodes[i].Id);
            }
            for (; i < nodes.Length; i++) {
                AreEqual(0, nodes[i].Id);
            }
            store.CreateEntity();
        }
    }
    */
}

}


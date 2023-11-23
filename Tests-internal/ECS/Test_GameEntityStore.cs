using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_GameEntityStore
{
    /// <summary>Cover <see cref="EntityStore.DeleteNode"/></summary>
    [Test]
    public static void Test_GameEntityStore_DeleteEntity()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var entity = store.CreateEntity(10);
        var nodes = (EntityNode[])store.GetInternalField("nodes");
        nodes[10].parentId = 5;
        
        var e = Throws<InvalidOperationException>(() => {
            entity.DeleteEntity();
        });
        AreEqual("unexpected state: child id not found. parent id: 5, child id: 10", e!.Message);
    }
    
    /// <summary>Cover <see cref="EntityStoreBase.AddArchetype"/></summary>
    [Test]
    public static void Test_Tags_cover_AddArchetype() {
        var store       = new EntityStore();
        var archetype   = store.GetArchetype(Signature.Get<Position>());
        
        archetype.SetInternalField(nameof(archetype.archIndex), 5);
        
        var e = Throws<InvalidOperationException>(() => {
            store.AddArchetype(archetype);
        });
        AreEqual("invalid archIndex. expect: 2, was: 5", e!.Message);
    }
}


using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;


// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_Find
{
    [Test]
    public static void Test_Find_UniqueEntity()
    {
        Assert.AreEqual("UniqueEntity: 'test'", new UniqueEntity("test").ToString());
        
        var store   = new EntityStore(PidType.RandomPids);
        
        var entity1 = store.CreateEntity();
        entity1.AddComponent(new UniqueEntity("Player"));
        
        var entity2 = store.CreateEntity();
        entity2.AddComponent(new UniqueEntity("Enemy-1"));
        
        var entity3 = store.CreateEntity();
        entity3.AddComponent(new UniqueEntity("Enemy-2"));
        
        var find1 = store.GetUniqueEntity("Player");
        Mem.AreEqual(entity1.Id, find1.Id);
        
        int count = 10;     // 100_000_000 ~ #PC: 2742 ms
        for (int n = 0; n < count; n++) {
            store.GetUniqueEntity("Player");
        }
        
        // --- test heap allocations
        var start = Mem.GetAllocatedBytes();
        store.GetUniqueEntity("Player");
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Find_UniqueEntity_error()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var archetype1  = store.GetArchetype(ComponentTypes.Get<Position>());
        
        var entity1 = archetype1.CreateEntity();
        entity1.AddComponent(new UniqueEntity("Player"));
        
        var entity2 = archetype1.CreateEntity();
        entity2.AddComponent(new UniqueEntity("Player"));
        
        var e = Assert.Throws<InvalidOperationException>(() => {
            store.GetUniqueEntity("Player");
        });
        Assert.AreEqual("found multiple UniqueEntity's with uid: \"Player\"", e!.Message);
        
        e = Assert.Throws<InvalidOperationException>(() => {
            store.GetUniqueEntity("Foo");
        });
        Assert.AreEqual("found no UniqueEntity with uid: \"Foo\"", e!.Message);
    }
    
    [Test]
    public static void Test_Find_UniqueEntity_perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype1  = store.GetArchetype(ComponentTypes.Get<Position>());
        
        for (int n = 0; n < 100; n++) {
            var entity = archetype1.CreateEntity();
            if (n % 2 == 0) entity.Enabled = false; //  
            entity.AddComponent(new UniqueEntity(n.ToString())); // create names with < 3 characters
        }
        var player = archetype1.CreateEntity();
        player.AddComponent(new UniqueEntity("xxx"));
        
        Assert.AreEqual(101, store.UniqueEntities.Count);
        
        Assert.AreEqual(101, store.GetUniqueEntity("xxx").Id);
        
        int count = 10;     // 10_000_000 ~ #PC: 2132 ms
        for (int n = 0; n < count; n++) {
            store.GetUniqueEntity("xxx"); // find name with 3 characters
        }
        
        var disabled = archetype1.CreateEntity();
        disabled.Enabled = false;
        disabled.AddComponent(new UniqueEntity("yyy"));
        Assert.AreEqual(102, store.GetUniqueEntity("yyy").Id);
    }
}

}

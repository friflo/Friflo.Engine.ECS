using System;
using System.Linq;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

#pragma warning disable CS0618 // Type or member is obsolete

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch;

public static class Test_Find
{
    [Test]
    public static void Test_Find_Entity()
    {
        var store       = new EntityStore();
        var archetype1  = store.GetArchetype(Signature.Get<Position>());
        var archetype2  = store.GetArchetype(Signature.Get<Position, Rotation>());
        var archetype3  = store.GetArchetype(Signature.Get<Position, Rotation, EntityName>());
        var archetype4  = store.GetArchetype(Tags.Get<TestTag>());
        
        var entity1 = store.CreateEntity(archetype1);
        var entity2 = store.CreateEntity(archetype2);
        var entity3 = store.CreateEntity(archetype3);
        var entity4 = store.CreateEntity(archetype4);
        entity2.AddTag<TestTag2>();
        
        var find1 = store.FindEntity(default, ComponentTypes.Get<EntityName>());
        Mem.AreEqual(entity3.Id, find1.Id);

        var find2 = store.FindEntityWithTags(Tags.Get<TestTag>());
        Mem.AreEqual(entity4.Id, find2.Id);
        
        var find3 = store.FindEntity(Tags.Get<TestTag2>(), ComponentTypes.Get<Position>());
        Mem.AreEqual(entity2.Id, find3.Id);
        
        int count = 10;     // 100_000_000 ~ #PC: 2719 ms
        for (int n = 0; n < count; n++) {
            store.FindEntityWithTags(Tags.Get<TestTag>());
        }
        
        // --- test heap allocations
        var start = Mem.GetAllocatedBytes();
        store.FindEntity(default, ComponentTypes.Get<EntityName>());
        store.FindEntityWithTags(Tags.Get<TestTag>());
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Find_Entities()
    {
        var store       = new EntityStore();
        var archetype1  = store.GetArchetype(Signature.Get<Position>());
        var archetype2  = store.GetArchetype(Signature.Get<Position, Rotation>());
        
        var entity1 = store.CreateEntity(archetype1);
        var entity2 = store.CreateEntity(archetype2);
        entity1.AddTag<TestTag>();
        entity2.AddTag<TestTag>();

        var find1 = store.FindEntitiesWithTags(Tags.Get<TestTag>());
        Assert.AreEqual(2, find1.Count());
        
        var find2 = store.FindEntities(Tags.Get<TestTag>(), ComponentTypes.Get<Position>());
        Assert.AreEqual(2, find2.Count());
        
        // --- test heap allocations
        var start = Mem.GetAllocatedBytes();
        store.FindEntitiesWithTags(Tags.Get<TestTag>());
        store.FindEntities(default, ComponentTypes.Get<Position>());
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Find_Cover()
    {
        var store       = new EntityStore();
        var archetype1  = store.GetArchetype(Signature.Get<Position, Rotation>());
        var archetype2  = store.GetArchetype(Signature.Get<Position, Rotation, EntityName>());
        var archetype3  = store.GetArchetype(Signature.Get<Position, Rotation, EntityName, Transform>());
        var archetype4  = store.GetArchetype(Signature.Get<Position, Rotation, EntityName, Transform, MyComponent1>());
        {
            var entity  = store.CreateEntity(archetype1);
            var find    = store.FindEntity(default, ComponentTypes.Get<Position, Rotation>());
            Mem.AreEqual(entity.Id, find.Id);
        } {
            var entity  = store.CreateEntity(archetype2);
            var find    = store.FindEntity(default, ComponentTypes.Get<Position, Rotation, EntityName>());
            Mem.AreEqual(entity.Id, find.Id);
        } {
            var entity  = store.CreateEntity(archetype3);
            var find    = store.FindEntity(default, ComponentTypes.Get<Position, Rotation, EntityName, Transform>());
            Mem.AreEqual(entity.Id, find.Id);
        } {
            var entity  = store.CreateEntity(archetype4);
            var find    = store.FindEntity(default, ComponentTypes.Get<Position, Rotation, EntityName, Transform, MyComponent1>());
            Mem.AreEqual(entity.Id, find.Id);
        }
    }
    
    [Test]
    public static void Test_Find_Error()
    {
        var store       = new EntityStore();
        var archetype1  = store.GetArchetype(Signature.Get<Position>());
        var archetype2  = store.GetArchetype(Signature.Get<Position, Rotation>());
        
        store.CreateEntity(archetype1);
        store.CreateEntity(archetype2);
        
        var e = Assert.Throws<InvalidOperationException>(() => {
            store.FindEntity(default, ComponentTypes.Get<Position>());
        });
        Assert.AreEqual("found multiple matching entities. found: 2", e!.Message);
        
        e = Assert.Throws<InvalidOperationException>(() => {
            store.FindEntity(default, ComponentTypes.Get<EntityName>());
        });
        Assert.AreEqual("found no matching entity", e!.Message);
    }
}


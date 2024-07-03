using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {


public static class Test_Index_Delete
{
    [Test]
    public static void Test_Index_Delete_Value()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        var values  = store.GetAllIndexedComponentValues<IndexedInt, int>();
        
        entity1.AddComponent(new IndexedInt { value = 10 });
        entity2.AddComponent(new IndexedInt { value = 10 });
        entity3.AddComponent(new IndexedInt { value = 20 });
        AreEqual("{ 10, 20 }", values.ToStr());
        
        entity1.DeleteEntity();
        AreEqual("{ 10, 20 }", values.ToStr());
        
        entity2.DeleteEntity();
        AreEqual("{ 20 }", values.ToStr());
        
        entity3.DeleteEntity();
        AreEqual("{ }", values.ToStr());
    }
    
    [Test]
    public static void Test_Index_Delete_Class()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        var values  = store.GetAllIndexedComponentValues<IndexedName, string>();
        
        entity1.AddComponent(new IndexedName { name = "abc" });
        entity2.AddComponent(new IndexedName { name = "abc" });
        entity3.AddComponent(new IndexedName { name = "xyz" });
        AreEqual("{ abc, xyz }", values.ToStr());
        
        entity1.DeleteEntity();
        AreEqual("{ abc, xyz }", values.ToStr());
        
        entity2.DeleteEntity();
        AreEqual("{ xyz }", values.ToStr());
        
        entity3.DeleteEntity();
        AreEqual("{ }", values.ToStr());
    }
    
    [Test]
    public static void Test_Index_Delete_Entity()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var target1 = store.CreateEntity(4);
        var target2 = store.CreateEntity(5);
        
        var values  = store.GetAllIndexedComponentValues<AttackComponent, Entity>();
        
        entity1.AddComponent(new AttackComponent { target = target1 });
        entity2.AddComponent(new AttackComponent { target = target1 });
        entity3.AddComponent(new AttackComponent { target = target2 });
        AreEqual("{ 4, 5 }", values.ToStr());
        
        entity1.DeleteEntity();
        AreEqual("{ 4, 5 }", values.ToStr());
        
        entity2.DeleteEntity();
        AreEqual("{ 5 }", values.ToStr());
        
        entity3.DeleteEntity();
        AreEqual("{ }", values.ToStr());
    }
    
    [Test]
    public static void Test_Index_Delete_linked_entities()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var target1 = store.CreateEntity(10);
        var target2 = store.CreateEntity(11);
        
        var targets = store.GetAllIndexedComponentValues<AttackComponent, Entity>();
        
        entity1.AddComponent(new AttackComponent { target = target1 });
        entity2.AddComponent(new AttackComponent { target = target1 });
        entity3.AddComponent(new AttackComponent { target = target2 });
        // --- initial targets state
        AreEqual("{ 1, 2 }",    target1.GetEntityReferences<AttackComponent>().ToStr());
        AreEqual("{ 3 }",       target2.GetEntityReferences<AttackComponent>().ToStr());
        AreEqual("{ 10, 11 }",  targets.ToStr());
        
        target1.DeleteEntity();
        IsFalse (               entity1.HasComponent<AttackComponent>());
        IsFalse (               entity2.HasComponent<AttackComponent>());
        AreEqual("{ 11 }",      targets.ToStr());
        
        target2.DeleteEntity();
        IsFalse (               entity3.HasComponent<AttackComponent>());
        AreEqual("{ }",         targets.ToStr());
    }
    
    [Test]
    public static void Test_Index_Delete_Range()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        
        var values  = store.GetAllIndexedComponentValues<IndexedIntRange, int>();
        
        entity1.AddComponent(new IndexedIntRange { value = 10 });
        entity2.AddComponent(new IndexedIntRange { value = 10 });
        entity3.AddComponent(new IndexedIntRange { value = 20 });
        AreEqual("{ 10, 20 }", values.ToStr());
        
        entity1.DeleteEntity();
        AreEqual("{ 10, 20 }", values.ToStr());
        
        entity2.DeleteEntity();
        AreEqual("{ 20 }", values.ToStr());
        
        entity3.DeleteEntity();
        AreEqual("{ }", values.ToStr());
    }
}

}

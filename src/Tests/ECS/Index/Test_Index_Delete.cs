using Friflo.Engine.ECS;
using NUnit.Framework;
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
        AreEqual(2, values.Count);
        
        entity1.DeleteEntity();
        AreEqual(2, values.Count);  // 10 is still indexed
        
        entity2.DeleteEntity();
        AreEqual(1, values.Count);  // removed 10
        
        entity3.DeleteEntity();
        AreEqual(0, values.Count);  // removed 20
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
        AreEqual(2, values.Count);
        
        entity1.DeleteEntity();
        AreEqual(2, values.Count);  // 10 is still indexed
        
        entity2.DeleteEntity();
        AreEqual(1, values.Count);  // removed 10
        
        entity3.DeleteEntity();
        AreEqual(0, values.Count);  // removed 20
    }
    
    [Test]
    public static void Test_Index_Delete_Entity()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var target1 = store.CreateEntity();
        var target2 = store.CreateEntity();
        
        var values  = store.GetAllIndexedComponentValues<AttackComponent, Entity>();
        
        entity1.AddComponent(new AttackComponent { target = target1 });
        entity2.AddComponent(new AttackComponent { target = target1 });
        entity3.AddComponent(new AttackComponent { target = target2 });
        AreEqual(2, values.Count);
        
        entity1.DeleteEntity();
        AreEqual(2, values.Count);  // 10 is still indexed
        
        entity2.DeleteEntity();
        AreEqual(1, values.Count);  // removed 10
        
        entity3.DeleteEntity();
        AreEqual(0, values.Count);  // removed 20
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
        AreEqual(2, values.Count);
        
        entity1.DeleteEntity();
        AreEqual(2, values.Count);  // 10 is still indexed
        
        entity2.DeleteEntity();
        AreEqual(1, values.Count);  // removed 10
        
        entity3.DeleteEntity();
        AreEqual(0, values.Count);  // removed 20
    }
}

}

using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_Entities
{
    [Test]
    public static void Test_Entities_DebugView()
    {
        var store       = new EntityStore();
        var type        = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        var entities    = type.CreateEntities(10);
        AreSame (store, entities.EntityStore);
        AreEqual(10, entities.Count);
        
        var view    = new EntitiesDebugView(entities);
        var array   = view.Entities;
        AreEqual(10, array.Length);
        for (int n = 0; n < 10; n++) {
            var entity = array[n];
            AreSame (store, entity.Store);
            AreEqual(n + 1, entity.Id);   
        }
        
        var entities1   = new Entities(store, 1);
        view            = new EntitiesDebugView(entities1);
        array           = view.Entities;
        AreEqual(1,     array.Length);
        AreEqual(1,     array[0].Id);
        AreSame (store, array[0].Store);
    }
    
    [Test]
    public static void Test_Entities_constructors()
    {
        var store = new EntityStore();
        store.CreateEntity(1);
        store.CreateEntity(2);
        store.CreateEntity(42);

        // --- Length: 0
        var entities0 = new Entities(store, 0);
        AreEqual("Entity[0]", entities0.ToString());
        AreEqual(0,     entities0.Count);
        AreEqual(0,     entities0.Count);
        AreSame (store, entities0.EntityStore);
        
        int count = 0;
        foreach (var _ in entities0) {
            count++;
        }
        AreEqual(0, count);
            
        // --- Length: 1
        var entities1 = new Entities(store, 42);
        AreEqual("Entity[1]", entities1.ToString());
        AreEqual(1, entities1.Count);
        AreEqual(1,     entities1.Count);
        AreSame (store, entities1[0].Store);
        AreEqual(42,    entities1[0].Id);
        
        count = 0;
        foreach (var entity in entities1) {
            count++;
            AreEqual(42, entity.Id);
        }
        AreEqual(1, count);
            
        // --- Length: 2
        var entities2 = new Entities(store, new [] { 1, 2 }, 0, 2);
        AreEqual("Entity[2]", entities2.ToString());
        AreEqual(2, entities2.Count);
        AreEqual(2,     entities2.Count);
        AreSame (store, entities2[0].Store);
        AreEqual(1,     entities2[0].Id);
        AreEqual(2,     entities2[1].Id);

        count = 0;
        foreach (var entity in entities2) {
            switch (count++) {
                case 0: AreEqual(1, entity.Id); break;
                case 1: AreEqual(2, entity.Id); break;
            }
        }
        AreEqual(2, count);
    }
}

}

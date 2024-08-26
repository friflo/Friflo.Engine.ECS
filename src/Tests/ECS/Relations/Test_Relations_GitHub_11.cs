using System;
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Relations
{
    // Fix https://github.com/friflo/Friflo.Engine.ECS/issues/11
    public static class Test_Relations_GitHub_11
    {
        [Test]
        public static void Test_Rel_Test()
        {
            var store = new EntityStore();
            var entity1 = store.CreateEntity();
            entity1.Add(new MyComponent1 { a = 1 });
            
            var entity2 = store.CreateEntity();
            entity2.Add(new MyComponent1 { a = 2 });
            
            var entity3 = store.CreateEntity();
            
            entity1.AddRelation(new AttackRelation { target = entity3 });
            entity2.AddRelation(new AttackRelation { target = entity3 });
            
            entity1.RemoveRelation<AttackRelation>(entity3);
            
            Assert.AreEqual(1, entity1.GetComponent<MyComponent1>().a);
            Assert.AreEqual(2, entity2.GetComponent<MyComponent1>().a);
            Console.WriteLine($"{entity1.GetComponent<MyComponent1>().a} {entity2.GetComponent<MyComponent1>().a}");
        }
    }
}
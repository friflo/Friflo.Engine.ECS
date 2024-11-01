using System;
using System.Linq;
using Friflo.Engine.ECS;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Tests.ECS.Github
{
    /// <summary>
    /// Example to sort entities of a query
    /// https://github.com/friflo/Friflo.Engine.ECS/issues/25
    /// </summary>
    public static class Test_GitHub_25
    {
        [Test]
        public static void SortEntities()
        {
            var store = new EntityStore();
            store.CreateEntity(new MyComponent1 { a = 30 });
            store.CreateEntity(new MyComponent1 { a = 20 });
            store.CreateEntity(new MyComponent1 { a = 40 });
            store.CreateEntity(new MyComponent1 { a = 10 });
            var query = store.Query<MyComponent1>();
            var array = query.ToEntityList().ToArray();
            Array.Sort(array, (entity1, entity2) =>
                entity1.GetComponent<MyComponent1>().a - entity2.GetComponent<MyComponent1>().a);
            
            Assert.AreEqual(10, array[0].GetComponent<MyComponent1>().a);
            Assert.AreEqual(20, array[1].GetComponent<MyComponent1>().a);
            Assert.AreEqual(30, array[2].GetComponent<MyComponent1>().a);
            Assert.AreEqual(40, array[3].GetComponent<MyComponent1>().a);
        }
    }
}

using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS.Index;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Github
{
    /// <summary>
    /// Update indexed components on deserialization 
    /// https://github.com/friflo/Friflo.Engine.ECS/issues/15
    /// </summary>
    public static class Test_GitHub_15
    {
        [Test]
        public static void Test_GitHub_15_update_indexed_components_on_deserialization()
        {
            var store = new EntityStore();
            var entityA = store.CreateEntity(1);
            entityA.AddComponent(new IndexedInt { value = 11 });
            var entityB = store.CreateEntity(2);
            entityB.AddComponent(new IndexedInt { value = 12 });
            
            AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(11).Count);
            AreEqual(2, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
            
            using var stream = new MemoryStream();
            var serializer = new EntitySerializer();
            serializer.WriteStore(store, stream);
            
            var readStore = new EntityStore();
            serializer.ReadIntoStore(readStore, stream);
            AreEqual(2, readStore.Count);
            AreEqual(1, readStore.GetEntitiesWithComponentValue<IndexedInt,int>(11).Count);
            AreEqual(2, readStore.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        }
    }
}

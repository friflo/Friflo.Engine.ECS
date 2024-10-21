using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Relations
{
    // https://github.com/friflo/Friflo.Engine.ECS/issues/13
    [Ignore("FIXME https://github.com/friflo/Friflo.Engine.ECS/issues/13")]
    public static class Test_Relations_GitHub_13
    {
        [Test]
        public static void Test_Rel_Test()
        {
            var store = new EntityStore();
            var entity1 = store.CreateEntity(1);
            var entity2 = store.CreateEntity(2);
            Assert.AreEqual(0, entity2.GetIncomingLinks<AttackRelation>().Count);
            
            entity1.AddRelation(new AttackRelation{ target = entity2 });
            Assert.AreEqual(1, entity1.GetRelations<AttackRelation>().Length);
            Assert.AreEqual(1, entity2.GetIncomingLinks<AttackRelation>().Count);
            
            entity1.DeleteEntity();
            var incomingLinks = entity2.GetIncomingLinks<AttackRelation>();
            Assert.AreEqual(0, incomingLinks.Count);
        }
    }
}
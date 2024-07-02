using System.Linq;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Relations {


public static class Test_Relations_Delete
{
    [Test]
    public static void Test_Relations_Delete_Int()
    {
        var store       = new EntityStore();
        var allEntities = store.GetAllEntitiesWithRelations<IntRelation>();
        AreEqual("{ }",         allEntities.ToStr());

        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.AddComponent(new IntRelation { value = 10 });
        entity1.AddComponent(new IntRelation { value = 20 });
        AreEqual("{ 1 }",       allEntities.ToStr());
        
        entity2.AddComponent(new IntRelation { value = 30 });
        AreEqual("{ 1, 2 }",    allEntities.ToStr());
        
        int count = 0;
        // --- version: iterate all entity relations in O(N)
        store.ForAllEntityRelations((ref IntRelation relation, Entity entity) => {
            switch (count++) {
                case 0: AreEqual(1, entity.Id); AreEqual(10, relation.value); break;
                case 1: AreEqual(1, entity.Id); AreEqual(20, relation.value); break;
                case 2: AreEqual(2, entity.Id); AreEqual(30, relation.value); break;
            }
        });
        AreEqual(3, count);
        
        // --- version: get all entity relations in O(1)
        var (entities, relations) = store.GetAllEntityRelations<IntRelation>();
        AreEqual(3, entities.Count); AreEqual(3,  relations.Length);
        AreEqual(1, entities[0].Id); AreEqual(10, relations[0].value);
        AreEqual(1, entities[1].Id); AreEqual(20, relations[1].value);
        AreEqual(2, entities[2].Id); AreEqual(30, relations[2].value);
        
        entity1.DeleteEntity();
        AreEqual("{ 2 }",       allEntities.ToStr());
        var array = allEntities.ToArray();
        AreEqual(30, array[0].GetRelation<IntRelation, int>(30).value);
        
        entity2.DeleteEntity();
        AreEqual("{ }",         allEntities.ToStr());
    }
}

}

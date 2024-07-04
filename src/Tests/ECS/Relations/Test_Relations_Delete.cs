using System.Linq;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Relations {


public static class Test_Relations_Delete
{
    [Test]
    public static void Test_Relations_Delete_int_relations()
    {
        var store       = new EntityStore();
        var allEntities = store.GetAllEntitiesWithRelations<IntRelation>();
        AreEqual("{ }",         allEntities.Debug());

        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.AddComponent(new IntRelation { value = 10 });
        entity1.AddComponent(new IntRelation { value = 20 });
        AreEqual("{ 1 }",       allEntities.Debug());
        
        entity2.AddComponent(new IntRelation { value = 30 });
        AreEqual("{ 1, 2 }",    allEntities.Debug());
        
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
        AreEqual("{ 2 }",       allEntities.Debug());
        var array = allEntities.ToArray();
        AreEqual(30, array[0].GetRelation<IntRelation, int>(30).value);
        
        entity2.DeleteEntity();
        AreEqual("{ }",         allEntities.Debug());
    }
    
    [Test]
    public static void Test_Relations_Delete_Entity_relations()
    {
        var store       = new EntityStore();
        var sourceNodes = store.GetAllEntitiesWithRelations<AttackRelation>();
        AreEqual("{ }",         sourceNodes.Debug());
        
        var target10    = store.CreateEntity(10);
        var target11    = store.CreateEntity(11);
        var target12    = store.CreateEntity(12);

        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);                            //  10      1      11
        AreEqual("{ }",         sourceNodes.Debug());                       //  12      2 
        
        entity1.AddComponent(new AttackRelation { target = target10 });     //  10  🡐  1      11
        AreEqual("{ 1 }",       sourceNodes.Debug());                       //  12      2 
        
        entity1.AddComponent(new AttackRelation { target = target11 });     //  10  🡐  1  🡒  11
        AreEqual("{ 1 }",       sourceNodes.Debug());                       //  12
        
        entity2.AddComponent(new AttackRelation { target = target12 });     //  10  🡐  1  🡒  11
        AreEqual("{ 1, 2 }",    sourceNodes.Debug());                       //  12  🡐  2
        
        int count = 0;
        // --- version: iterate all entity relations in O(N)
        store.ForAllEntityRelations((ref AttackRelation relation, Entity entity) => {
            switch (count++) {
                case 0: AreEqual(1, entity.Id); AreEqual(target10, relation.target); break;
                case 1: AreEqual(1, entity.Id); AreEqual(target11, relation.target); break;
                case 2: AreEqual(2, entity.Id); AreEqual(target12, relation.target); break;
            }
        });
        AreEqual(3, count);
        
        // --- version: get all entity relations in O(1)
        var (entities, relations) = store.GetAllEntityRelations<AttackRelation>();
        AreEqual(3, entities.Count); AreEqual(3,  relations.Length);
        AreEqual(1, entities[0].Id); AreEqual(target10, relations[0].target);
        AreEqual(1, entities[1].Id); AreEqual(target11, relations[1].target);
        AreEqual(2, entities[2].Id); AreEqual(target12, relations[2].target);
        
        entity1.DeleteEntity();                                             //  10           11
        AreEqual("{ 2 }",       sourceNodes.Debug());                       //  12  🡐  2
        
        var array =             sourceNodes.ToArray();
        AreEqual(target12, array[0].GetRelation<AttackRelation, Entity>(target12).target);
        
        entity2.DeleteEntity();                                             //  10           11
        AreEqual("{ }",         sourceNodes.Debug());                       //  12
    }
    
    [Test]
    public static void Test_Relations_Delete_Entity_target()
    {
        var store       = new EntityStore();

        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        var entity3     = store.CreateEntity(3);
                                                                                                //  1     2     3
        AreEqual("{ }",         entity2.GetLinkRelationReferences<AttackRelation>().Debug());   //
        var sourceNodes = store.GetAllEntitiesWithRelations<AttackRelation>();
        
        entity1.AddComponent(new AttackRelation { target = entity2 });                          //  1  🡒  2     3
        AreEqual("{ 1 }",       sourceNodes.Debug());                                           //
        AreEqual("{ 1 }",       entity2.GetLinkRelationReferences<AttackRelation>().Debug());
        AreEqual(1,             entity1.GetRelations<AttackRelation>().Length);
        
        entity2.AddComponent(new AttackRelation { target = entity2 });                          //  1  🡒  2     3
        AreEqual("{ 1, 2 }",    sourceNodes.Debug());                                           //        ⮍
        AreEqual("{ 1, 2 }",    entity2.GetLinkRelationReferences<AttackRelation>().Debug());
        AreEqual(1,             entity2.GetRelations<AttackRelation>().Length);
        
        entity3.AddComponent(new AttackRelation { target = entity2 });                          //  1  🡒  2  🡐  3
        AreEqual("{ 1, 2, 3 }", sourceNodes.Debug());                                           //        ⮍
        AreEqual("{ 1, 2, 3 }", entity2.GetLinkRelationReferences<AttackRelation>().Debug());
        AreEqual(1,             entity3.GetRelations<AttackRelation>().Length);
        
        entity2.DeleteEntity();                                                                 //  1            3
        AreEqual("{ }",         sourceNodes.Debug());                                           //
        AreEqual(0,             entity1.GetRelations<AttackRelation>().Length);
        AreEqual(0,             entity3.GetRelations<AttackRelation>().Length);
    }
}

}

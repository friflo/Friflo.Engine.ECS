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
        
        entity1.AddRelation(new IntRelation { value = 10 });
        entity1.AddRelation(new IntRelation { value = 20 });
        AreEqual("{ 10, 20 }",  entity1.GetRelations<IntRelation>().Debug());
        AreEqual("{ 1 }",       allEntities.Debug());
        
        entity2.AddRelation(new IntRelation { value = 30 });
        AreEqual("{ 30 }",      entity2.GetRelations<IntRelation>().Debug());
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
        AreEqual("{ 1, 1, 2 }",     entities.Debug());
        AreEqual("{ 10, 20, 30 }",  relations.Debug());
            
        entity1.DeleteEntity();
        AreEqual("{ 2 }",           allEntities.Debug());

        entity2.DeleteEntity();
        AreEqual("{ }",             allEntities.Debug());
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
        var entity2     = store.CreateEntity(2);                            //  10     1     11
        AreEqual("{ }",         sourceNodes.Debug());                       //  12     2 
        
        entity1.AddRelation(new AttackRelation { target = target10 });      //  10  ←  1     11
        AreEqual("{ 1 }",       sourceNodes.Debug());                       //  12     2 
        
        entity1.AddRelation(new AttackRelation { target = target11 });      //  10  ←  1  →  11
        AreEqual("{ 1 }",       sourceNodes.Debug());                       //  12
        
        entity2.AddRelation(new AttackRelation { target = target12 });      //  10  ←  1  →  11
        AreEqual("{ 1, 2 }",    sourceNodes.Debug());                       //  12  ←  2
        
        int count = 0;
        // --- version: iterate all entity relations in O(N)
        store.ForAllEntityRelations((ref AttackRelation relation, Entity entity) => {
            switch (count++) {
                case 0: AreEqual(1, entity.Id); AreEqual(10, relation.target.Id); break;
                case 1: AreEqual(1, entity.Id); AreEqual(11, relation.target.Id); break;
                case 2: AreEqual(2, entity.Id); AreEqual(12, relation.target.Id); break;
            }
        });
        AreEqual(3, count);
        
        // --- version: get all entity relations in O(1)
        var (entities, relations) = store.GetAllEntityRelations<AttackRelation>();
        AreEqual("{ 1, 1, 2 }",     entities.Debug());
        AreEqual("{ 10, 11, 12 }",  relations.Debug());
        
        entity1.DeleteEntity();                                             //  10     -     11
        AreEqual("{ 2 }",           sourceNodes.Debug());                   //  12  ←  2
        
        var entity2Relation = entity2.GetRelation<AttackRelation, Entity>(target12);
        AreEqual(12, entity2Relation.target.Id);
        
        entity2.DeleteEntity();                                             //  10     -     11
        AreEqual("{ }",             sourceNodes.Debug());                   //  12     -
    }
    
    [Test]
    public static void Test_Relations_Delete_Entity_target()
    {
        var store       = new EntityStore();

        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        var entity3     = store.CreateEntity(3);
                                                                                        //  1     2     3
        AreEqual("{ }",         entity2.GetIncomingLinks<AttackRelation>().Debug());    //
        
        entity1.AddRelation(new AttackRelation { target = entity2 });                   //  1  →  2     3
        AreEqual("{ 1 }",       entity2.GetIncomingLinks<AttackRelation>().Debug());    //
        AreEqual("{ 2 }",       entity1.GetRelations<AttackRelation>().Debug());
        
        entity2.AddRelation(new AttackRelation { target = entity2 });                   //  1  →  2     3
        AreEqual("{ 1, 2 }",    entity2.GetIncomingLinks<AttackRelation>().Debug());    //        ⮍
        AreEqual("{ 2 }",       entity2.GetRelations<AttackRelation>().Debug());
        
        entity3.AddRelation(new AttackRelation { target = entity2 });                   //  1  →  2  ←  3
        AreEqual("{ 1, 2, 3 }", entity2.GetIncomingLinks<AttackRelation>().Debug());    //        ⮍
        AreEqual("{ 2 }",       entity3.GetRelations<AttackRelation>().Debug());
        
        entity2.DeleteEntity();                                                         //  1     -     3
        AreEqual("{ }",         entity1.GetRelations<AttackRelation>().Debug());        //
        AreEqual("{ }",         entity3.GetRelations<AttackRelation>().Debug());
    }
}

}

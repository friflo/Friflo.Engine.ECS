using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS.Index;
using Tests.ECS.Relations;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Component-Types
public static class Component_Types
{
    
[Test]
public static void LinkComponents()
{
    var store   = new EntityStore();

    var entity1 = store.CreateEntity(1);
    var entity2 = store.CreateEntity(2);                            //                  link components
    var entity3 = store.CreateEntity(3);                            //                  1     2     3
    
    // add a link component to entity (2) referencing entity (1)
    entity2.AddComponent(new AttackComponent { target = entity1 }); //                  1  ←  2     3
    entity1.GetIncomingLinks<AttackComponent>();                    // { 2 }

    // update link component of entity (2). It links now entity (3)
    entity2.AddComponent(new AttackComponent { target = entity3 }); //                  1     2  →  3
    entity1.GetIncomingLinks<AttackComponent>();                    // { }
    entity3.GetIncomingLinks<AttackComponent>();                    // { 2 }

    // delete entity (3) is reflected by incoming links query
    entity3.DeleteEntity();                                         //                  1     2
    entity2.GetIncomingLinks<AttackComponent>();                    // { }
}

[Test]
public static void LinkRelations()
{
    var store   = new EntityStore();
    
    var entity1 = store.CreateEntity(1);
    var entity2 = store.CreateEntity(2);                            //                  link relations
    var entity3 = store.CreateEntity(3);                            //                  1     2     3
    
    // add a link relation to entity (2) referencing entity (1)
    entity2.AddRelation(new AttackRelation { target = entity1 });   //                  1  ←  2     3
    entity2.GetRelations    <AttackRelation>();                     // { 1 }
    entity1.GetIncomingLinks<AttackRelation>();                     // { 2 }
    
    // add another one. An entity can have multiple relation components
    entity2.AddRelation(new AttackRelation { target = entity3 });   //                  1  ←  2  →  3
    entity2.GetRelations    <AttackRelation>();                     // { 1, 3 }
    entity3.GetIncomingLinks<AttackRelation>();                     // { 2 }
    
    // deleting a linked entity (1) removes all link relations referencing it
    entity1.DeleteEntity();                                         //                        2  →  3
    entity2.GetRelations    <AttackRelation>();                     // { 3 }
    
    // deleting entity (2) is reflected by incoming links query
    entity2.DeleteEntity();                                         //                              3
    entity3.GetIncomingLinks<AttackRelation>();                     // { }
}



}

}
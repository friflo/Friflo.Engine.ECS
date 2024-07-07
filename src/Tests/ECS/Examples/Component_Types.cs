using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Component-Types
public static class Component_Types
{

#region link component

struct AttackComponent : ILinkComponent {
    public  Entity  target;
    public  Entity  GetIndexedValue() => target;
}

[Test]
public static void LinkComponents()
{
    var store   = new EntityStore();

    var entity1 = store.CreateEntity(1);
    var entity2 = store.CreateEntity(2);                            //              link components (drawn as →)
    var entity3 = store.CreateEntity(3);                            //              1     2     3
    
    // add a link component to entity (2) referencing entity (1)
    entity2.AddComponent(new AttackComponent { target = entity1 }); //              1  ←  2     3
    entity1.GetIncomingLinks<AttackComponent>();                    // { 2 }

    // update link component of entity (2). It links now entity (3)
    entity2.AddComponent(new AttackComponent { target = entity3 }); //              1     2  →  3
    entity1.GetIncomingLinks<AttackComponent>();                    // { }
    entity3.GetIncomingLinks<AttackComponent>();                    // { 2 }

    // deleting a linked entity (3) removes all link components referencing it
    entity3.DeleteEntity();                                         //              1     2
    entity2.HasComponent    <AttackComponent>();                    // false
}
#endregion


#region link relation

struct AttackRelation : ILinkRelation {
    public  Entity  target;
    public  Entity  GetRelationKey() => target;
}

[Test]
public static void LinkRelations()
{
    var store   = new EntityStore();
    
    var entity1 = store.CreateEntity(1);
    var entity2 = store.CreateEntity(2);                            //              link relations (drawn as →)
    var entity3 = store.CreateEntity(3);                            //              1     2     3
    
    // add a link relation to entity (2) referencing entity (1)
    entity2.AddRelation(new AttackRelation { target = entity1 });   //              1  ←  2     3
    entity2.GetRelations    <AttackRelation>();                     // { 1 }
    entity1.GetIncomingLinks<AttackRelation>();                     // { 2 }
    
    // add another one. An entity can have multiple link relations
    entity2.AddRelation(new AttackRelation { target = entity3 });   //              1  ←  2  →  3
    entity2.GetRelations    <AttackRelation>();                     // { 1, 3 }
    entity3.GetIncomingLinks<AttackRelation>();                     // { 2 }
    
    // deleting a linked entity (1) removes all link relations referencing it
    entity1.DeleteEntity();                                         //                    2  →  3
    entity2.GetRelations    <AttackRelation>();                     // { 3 }
    
    // deleting entity (2) is reflected by incoming links query
    entity2.DeleteEntity();                                         //                          3
    entity3.GetIncomingLinks<AttackRelation>();                     // { }
}
#endregion


#region indexed component

struct Player : IIndexedComponent<string>
{
    public  string  name;
    public  string  GetIndexedValue() => name;
}

[Test]
public static void IndexedComponents()
{
    var store   = new EntityStore();
    for (int n = 0; n < 1000; n++) {
        var entity = store.CreateEntity();
        entity.AddComponent(new Player { name = $"Player-{n,0:000}"});
    }
    // get all entities where Player.name == "Player-001". O(1)
    var lookup = store.GetEntitiesWithComponentValue<Player,string>("Player-001");
    Console.WriteLine($"lookup: {lookup.Count}");                           // > lookup: 1
    
    // return same result as lookup using a Query(). O(1)
    var query      = store.Query().HasValue    <Player,string>("Player-001");
    Console.WriteLine($"query: {query.Count}");                             // > query: 1
    
    // return all entities with a Player.name in the given range. O(N ⋅ log N) - N: all unique player names
    var rangeQuery = store.Query().ValueInRange<Player,string>("Player-000", "Player-099");
    Console.WriteLine($"range query: {rangeQuery.Count}");                  // > range query: 100
    
    // get all unique Player.name's. O(1)
    var allNames = store.GetAllIndexedComponentValues<Player,string>();
    Console.WriteLine($"all names: {allNames.Count}");                      // > all names: 1000
}

#endregion


#region relation component

[Test]
public static void RelationComponents()
{
    // todo
}
#endregion

}

}
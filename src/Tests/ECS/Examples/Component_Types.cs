using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable NotAccessedField.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://friflo.gitbook.io/friflo.engine.ecs/examples/component-types
public static class Component_Types
{

#region link component
[Test]
public static void LinkComponent_Snippets()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    var entity2 = store.CreateEntity();
    // --- snippets
    entity.AddComponent(new AttackComponent { target = entity2 });
    entity.GetComponent    <AttackComponent>();
    entity.RemoveComponent <AttackComponent>();
    entity.HasComponent    <AttackComponent>();
}

struct AttackComponent : ILinkComponent {
    public  Entity  target;
    public  Entity  GetIndexedValue() => target;
}

[Test]
public static void LinkComponents()
{
    var store   = new EntityStore();

    var entity1 = store.CreateEntity(1);                            // link components
    var entity2 = store.CreateEntity(2);                            // symbolized as →
    var entity3 = store.CreateEntity(3);                            //   1     2     3
    
    // add a link component to entity (2) referencing entity (1)
    entity2.AddComponent(new AttackComponent { target = entity1 }); //   1  ←  2     3
    // get all incoming links of given type.    O(1)
    entity1.GetIncomingLinks<AttackComponent>();        // { 2 }

    // update link component of entity (2). It links now entity (3)
    entity2.AddComponent(new AttackComponent { target = entity3 }); //   1     2  →  3
    entity1.GetIncomingLinks<AttackComponent>();        // { }
    entity3.GetIncomingLinks<AttackComponent>();        // { 2 }

    // deleting a linked entity (3) removes all link components referencing it
    entity3.DeleteEntity();                                         //   1     2
    entity2.HasComponent    <AttackComponent>();        // false
}
#endregion


#region link relation

[Test]
public static void LinkRelation_Snippets()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    var entity1 = store.CreateEntity();
    var entity2 = store.CreateEntity();
    // --- snippets
    entity.AddRelation(new AttackRelation { target = entity1 });
    entity.AddRelation(new AttackRelation { target = entity2 });
    entity.RemoveRelation <AttackRelation>(entity1);
    entity.GetRelations   <AttackRelation>();               // O(1)
    entity.GetRelation    <AttackRelation,Entity>(entity2); // O(1)
}

struct AttackRelation : ILinkRelation {
    public  Entity  target;
    public  Entity  GetRelationKey() => target;
}

[Test]
public static void LinkRelations()
{
    var store   = new EntityStore();
    
    var entity1 = store.CreateEntity(1);                          // link relations
    var entity2 = store.CreateEntity(2);                          // symbolized as →
    var entity3 = store.CreateEntity(3);                          //   1     2     3
    
    // add a link relation to entity (2) referencing entity (1)
    entity2.AddRelation(new AttackRelation { target = entity1 }); //   1  ←  2     3
    // get all links added to the entity. O(1)
    entity2.GetRelations    <AttackRelation>();     // { 1 }
    // get all incoming links. O(1)
    entity1.GetIncomingLinks<AttackRelation>();     // { 2 }
    
    // add another one. An entity can have multiple link relations
    entity2.AddRelation(new AttackRelation { target = entity3 }); //   1  ←  2  →  3
    entity2.GetRelations    <AttackRelation>();     // { 1, 3 }
    entity3.GetIncomingLinks<AttackRelation>();     // { 2 }
    
    // deleting a linked entity (1) removes all link relations referencing it
    entity1.DeleteEntity();                                       //         2  →  3
    entity2.GetRelations    <AttackRelation>();     // { 3 }
    
    // deleting entity (2) is reflected by incoming links query
    entity2.DeleteEntity();                                       //               3
    entity3.GetIncomingLinks<AttackRelation>();     // { }
}
#endregion


#region relation component

[Test]
public static void RelationComponent_Snippets()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    // --- snippets
    entity.AddRelation(new InventoryItem { type = ItemType.Coin });
    entity.AddRelation(new InventoryItem { type = ItemType.Axe  });
    entity.RemoveRelation <InventoryItem,ItemType>(ItemType.Coin);
    entity.GetRelations   <InventoryItem>();                        // O(1)
    entity.GetRelation    <InventoryItem,ItemType>(ItemType.Axe);   // O(1)
}

enum ItemType {
    Coin    = 1,
    Axe     = 2,
}

struct InventoryItem : IRelationComponent<ItemType> { // relation key type: ItemType
    public  ItemType    type;
    public  int         count;
    public  ItemType    GetRelationKey() => type;     // unique relation key
}

[Test]
public static void RelationComponents()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    
    // add multiple relations of the same component type
    entity.AddRelation(new InventoryItem { type = ItemType.Coin, count = 42 });
    entity.AddRelation(new InventoryItem { type = ItemType.Axe,  count =  3 });
    
    // Get all relations added to an entity.   O(1)
    entity.GetRelations  <InventoryItem>();                       // { Coin, Axe }
    
    // Get a specific relation from an entity. O(1)
    entity.GetRelation   <InventoryItem,ItemType>(ItemType.Coin); // {type=Coin, count=42}
    
    // Remove a specific relation from an entity
    entity.RemoveRelation<InventoryItem,ItemType>(ItemType.Axe);
    entity.GetRelations  <InventoryItem>();                       // { Coin }
}
#endregion


#region indexed component

struct Player : IIndexedComponent<string>       // indexed field type: string
{
    public  string  name;
    public  string  GetIndexedValue() => name;  // indexed field
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
    store.GetEntitiesWithComponentValue<Player,string>("Player-001");      // Count: 1
    
    // return same result as lookup using a Query(). O(1)
    store.Query().HasValue    <Player,string>("Player-001");               // Count: 1
    
    // return all entities with a Player.name in the given range.
    // O(N ⋅ log N) - N: all unique player names
    store.Query().ValueInRange<Player,string>("Player-000", "Player-099"); // Count: 100
    
    // get all unique Player.name's. O(1)
    store.GetAllIndexedComponentValues<Player,string>();                   // Count: 1000
}

#endregion
}

}
using System;
using Friflo.Engine.ECS;
using Tests.ECS;
using Tests.ECS.Index;
using Tests.ECS.Relations;

// [Testing Your Native AOT Applications - .NET Blog](https://devblogs.microsoft.com/dotnet/testing-your-native-aot-dotnet-apps/)
// > Parallelize() is ignored in NativeOAT unit tests  => tests run in parallel
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.ClassLevel)]

// ReSharper disable InconsistentNaming
namespace Tests.AOT.ECS {

[TestClass]
public class Test_AOT
{
    [TestMethod]
    public void Test_AOT_Setup()
    {
        Assert.IsTrue(true);
    }
    
    [TestMethod] // [DoNotParallelize]
    public void Test_All()
    {
        Test_AOT_Create_EntityStore();
        Test_AOT_AddComponent();
        Test_AOT_AddTag();
        Test_AOT_AddScript();
    }
    
    [TestMethod]
    public void Test_AOT_Create_Schema()
    {
        var schema = CreateSchema();
        var dependants = schema.EngineDependants;
        //Assert.AreEqual(2, dependants.Length);
        var engine = dependants[0];
        var test   = dependants[1];
        // Assert.AreEqual("Friflo.Engine.ECS",    engine.AssemblyName);
        // Assert.AreEqual(9,                      engine.Types.Length);
        // Assert.AreEqual("Tests",                test.AssemblyName);
    }

	[TestMethod]
	public void Test_AOT_Create_EntityStore()
	{
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        Assert.AreEqual(1, entity.Id);
	}
    
    [TestMethod]
    public void Test_AOT_AddComponent()
    {
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddComponent(new Position(1,2,3));
    }
    
    [TestMethod]
    public void Test_AOT_AddTag()
    {
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddTag<TestTag>();
    }
    
    [TestMethod]
    public void Test_AOT_AddScript()
    {
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddScript(new TestScript1());
    }
    
    [TestMethod]
    public void Test_AOT_AddComponent_unknown()
    {
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        Assert.ThrowsException<TypeInitializationException>(() => {
            entity.AddComponent(new MyComponent2());    
        });
    }
#region IIndexedComponent<>
    [TestMethod]
    public void Test_AOT_IndexedComponents_class()
    {
        CreateSchema();
        var store   = new EntityStore();
        
        var index   = store.ComponentIndex<Player,string>();
        for (int n = 0; n < 1000; n++) {
            var entity = store.CreateEntity();
            entity.AddComponent(new Player { name = $"Player-{n,0:000}"});
        }
        // get all entities where Player.name == "Player-001". O(1)
        var entities = index["Player-001"];                                    // Count: 1
    
        // return same result as lookup using a Query(). O(1)
        store.Query().HasValue    <Player,string>("Player-001");               // Count: 1
    
        // return all entities with a Player.name in the given range.
        // O(N ⋅ log N) - N: all unique player names
        store.Query().ValueInRange<Player,string>("Player-000", "Player-099"); // Count: 100
    
        // get all unique Player.name's. O(1)
        var values = index.Values;                                             // Count: 1000
    }
    
    [TestMethod]
    public void Test_AOT_IndexedComponents_struct()
    {
        CreateSchema();
        var store   = new EntityStore();
        var index   = store.ComponentIndex<IndexedInt,int>();
        for (int n = 0; n < 1000; n++) {
            var entity = store.CreateEntity();
            entity.AddComponent(new IndexedInt { value = n });
        }
        // get all entities where IndexedInt.value == 1
        var entities = index[1];                                                // Count: 1
    
        // return same result as lookup using a Query(). O(1)
        store.Query().HasValue    <IndexedInt,int>(1);                          // Count: 1
    
        // return all entities with a Player.name in the given range.
        // O(N ⋅ log N) - N: all unique player names
        store.Query().ValueInRange<IndexedInt,int>(0, 99);  // Count: 100
    
        // get all unique IndexedInt.value's. O(1)
        var values = index.Values;                                             // Count: 1000
    }
    
    [TestMethod]
    public void Test_AOT_LinkComponents()
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

#region IRelation<>
    [TestMethod]
    public void Test_AOT_LinkRelations()
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
    
    [TestMethod]
    public void Test_AOT_Relations()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
    
        // add multiple relations of the same component type
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Gun, amount = 42 });
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Axe,  amount =  3 });
    
        // Get all relations added to an entity.   O(1)
        entity.GetRelations  <InventoryItem>();                       // { Coin, Axe }
    
        // Get a specific relation from an entity. O(1)
        entity.GetRelation   <InventoryItem,InventoryItemType>(InventoryItemType.Gun); // {type=Coin, count=42}
    
        // Remove a specific relation from an entity
        entity.RemoveRelation<InventoryItem,InventoryItemType>(InventoryItemType.Axe);
        entity.GetRelations  <InventoryItem>();                       // { Coin }
    }
    #endregion
    
    struct Player : IIndexedComponent<string>       // indexed field type: string
    {
        public  string  name;
        public  string  GetIndexedValue() => name;  // indexed field
    }
    
    
    private static          EntitySchema    schemaCreated;
    private static readonly object          monitor = new object();
    
    private static EntitySchema CreateSchema()
    {
        // monitor required as tests are executed in parallel in MSTest
        lock (monitor)
        {
            Console.WriteLine("Test_AOT.CreateSchema() - 1");
            if (schemaCreated != null) {
                return schemaCreated;
            }
            Console.WriteLine("Test_AOT.CreateSchema() - 2");
            var aot = new NativeAOT();
            
            aot.RegisterComponent<MyComponent1>();
            aot.RegisterComponent<MyComponent1>(); // register again

            aot.RegisterTag<TestTag>();
            aot.RegisterTag<TestTag>(); // register again
            
            aot.RegisterScript<TestScript1>();
            aot.RegisterScript<TestScript1>(); // register again
            
            aot.RegisterIndexedComponentClass<Player, string>();
            aot.RegisterIndexedComponentStruct<IndexedInt, int>();
            aot.RegisterIndexedComponentEntity<AttackComponent>();
            
            aot.RegisterRelation<InventoryItem, InventoryItemType>();
            aot.RegisterLinkRelation<AttackRelation>();
            
            return schemaCreated = aot.CreateSchema();
        }
    }

}
}


using System;
using Friflo.Engine.ECS;
using Tests.ECS;

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
        Assert.AreEqual(2, dependants.Length);
        var engine = dependants[0];
        var test   = dependants[1];
        Assert.AreEqual("Friflo.Engine.ECS",    engine.AssemblyName);
        Assert.AreEqual(9,                      engine.Types.Length);
        Assert.AreEqual("Tests",                test.AssemblyName);
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
            
            return schemaCreated = aot.CreateSchema();
        }
    }

}
}


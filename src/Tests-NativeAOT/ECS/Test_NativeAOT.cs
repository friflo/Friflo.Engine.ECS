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
    
    private static          bool    schemaCreated;
    private static readonly object  monitor = new object();
    
    private static void CreateSchema()
    {
        // monitor required as tests are executed in parallel in MSTest
        lock (monitor)
        {
            Console.WriteLine("Test_AOT.CreateSchema() - 1");
            if (schemaCreated) {
                return;
            }
            Console.WriteLine("Test_AOT.CreateSchema() - 2");
            schemaCreated = true;
            var aot = new NativeAOT();
            aot.RegisterComponent<MyComponent1>();
            aot.RegisterTag<TestTag>();
            aot.RegisterScript<TestScript1>();
            aot.CreateSchema();
        }
    }

}
}


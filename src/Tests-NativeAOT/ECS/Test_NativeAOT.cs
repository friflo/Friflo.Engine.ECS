using System;
using Friflo.Engine.ECS;
using Tests.ECS;


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
    
    [TestMethod]
    public void Test_All()
    {
        Test_AOT_Create_EntityStore();
        Test_AOT_AddComponent();
        Test_AOT_AddTag();
        Test_AOT_AddScript();
    }

	// [TestMethod]
	public void Test_AOT_Create_EntityStore()
	{
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        Assert.AreEqual(1, entity.Id);
	}
    
    // [TestMethod]
    public void Test_AOT_AddComponent()
    {
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddComponent(new Position(1,2,3));
    }
    
    // [TestMethod]
    public void Test_AOT_AddTag()
    {
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddTag<TestTag>();
    }
    
    // [TestMethod]
    public void Test_AOT_AddScript()
    {
        CreateSchema();
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddScript(new TestScript1());
    }
    
    private static bool schemaCreated;
    
    private static void CreateSchema()
    {
        Console.WriteLine("Test_AOT.CreateSchema()");
        if (schemaCreated) {
            return;
        }
        schemaCreated = true;
        NativeAOT.RegisterComponent<MyComponent1>();
        NativeAOT.RegisterTag<TestTag>();
        NativeAOT.RegisterScript<TestScript1>();
        NativeAOT.CreateSchema();
    }

}
}


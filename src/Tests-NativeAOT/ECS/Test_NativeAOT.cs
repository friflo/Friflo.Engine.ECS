using Friflo.Engine.ECS;


// ReSharper disable InconsistentNaming
namespace NativeAOT.ECS {

[TestClass]
public class Test_NativeAOT
{
    [TestMethod]
    public void Test_NativeAOT_Setup()
    {
        Assert.IsTrue(true);
    }

	[TestMethod]
	public void Test_NativeAOT_Create_EntityStore()
	{
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        Assert.AreEqual(1, entity.Id);
	}
    
    [TestMethod]
    public void Test_NativeAOT_AddComponent()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddComponent(new Position(1,2,3));
    }

}
}


using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Relations {


public static class Test_Relations_Delete
{
    [Test]
    public static void Test_Relations_Delete_Int()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        
        entity1.AddComponent(new IntRelation { value = 10 });
        entity1.AddComponent(new IntRelation { value = 20 });
        
        entity2.AddComponent(new IntRelation { value = 30 });
        
        entity1.DeleteEntity();
        
        entity2.DeleteEntity();
    }
}

}

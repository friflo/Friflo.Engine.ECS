using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable once InconsistentNaming
namespace Internal.ECS;

public static class Test_QueryJob
{
    [Test]
    public static void Test_QueryJob_ToString()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < 32; n++) {
            archetype.CreateEntity();
        }
        
        var query = store.Query<MyComponent1>();
        
        var job = query.ForEach((_, _) => { });
        
        Assert.AreEqual(32, job.Chunks.EntityCount);
        Assert.AreEqual("QueryJob [MyComponent1]", job.ToString());
    }
}
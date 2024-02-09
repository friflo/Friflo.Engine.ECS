using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

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
        
        AreEqual(32, job.Chunks.EntityCount);
        AreEqual("QueryJob [MyComponent1]", job.ToString());
    }

    [Test]
    public static void Test_QueryJob_LeastCommonMultiple()
    {
        AreEqual(  0, QueryJob.LeastCommonMultiple( 0,  1));
        AreEqual(  0, QueryJob.LeastCommonMultiple( 1,  0));
        
        AreEqual(  1, QueryJob.LeastCommonMultiple( 1,  1));
        AreEqual(  2, QueryJob.LeastCommonMultiple( 1,  2));
        AreEqual(  6, QueryJob.LeastCommonMultiple( 2,  3));
        
        AreEqual(192, QueryJob.LeastCommonMultiple(12, 64));
        AreEqual(320, QueryJob.LeastCommonMultiple(20, 64));
        AreEqual(192, QueryJob.LeastCommonMultiple(24, 64));
        AreEqual(448, QueryJob.LeastCommonMultiple(28, 64));
        AreEqual(320, QueryJob.LeastCommonMultiple(40, 64));
        AreEqual(192, QueryJob.LeastCommonMultiple(48, 64));
        AreEqual(448, QueryJob.LeastCommonMultiple(56, 64));
    }
}
using System;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;

// ReSharper disable UnusedVariable
// ReSharper disable RedundantLambdaParameterType
// ReSharper disable UnusedParameter.Local
// ReSharper disable once InconsistentNaming
namespace Internal.ECS;

public static class Test_QueryJob
{
    [Test]
    public static void Test_QueryJob_Run()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < 32; n++) {
            archetype.CreateEntity();
        }
        
        ArchetypeQuery<MyComponent1> query = store.Query<MyComponent1>();
        
        // use same interface by ForEach() as in foreach loop
        foreach (var (component1, entities) in query.Chunks) { }
        foreach ((Chunk<MyComponent1> component1, ChunkEntities entities) in query.Chunks) { }
        
        var job = query.ForEach((component1, entities) => { });
        job.Run();
        
        job = query.ForEach((Chunk<MyComponent1> component1, ChunkEntities entities) => { });
        job.Run();
    }
    
    // [Test]
    public static void Test_QueryJob_RunParallel()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < 32; n++) {
            archetype.CreateEntity();
        }
        
        ArchetypeQuery<MyComponent1> query = store.Query<MyComponent1>();
        
        var job = query.ForEach((component1, entities) => { });
        job.RunParallel();
        
        job = query.ForEach((Chunk<MyComponent1> component1, ChunkEntities entities) => { });
        job.RunParallel();
    }

    /// all TPL <see cref="Parallel"/> methods allocate memory. SO they are out.
    [Test]
    public static void Test_QueryJob_TPL()
    {
        // --- Parallel.ForEach()
        var array = new int[100];
        var start = Mem.GetAllocatedBytes();
        Parallel.ForEach(array, value => { _ = value; });
        Assert.IsTrue(Mem.GetAllocatedBytes() - start > 0);
        
        // --- Parallel.For()
        start = Mem.GetAllocatedBytes();
        Parallel.For(0, 10, i => {});
        Assert.IsTrue(Mem.GetAllocatedBytes() - start > 0);
        
        // --- Parallel.Invoke()
        var actions = new Action[4];
        actions[0] = () => {};
        actions[1] = () => {};
        actions[2] = () => {};
        actions[3] = () => {};
        start = Mem.GetAllocatedBytes();
        Parallel.Invoke(actions);
        Assert.IsTrue(Mem.GetAllocatedBytes() - start > 0);
    }
}
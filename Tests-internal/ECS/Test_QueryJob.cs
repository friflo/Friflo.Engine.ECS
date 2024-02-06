using System;
using System.Diagnostics;
using System.Threading;
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
        
        // --- use same interface by ForEach() as in foreach loop
        foreach (var  (component1, entities) in query.Chunks) { }
        query.ForEach((component1, entities) => { });
        
        foreach      ((Chunk<MyComponent1> component1, ChunkEntities entities) in query.Chunks) { }
        query.ForEach((Chunk<MyComponent1> component1, ChunkEntities entities) => { });
        
        // --- execute ForEach() synchronously
        var job = query.ForEach((component1, entities) => { });
        job.Run();
        job.Run();
    }
    
    // [Test]
    public static void Test_QueryJob_RunParallel()
    {
        long count       = 10;      // 100_000;
        long entityCount = 10_000;  // 100_000;
        int  threadCount = 2;
        
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < entityCount; n++) {
            archetype.CreateEntity();
        }
        
        var query = store.Query<MyComponent1>();
        var forEachCount    = 0;
        var lengthSum       = 0L;
        
        var job = query.ForEach((component1, entities) =>
        {
            Interlocked.Increment(ref forEachCount);
            Interlocked.Add(ref lengthSum, entities.Length);
            var componentSpan = component1.Span;
            foreach (ref var c in componentSpan) {
                ++c.a;
            }
        });
        job.JobRunner               = new ParallelJobRunner(threadCount);
        job.MinParallelChunkLength  = 1000;
        job.RunParallel();  // force one time allocations

        var sw      = new Stopwatch();
        var start   = Mem.GetAllocatedBytes();
        sw.Start();
        for (int n = 1; n < count; n++) {
            job.RunParallel();
        }
        Mem.AssertNoAlloc(start);
        var duration = sw.ElapsedMilliseconds;
        Console.Write($"JobQuery.RunParallel() - entities: {entityCount}, threads: {threadCount}, count: {count}, duration: {duration}" );
        
        Assert.AreEqual(threadCount * count, forEachCount);
        Assert.AreEqual(entityCount * count, lengthSum);
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
    
    [Test]
    public static void Test_QueryJob_ToString()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < 32; n++) {
            archetype.CreateEntity();
        }
        
        ArchetypeQuery<MyComponent1> query = store.Query<MyComponent1>();
        
        var job = query.ForEach((component1, entities) => { });
        
        Assert.AreEqual(32, job.Chunks.EntityCount);
        Assert.AreEqual("QueryChunks[1]  Components: [MyComponent1]", job.ToString());
    }
}
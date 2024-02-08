using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;

// ReSharper disable AccessToModifiedClosure
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
        
        // --- execute ForEach() single threaded
        int taskCount = 0;
        var job = query.ForEach((component1, entities) => taskCount++);
        job.Run();
        
        using var runner = new ParallelJobRunner(1);  
        job.JobRunner = runner;
        job.RunParallel();
        
        Assert.AreEqual(2, taskCount);
    }
    
    [Test]
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
        var runner = new ParallelJobRunner(threadCount);
        job.JobRunner               = runner;
        job.MinParallelChunkLength  = 1000;
        job.RunParallel();  // force one time allocations

        var start   = Mem.GetAllocatedBytes();
        Mem.AssertNoAlloc(start);
        job.RunParallel();
        
        var sw      = new Stopwatch();
        sw.Start();
        for (int n = 2; n < count; n++) {
            job.RunParallel(); // allocate occasionally 24 byte for the entire loop in DEBUG
        }
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
        
        var query = store.Query<MyComponent1>();
        
        var job = query.ForEach((component1, entities) => { });
        
        Assert.AreEqual(32, job.Chunks.EntityCount);
        Assert.AreEqual("QueryJob [MyComponent1]", job.ToString());
    }
    
    [Test]
    public static void Test_QueryJob_task_exceptions()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < 10; n++) {
            archetype.CreateEntity();
        }
        
        var query = store.Query<MyComponent1>();
        
        var job     = query.ForEach((component1, entities) => throw new InvalidOperationException("test exception"));
        job.MinParallelChunkLength = 10;
        
        using var runner  = new ParallelJobRunner(2);
        Assert.AreEqual(2, runner.ThreadCount);
        job.JobRunner = runner;
        Assert.AreSame(runner, job.JobRunner);
        
        var e   = Assert.Throws<AggregateException>(() => {
            job.RunParallel();
        });
        Assert.AreEqual(2, e!.InnerExceptions.Count);
        Assert.AreEqual("QueryJob [MyComponent1] - 2 task exceptions. (test exception) (test exception)", e!.Message);
        
        var e2  = Assert.Throws<InvalidOperationException>(() => {
            job.Run();
        });
        Assert.AreEqual("test exception", e2!.Message);
    }
    
    [Test]
    public static void Test_QueryJob_QueryJob_exceptions()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var query   = store.Query<MyComponent1>();
        var job     = query.ForEach((_,_) => {});
        
        var e1 = Assert.Throws<InvalidOperationException>(() => {
            job.RunParallel();
        });
        Assert.AreEqual("QueryJob requires a JobRunner", e1!.Message);
        
        job.MinParallelChunkLength = 10000;
        Assert.AreEqual(10000, job.MinParallelChunkLength);
        
        var e2 = Assert.Throws<ArgumentException>(() => {
            job.MinParallelChunkLength = 0;
        });
        Assert.AreEqual("MinParallelChunkLength must be > 0", e2!.Message);
        
        var e3 = Assert.Throws<ArgumentNullException>(() => {
            job.JobRunner = null;
        });
        Assert.AreEqual("Value cannot be null. (Parameter 'jobRunner')", e3!.Message);
    }
    
    [Test]
    public static void Test_QueryJob_nested_ForEach()
    {
        Thread.CurrentThread.Name = "MainThread";
        using var runner    = new ParallelJobRunner(2, "JobRunner");
        var store           = new EntityStore(PidType.UsePidAsId) { JobRunner = runner };
        store.CreateEntity().AddComponent<MyComponent1>();
        
        var count       = 0;
        var query       = store.Query<MyComponent1>();
        var nestedJob   = query.ForEach((_, _) => {
            Interlocked.Increment(ref count);
        });
        nestedJob.MinParallelChunkLength = 1;
        var enclosingJob = query.ForEach((_,_) => {
            nestedJob.RunParallel(); // throws '... is already used by ...' exception if using the same job runner
        });
        Assert.AreEqual(1000, enclosingJob.MinParallelChunkLength);
        
        // --- error case
        enclosingJob.MinParallelChunkLength = 1;
        var e = Assert.Throws<AggregateException>(enclosingJob.RunParallel)!;
        
        var exceptions =  e.InnerExceptions;
        Assert.AreEqual(2, exceptions.Count);
        Assert.AreEqual("'JobRunner' is already used by <- QueryJob [MyComponent1]", exceptions[0].Message);
        Assert.AreEqual("'JobRunner' is already used by <- QueryJob [MyComponent1]", exceptions[1].Message);
        
        // --- happy case
        count = 0;
        using var runner2 = new ParallelJobRunner(2, "JobRunner 2");
        nestedJob.JobRunner = runner2;  // assign a separate job runner
        enclosingJob.RunParallel();
        Assert.AreEqual(4, count);
    }
    
    [Test]
    public static void Test_QueryJob_multi_thread_JobRunner()
    {
        var threads = 4; 
        using var runner    = new ParallelJobRunner(threads, "TestRunner");
        var store           = new EntityStore(PidType.UsePidAsId) { JobRunner = runner };
        for (int n = 0; n < 10; n++) {
            var entity  = store.CreateEntity();
            entity.AddComponent<MyComponent1>();
            entity.AddComponent<MyComponent2>();
        }
        var query1  = store.Query<MyComponent1>();
        var query2  = store.Query<MyComponent2>();
        var count1  = 0;
        var count2  = 0;
        var job1    = query1.ForEach((_,_) =>  Interlocked.Increment(ref count1)); 
        var job2    = query2.ForEach((_,_) =>  Interlocked.Increment(ref count2));
        job1.MinParallelChunkLength = 1;
        job2.MinParallelChunkLength = 1;
        
        var jobs    = new QueryJob [] { job1, job2 };
        Parallel.ForEach(jobs, job =>
        {
            for (int n = 0; n < 1000; n++) {
                job.RunParallel();
            }
        });
        Console.WriteLine($"{count1} {count2}");
        Assert.AreEqual(threads * 1000, count1);
        Assert.AreEqual(threads * 1000, count2);
    }
    
    [Test]
    public static void Test_QueryJob_EntityStore_JobRunner()
    {
        using var jobRunner = new ParallelJobRunner(2, "MyRunner");
        Assert.AreEqual("MyRunner - threads: 2", jobRunner.ToString());
        
        var store       = new EntityStore(PidType.UsePidAsId) {
            JobRunner = jobRunner   // attach JobRunner to EntityStore
        };
        store.CreateEntity().AddComponent<MyComponent1>();
        var query       = store.Query<MyComponent1>();
        int count       = 0;
        Thread.CurrentThread.Name = "MainThread";
        bool foundWorkerName = false;
        bool foundCallerName = false;
        var job         = query.ForEach((_, entities) => {
            count++;
            switch (Thread.CurrentThread.Name) {
                case "MyRunner - worker 1": foundWorkerName = true; break;
                case "MainThread":          foundCallerName = true; break;
            }
        });
        Assert.AreEqual(1000, job.MinParallelChunkLength);
        job.MinParallelChunkLength = 1;
        job.RunParallel();  // uses JobRunner from EntityStore
        
        Assert.AreEqual(2, count);
        Assert.IsTrue(foundWorkerName, "worker thread name not found");
        Assert.IsTrue(foundCallerName, "caller thread name not found");
    }
    
    [Test]
    public static void Test_QueryJob_QueryJobDispose()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < 10; n++) {
            archetype.CreateEntity();
        }
        var runner  = new ParallelJobRunner(4);
        var query   = store.Query<MyComponent1>();
        var job     = query.ForEach((_,_) => {});
        job.MinParallelChunkLength  = 10;
        job.JobRunner               = runner;
        
        job.RunParallel();
        
        runner.Dispose();
        var e = Assert.Throws<ObjectDisposedException>(() => {
            job.RunParallel();    
        });
        // no check of e.Message. uses different line ending \r\n \n on different platforms
        
        var e2 = Assert.Throws<ArgumentException>(() => {
            job.JobRunner = runner;
        });
        Assert.AreEqual("ParallelJobRunner is disposed", e2!.Message);
    }
}
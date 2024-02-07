// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using Friflo.Engine.ECS;

// ReSharper disable NotAccessedField.Local
namespace GameTest;

public static class ParallelQuery
{
    internal static void Query_ForEach(string[] args)
    {
        int  threadCount    = 8;
        long count          = 100_000;      // 100_000;
        long entityCount    = 100_000;  // 100_000;
        int  loop           = 10;
        if (args.Length > 0) {
            threadCount = Int32.Parse(args[0]);
        }
        Console.WriteLine($"threadCount: {threadCount}");
        var store       = new EntityStore(PidType.UsePidAsId);
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < entityCount; n++) {
            archetype.CreateEntity();
        }
        
        var query = store.Query<MyComponent1>();
        var forEachCount    = 0;
        var lengthSum       = 0L;
        var runner          = new ParallelJobRunner(threadCount);
        
        var job = query.ForEach((_, _) => {});
        job.JobRunner = runner;
        job.RunParallel();
        // Thread.Sleep(10_000); // ensure all threads are idle during Sleep()
        
        job = query.ForEach((component1, entities) =>
        {
            Interlocked.Increment(ref forEachCount);
            Interlocked.Add(ref lengthSum, entities.Length);
            var componentSpan = component1.Span;
            foreach (ref var c in componentSpan) {
                ++c.a;
            }
        });
        job.JobRunner               = runner;
        job.MinParallelChunkLength  = 1000;

        long log = count / 5;
        for (int i = 0; i < loop; i++) {
            var sw      = new Stopwatch();
            sw.Start();
            for (int n = 0; n < count; n++) {
                if (n % log == 0) Console.WriteLine(n);
                job.RunParallel();
            }
            var duration = sw.ElapsedMilliseconds;
            Console.WriteLine($"RunParallel() - entities: {entityCount}, threads: {threadCount}, count: {count}, duration: {duration} ms");
        }
        
        Console.WriteLine($"forEachCount: {forEachCount}, lengthSum: {lengthSum}" );
        Console.WriteLine($"expect:       {loop * threadCount * count}             {loop * entityCount * count}" );
        // Assert.AreEqual(threadCount * count, forEachCount);
        // Assert.AreEqual(entityCount * count, lengthSum);
        runner.Dispose();
    }

    private struct MyComponent1 : IComponent { public int a; }
}
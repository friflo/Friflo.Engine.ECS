// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using Friflo.Engine.ECS;

namespace GameTest;

public static class ParallelQuery
{
    internal static void Query_ForEach(string[] args)
    {
        int  threadCount = 8;
        long count       = 100_000;      // 100_000;
        long entityCount = 100_000;  // 100_000;
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

        long log = count / 5;
        for (int n = 1; n < count; n++) {
            if (n % log == 0) Console.WriteLine(n);
            job.RunParallel();
        }
        // Thread.Sleep(4000);
        
        var sw      = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            if (n % log == 0) Console.WriteLine(n);
            job.RunParallel();
        }
        var duration = sw.ElapsedMilliseconds;
        Console.WriteLine($"RunParallel() - entities: {entityCount}, threads: {threadCount}, count: {count}, duration: {duration} ms");
        
        Console.WriteLine($"forEachCount: {forEachCount}, lengthSum: {lengthSum}" );
        Console.WriteLine($"expect:       {2 * threadCount * count}             {2 * entityCount * count}" );
        // Assert.AreEqual(threadCount * count, forEachCount);
        // Assert.AreEqual(entityCount * count, lengthSum);
    }

    private struct MyComponent1 : IComponent { public int a; }
}
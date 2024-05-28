using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

#if !UNITY_5_3_OR_NEWER
using System.Runtime.Intrinsics;
#endif

// ReSharper disable ConvertToConstant.Local
namespace Tests.ECS.Systems {

// ReSharper disable InconsistentNaming
public static class Bench_Query
{
    private static readonly int     entityCount = 32;   // 32 /   100_000
    private static readonly int     JitLoop     = 10;   // 10 / 5_000_000
    private static readonly bool    runParallel = false;
    
    [Test]
    public static void Test_BenchRef()
    {
        var components = new MyComponent1[32];
        // --- enable JIT optimization
        for (long i = 0; i < JitLoop; i++) {
            Bench_Reference(components);
        }
        
        // --- run perf
        // 1000 ~ #PC: 42,1 ms - component: int,   42,1 - component: byte
        components = new MyComponent1[entityCount];
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (long i = 0; i < 1000; i++) {
            Bench_Reference(components);
        }
        Console.WriteLine($"Iterate - array: {TestUtils.StopwatchMillis(stopwatch)} ms");
    }
    
    private static void Bench_Reference(MyComponent1[] components) {
        Span<MyComponent1> comps = components;
        for (int n = 0; n < comps.Length; n++) {
            ++comps[n].a;
        }
    }
    
    [Test]
    public static void Test_Bench()
    {
        using var runner = new ParallelJobRunner(8);
        var store   = new EntityStore() { JobRunner = runner };
        var archetype = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 0; n < 32; n++) {
            archetype.CreateEntity();
        }
        
        // --- enable JIT optimization
        var  query = store.Query<MyComponent1>();
        for (int i = 0; i < JitLoop; i++) {
            Bench_SIMD(query, runParallel);
            bench     (query, runParallel);
        }
        
        for (int n = 0; n < entityCount - 32; n++) {
            archetype.CreateEntity();
        }
        // --- run perf
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            // 1000 ~ #PC: 42,1 ms - component: int,   42,1 ms - component: byte
            bench(query, runParallel);
        }
        Console.WriteLine($"Iterate - Span<MyComponent1>: {TestUtils.StopwatchMillis(stopwatch)} ms");
        

        stopwatch = new Stopwatch(); stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            // 1000 ~ #PC: 14,7 ms - component: int,   2,6 ms - component: byte
            Bench_SIMD(query, runParallel);
        }
        Console.WriteLine($"Iterate - SIMD: {TestUtils.StopwatchMillis(stopwatch)} ms");
    }
    
    private static void bench(ArchetypeQuery<MyComponent1> query, bool runParallel)
    {
        if (runParallel) {
            var job = query.ForEach((component, _) => {
                foreach (ref var value in  component.Span) {
                    ++value.a;
                }
            });
            job.RunParallel();
            return;
        }
        foreach (var (component, _) in query.Chunks)
        {
            foreach (ref var value in  component.Span) {
                ++value.a;
            }
        }
    }
    
    private static void Bench_SIMD(ArchetypeQuery<MyComponent1> query, bool runParallel)
    {
        if (runParallel) {
            var job = query.ForEach((component, _) => {
                SIMD_Add(component);
            });
            job.RunParallel();
            return;
        }
        foreach (var (component, _) in query.Chunks) {
            SIMD_Add(component);
        }
    }

    private static void SIMD_Add(Chunk<MyComponent1> component) {
#if UNITY_5_3_OR_NEWER
    }
#else
        // Requires .NET 8 to enable SIMD on ARM (Apple Silicon).
        // See: [What's new in .NET 8] https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8#systemnumerics-and-systemruntimeintrinsics
        var add     = Vector256.Create<int>(1);         // create byte[32] vector - all values = 1
        var bytes   = component.AsSpan256<int>();       // bytes.Length - multiple of 32
        var step    = component.StepSpan256;            // step = 32
        for (int n = 0; n < bytes.Length; n += step) {
            var slice   = bytes.Slice(n, step);
            var result = Vector256.Create<int>(slice) + add; // execute 32 add instructions at once
            result.CopyTo(slice);
        }
    }
    
    /// <summary> Alternative to create <see cref="Vector256{T}"/> with custom values </summary>
    // ReSharper disable once UnusedMember.Local
    private static Vector256<byte> CreateVector256_Alternative()
    {
        Span<byte> oneBytes = stackalloc byte[32] {1,1,1,1,1,1,1,1,  2,2,2,2,2,2,2,2,  3,3,3,3,3,3,3,3,  4,4,4,4,4,4,4,4};
        return Vector256.Create<byte>(oneBytes);
    }
#endif
}

}
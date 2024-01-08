using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable ConvertToConstant.Local
namespace Tests.ECS.System;

// ReSharper disable InconsistentNaming
public static class Bench_Query
{
    private static readonly int entityCount     = 32;   //   100_000
    private static readonly int JitLoop         = 10;   // 5_000_000
    
    [Test]
    public static void Test_BenchRef()
    {
        var components = new ByteComponent[32];
        // --- enable JIT optimization
        for (long i = 0; i < JitLoop; i++) {
            bench_ref(components);
        }
        
        // --- run perf
        // 1000 ~ 42 ms
        components = new ByteComponent[entityCount];
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (long i = 0; i < 1000; i++) {
            bench_ref(components);
        }
        Console.WriteLine($"Iterate - array: {TestUtils.StopwatchMillis(stopwatch)} ms");
    }
    
    private static void bench_ref(ByteComponent[] components) {
        Span<ByteComponent> comps = components;
        for (int n = 0; n < comps.Length; n++) {
            ++comps[n].b;
        }
    }
    
    [Test]
    public static void Test_Bench()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var archetype = store.GetArchetype(Signature.Get<ByteComponent>());
        for (int n = 0; n < 32; n++) {
            store.CreateEntity(archetype);
        }
        
        var inc = CreateInc();
        // --- enable JIT optimization
        var  query = store.Query<ByteComponent>();
        for (int i = 0; i < JitLoop; i++) {
            bench_simd(query, inc);
            bench(query);
        }
        
        for (int n = 0; n < entityCount - 32; n++) {
            store.CreateEntity(archetype);
        }
        // --- run perf
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            // 1000 ~ 42 ms
            bench(query);
        }
        Console.WriteLine($"Iterate - Span<ByteComponent>: {TestUtils.StopwatchMillis(stopwatch)} ms");
        

        stopwatch = new Stopwatch(); stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            // 1000 ~ 2 ms
            bench_simd(query, inc);
        }
        Console.WriteLine($"Iterate - SIMD: {TestUtils.StopwatchMillis(stopwatch)} ms");
    }
    
    private static void bench(ArchetypeQuery<ByteComponent> query)
    {
        foreach (var (component, _) in query.Chunks)
        {
            for (int n = 0; n < component.Length; n++) {
                ++component[n].b;
            }
        }
    }
    
    private static Vector256<byte> CreateInc()
    {
        Span<byte> oneBytes = stackalloc byte[32];
        for (int n = 0; n < 32; n++) {
            oneBytes[n] = 1;
        }
        return Vector256.Create<byte>(oneBytes);
    }
    
    private static void bench_simd(ArchetypeQuery<ByteComponent> query, Vector256<byte> add)
    {
        foreach (var (component, _) in query.Chunks)
        {
            var bytes   = component.SpanByte;
            var step    = component.StepVector256;
            for (int n = 0; n < bytes.Length; n += step) {
                var slice   = bytes.Slice(n, step);
                var value   = Vector256.Create<byte>(slice);
                var result  = Vector256.Add(value, add);
                result.CopyTo(slice);
            }
        }
    }
}
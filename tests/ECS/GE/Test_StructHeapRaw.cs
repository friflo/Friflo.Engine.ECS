using System;
using System.Diagnostics;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;

public static class Test_StructHeapRaw
{
    [Test]
    public static void Test_StructHeapRaw_increase_entity_capacity()
    {
        var store       = new RawEntityStore();
        var arch        = store.GetArchetype(Signature.Get<Position>());
        int count       = 16384; // 16384 ~ 6 ms    8388608 ~ 384 ms
        var ids         = new int[count];
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int n = 0; n < count; n++)
        {
            var id  = store.CreateEntity(arch);
            ids[n]  = id;
            if (arch != store.GetEntityArchetype(id)) {
                Fail($"unexpected Archetype. n: {n}");
            }
            if (n + 1 != arch.EntityCount) {
                Fail($"expect: ${n + 1}, was: {arch.EntityCount}, n: {n}");
            }
            ref var pos = ref store.EntityComponentRef<Position>(id);
            if (new Position() != pos) {
                Fail($"expect default value. n: {n}");
            }
            pos.x = n;
        }
        Console.WriteLine($"CreateEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        AreEqual(count, arch.Capacity);
        for (int n = 0; n < count; n++) {
            ref var val = ref store.EntityComponentRef<Position>(ids[n]);
            if (n != (int)val.x) {
                Fail($"expect: {n}, was: {val.x}, n: {n}");
            }
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_shrink_entity_capacity()
    {
        var store       = new RawEntityStore();
        var arch        = store.GetArchetype(Signature.Get<Position>());
        int count       = 16384; // 16384 ~ 0-1 ms     8388608 ~ 98 ms
        var ids         = new int[count];
        for (int n = 0; n < count; n++)
        {
            var id = store.CreateEntity(arch);
            ids[n] = id;
            store.EntityComponentRef<Position>(id).x = n;
        }
        // --- delete majority of entities
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        const int remaining = 500;
        for (int n = remaining; n < count; n++)
        {
            store.DeleteEntity(ids[n]);
            var expectCount = count + remaining - n - 1;
            if (expectCount != arch.EntityCount) {
                Fail($"expect Archetype.EntityCount: ${expectCount}, was: {arch.EntityCount}, n: {n}");
            }
        }
        Console.WriteLine($"DeleteEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        AreEqual(1024, arch.Capacity);
        for (int n = 0; n < remaining; n++) {
            AreEqual(n, store.EntityComponentRef<Position>(ids[n]).x);
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_CreateEntity_Perf()
    {
        var store   = new RawEntityStore();
        var arch1   = store.GetArchetype(Signature.Get<Position>());
        _ = store.CreateEntity(arch1); // warmup
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int count       = 10; // 10_000_000 ~ 336 ms
        for (int n = 0; n < count; n++) {
            _ = store.CreateEntity(arch1);
        }
        Console.WriteLine($"CreateEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        AreEqual(count + 1, arch1.EntityCount);
    }
    
    [Test]
    public static void Test_StructHeapRaw_DeleteEntity_Perf()
    {
        var store   = new RawEntityStore();
        var arch1   = store.GetArchetype(Signature.Get<Position>());
        int count   = 10; // 10_000_000 ~ 217 ms
        for (int n = 0; n < count; n++) {
            _ = store.CreateEntity(arch1);
        }
        AreEqual(count, arch1.EntityCount);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int n = 1; n <= count; n++) {
            store.DeleteEntity(n);
        }
        Console.WriteLine($"DeleteEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        AreEqual(0, arch1.EntityCount);
    }
    
    [Test]
    public static void Test_StructHeapRaw_for_array_Reference()
    {
        var positions = new Position[Count];
        for (int n = 0; n < Count; n++) {
            positions[n].x = n;
        }
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var i = 0;
        // 10_000_000 ~ 9 ms
        for (int n = 0; n < Count; n++) {
            var x = (int)positions[n].x;
            if (x != n) throw new InvalidOperationException($"expect: {n}, was: {x}");
            i++;
        }
        AreEqual(Count, i);
        Console.WriteLine($"Iterate_Ref. count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms.");
    }
    
    /// user greater than <see cref="StructUtils.ChunkSize"/> for coverage
    private const int Count = 1000; // 10_000_000
    
    [Test]
    public static void Test_StructHeapRaw_Query_Perf()
    {
        var store   = new RawEntityStore();
        var arch1   = store.GetArchetype(Signature.Get<Position, Rotation>());
         // 10_000_000
        //      CreateEntity()          ~  498 ms
        //      foreach Query()         ~  145 ms
        //      Query.ForEach()         ~  114 ms
        //      foreach Query.Chunks    ~   10 ms
        var query   = store.Query(Signature.Get<Position, Rotation>());
        foreach (var (position, rotation) in query) { }     // force one time allocation
        query.ForEach((position, rotation) => {}).Run();    // warmup
        foreach (var _ in query.Chunks) { }                 // warmup
        {
            // _ = store.CreateEntity(arch1); // warmup
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int n = 0; n < Count; n++) {
                var id = store.CreateEntity(arch1);
                store.EntityComponentRef<Position>(id).x = n;
            }
            Console.WriteLine($"CreateEntity() - raw. count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms");
            AreEqual(Count, arch1.EntityCount);
        } {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int n           = 0;
            var memStart    = Mem.GetAllocatedBytes();
            foreach (var (position, rotation) in query) {
                 var x = (int)position.Value.x;
                if (x != n) throw new InvalidOperationException($"expect: {n}, was: {x}");
                n++;
            }
            Mem.AssertNoAlloc(memStart);
            AreEqual(Count, n);
            Console.WriteLine($"foreach Query(). count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        } {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int n           = 0;
            var memStart    = Mem.GetAllocatedBytes();
            var forEach     = query.ForEach((position, rotation) => {
                var x = (int)position.Value.x;
                if (x != n) throw new InvalidOperationException($"expect: {n}, was: {x}");
                n++;
            });
            forEach.Run();
            var diff = Mem.GetAllocatedBytes() - memStart;
            AreEqual(Count, n);
            Console.WriteLine($"Query.ForEach(). count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms.  Alloc: {diff}");
        } {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int n           = 0;
            var memStart    = Mem.GetAllocatedBytes();
            foreach (var (positionChunk, rotationChunk) in query.Chunks) {
                foreach (var position in positionChunk.Values) {
                    var x = (int)position.x;
                    if (x != n) throw new InvalidOperationException($"expect: {n}, was: {x}");
                    n++;
                }
            }
            var diff = Mem.GetAllocatedBytes() - memStart;
            AreEqual(Count, n);
            Console.WriteLine($"foreach Query.Chunks. count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms.  Alloc: {diff}");
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_invalid_store()
    {
        var store1      = new RawEntityStore();
        var store2      = new RawEntityStore();
        var arch1       = store1.GetArchetype(Signature.Get<Position>());
        var e = Throws<ArgumentException>(() => {
            store2.CreateEntity(arch1);
        });
        AreEqual("entity is owned by a different store (Parameter 'archetype')", e!.Message);
    }
}


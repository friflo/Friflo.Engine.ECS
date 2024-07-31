using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;


// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_StructHeapRaw
{
    [Test]
    public static void Test_StructHeapRaw_increase_entity_capacity()
    {
        var store       = new RawEntityStore();
        var arch        = store.GetArchetype(ComponentTypes.Get<Position>());
        int count       = 16384; // 16384 ~ #PC: 4 ms    8388608 ~ 372 ms
        var ids         = new int[count];
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int n = 0; n < count; n++)
        {
            var id  = store.CreateEntity(arch);
            ids[n]  = id;
            var entityArch = store.GetEntityArchetype(id);
            if (arch != entityArch)             Mem.AreSame(arch, entityArch);
            if (n + 1 != arch.Count)      Mem.FailAreEqual(n + 1, arch.Count);
            ref var pos = ref store.GetEntityComponent<Position>(id);
            if (default != pos)                 Mem.FailAreEqual(default, pos);
            pos.x = n;
        }
        Console.WriteLine($"CreateEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        Mem.AreEqual(count, arch.Capacity);
        for (int n = 0; n < count; n++) {
            var x = (int)store.GetEntityComponent<Position>(ids[n]).x;
            if (n != x)                         Mem.FailAreEqual(n, x);
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_shrink_entity_capacity()
    {
        var store       = new RawEntityStore { ShrinkRatioThreshold = 0 };
        var arch        = store.GetArchetype(ComponentTypes.Get<Position>());
        int count       = 16384; // 16384 ~ #PC: 0-1 ms     8388608 ~ 190 ms
        var ids         = new int[count];
        for (int n = 0; n < count; n++)
        {
            var id = store.CreateEntity(arch);
            ids[n] = id;
            store.GetEntityComponent<Position>(id).x = n;
        }
        // --- delete majority of entities
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        const int remaining = 500;
        for (int n = remaining; n < count; n++)
        {
            Mem.IsTrue(store.DeleteEntity(ids[n]));
            var expectCount = count + remaining - n - 1;
            if (expectCount != arch.Count)    Mem.FailAreEqual(expectCount, arch.Count);
        }
        Console.WriteLine($"DeleteEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        Mem.AreEqual(1024, arch.Capacity);
        for (int n = 0; n < remaining; n++) {
            Mem.AreEqual(n, store.GetEntityComponent<Position>(ids[n]).x);
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_CreateEntity_Perf()
    {
        for (int o = 0; o < 1; o++) {
            var store   = new RawEntityStore();
            var arch1   = store.GetArchetype(ComponentTypes.Get<ByteComponent>());
            _ = store.CreateEntity(arch1); // warmup
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int count       = 1000;
            //   1_000_000 ~ #PC:   29 ms
            //  10_000_000 ~ #PC:  274 ms
            // 100_000_000 ~ #PC: 1739 ms
            // 500_000_000 ~ #PC: 8259 ms
            for (int n = 0; n < count; n++) {
                _ = store.CreateEntity(arch1);
            }
            Console.WriteLine($"CreateEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
            Mem.AreEqual(count + 1, arch1.Count);
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_DeleteEntity_Perf()
    {
        var store   = new RawEntityStore();
        var arch1   = store.GetArchetype(ComponentTypes.Get<Position>());
        int count   = 10; // 10_000_000 ~ #PC: 244 ms
        for (int n = 0; n < count; n++) {
            _ = store.CreateEntity(arch1);
        }
        Mem.AreEqual(count, arch1.Count);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int n = 1; n <= count; n++) {
            Mem.IsTrue(store.DeleteEntity(n));
        }
        Console.WriteLine($"DeleteEntity() - raw. count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        Mem.AreEqual(0, arch1.Count);
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
        // 10_000_000 ~ #PC: 9 ms
        for (int n = 0; n < Count; n++) {
            var x = (int)positions[n].x;
            if (x != n)             Mem.FailAreEqual(x, n);
            i++;
        }
        Mem.AreEqual(Count, i);
        Console.WriteLine($"Iterate_Ref. count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms.");
    }
    
    /// <summary>
    /// Test cases that need to be covered: <br/>
    /// 0 <br/>
    /// 1 <br/>
    /// <see cref="ArchetypeUtils.MinCapacity"/> - 1<br/>
    /// <see cref="ArchetypeUtils.MinCapacity"/> <br/>
    /// <see cref="ArchetypeUtils.MinCapacity"/> + 1 <br/>
    /// 2 * <see cref="ArchetypeUtils.MinCapacity"/> - 1<br/>
    /// 2 * <see cref="ArchetypeUtils.MinCapacity"/> <br/>
    /// 2 * <see cref="ArchetypeUtils.MinCapacity"/> + 1 <br/>
    /// </summary>
    private const int QueryCount = 2 * ArchetypeUtils.MinCapacity + 1;
    
    [Test]
    public static void Test_StructHeapRaw_Query_Chunks()
    {
        var store   = new RawEntityStore();
        var arch1   = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        var query   = store.Query(Signature.Get<Position, Rotation>());
        for (int count = 0; count < QueryCount; count++) {
            int chunkCount  = 0;
            int n           = 0;
            foreach (var (positions, _, entities) in query.Chunks) {
                chunkCount++;
                foreach (var position in positions.Span) {
                    var x = (int)position.x;
                    if (x != n)     Mem.FailAreEqual(x, n);
                    n++;
                }
            }
            Mem.AreEqual(chunkCount, query.ChunkCount);
            Mem.AreEqual(count, n);
            var id = store.CreateEntity(arch1);
            store.GetEntityComponent<Position>(id).x = count;
        }
        Mem.AreEqual(QueryCount, arch1.Count);
    }
    
    /// use greater than <see cref="ArchetypeUtils.MinCapacity"/> for coverage
    private const int Count = 10_000; // 10_000  /  10_000_000
    
    [Test]
    public static void Test_StructHeapRaw_Query_Perf()
    {
        var store   = new RawEntityStore();
        var arch1   = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2>());
        // Count: 10_000_000
        //      CreateEntity()              ~ #PC:  408 ms
        //      for GetEntityComponent<>()  ~ #PC:   40 ms (good performance only, because archetypes remain unchanged after e 
        //      foreach Query.Chunks        ~ #PC:    4 ms
        var query   = store.Query(Signature.Get<MyComponent1, MyComponent2>());

        foreach (var _ in query.Chunks) { }                 // warmup
        {
            // _ = store.CreateEntity(arch1); // warmup
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int n = 0; n < Count; n++) {
                var id = store.CreateEntity(arch1);
                store.GetEntityComponent<MyComponent1>(id).a = n;
            }
            Console.WriteLine($"CreateEntity() - raw. count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms");
            Mem.AreEqual(Count, arch1.Count);
        } {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int n = 1; n < Count; n++) {
                ref var component1 = ref store.GetEntityComponent<MyComponent1>(n);                
                var x = component1.a;
                if (x != n - 1)     Mem.FailAreEqual(x, n - 1);
            }
            Console.WriteLine($"for GetEntityComponent<>(). count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms");
        } {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int n           = 0;
            var memStart    = Mem.GetAllocatedBytes();
            foreach (var (components, component2Chunk, _) in query.Chunks) {
                foreach (var component1 in components.Span) {
                    var x = component1.a;
                    if (x !=n)      Mem.FailAreEqual(x, n);
                    n++;
                }
            }
            var diff = Mem.GetAllocatedBytes() - memStart;
            Mem.AreEqual(Count, n);
            Console.WriteLine($"foreach Query.Chunks. count: {Count}, duration: {stopwatch.ElapsedMilliseconds} ms.  Alloc: {diff}");
        }
    }
    
    [Test]
    public static void Test_StructHeapRaw_DeleteEntity_twice()
    {
        var store   = new RawEntityStore();
        var arch    = store.GetArchetype(ComponentTypes.Get<Position>());
        var entity  = store.CreateEntity(arch);
        Mem.AreEqual(1,     store.Count);
        Mem.AreEqual(1,     arch.Count);
        Mem.IsTrue(store.DeleteEntity(entity));
        Mem.AreEqual(0,     store.Count);
        Mem.AreEqual(0,     arch.Count);
        Mem.IsTrue(!store.DeleteEntity(entity));
        Mem.AreEqual(0,     store.Count);
        Mem.AreEqual(0,     arch.Count);
    }
    
    [Test]
    public static void Test_StructHeapRaw_invalid_store()
    {
        var store1      = new RawEntityStore();
        var store2      = new RawEntityStore();
        var arch1       = store1.GetArchetype(ComponentTypes.Get<Position>());
        var e = Assert.Throws<ArgumentException>(() => {
            store2.CreateEntity(arch1);
        });
        Mem.AreEqual("entity is owned by a different store (Parameter 'archetype')", e!.Message);
    }
}

}
using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UselessBinaryOperation
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_StructHeap
{
    [Test]
    public static void Test_StructHeap_increase_entity_capacity()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var arch1       = store.GetArchetype(ComponentTypes.Get<Position>());
        int count       = 2000;
        var entities    = new Entity[count];
        for (int n = 0; n < count; n++)
        {
            var entity = arch1.CreateEntity();
            entities[n] = entity;
            Mem.AreSame(arch1,              entity.Archetype);
            Mem.AreEqual(n + 1,             arch1.Count);
            Mem.IsTrue(new Position() == entity.Position); // Position is present & default
            entity.Position.x = n;
        }
        Mem.AreEqual(2048, arch1.Capacity);
        for (int n = 0; n < count; n++) {
            Mem.AreEqual(n, entities[n].Position.x);
        }
    }
    
    [Test]
    public static void Test_StructHeap_shrink_entity_capacity() // ENTITY_STRUCT
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var arch1       = store.GetArchetype(ComponentTypes.Get<Position>());
        int count       = 2000;
        var entities    = new Entity[count];
        for (int n = 0; n < count; n++)
        {
            var entity = arch1.CreateEntity();
            entities[n] = entity;
            entity.Position.x = n;
        }
        // --- delete the majority of entities
        const int remaining = 500;
        for (int n = remaining; n < count; n++) {
            entities[n].DeleteEntity();
            Mem.AreEqual(count + remaining - n - 1, arch1.Count);
        }
        Mem.AreEqual(1024, arch1.Capacity);
        for (int n = 0; n < remaining; n++) {
            Mem.AreEqual(n, entities[n].Position.x);
        }
    }
    
    [Test]
    public static void Test_StructHeap_EntityStore_EnsureCapacity()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        Mem.AreEqual(1, store.EnsureCapacity(0)); // 1 => default capacity
        store.CreateEntity();
        Mem.AreEqual(0, store.EnsureCapacity(0));
        
        Mem.AreEqual(9, store.EnsureCapacity(9));
        for (int n = 0; n < 9; n++) {
            Mem.AreEqual(9 - n, store.EnsureCapacity(0));
            store.CreateEntity();
        }
        Mem.AreEqual(0, store.EnsureCapacity(0));
    }
    
    [Test]
    public static void Test_StructHeap_Archetype_EnsureCapacity()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var arch1   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        Mem.AreEqual(512, arch1.EnsureCapacity(0)); // 1 => default capacity
        arch1.CreateEntity();
        Mem.AreEqual(511, arch1.EnsureCapacity(0));
        
        Mem.AreEqual(1023, arch1.EnsureCapacity(1000));
        for (int n = 0; n < 1023; n++) {
            Mem.AreEqual(1023 - n, arch1.EnsureCapacity(0));
            arch1.CreateEntity();
        }
        Mem.AreEqual(0, arch1.EnsureCapacity(0));
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntities()
    {
        var count   = 10;
        var repeat  = 5;
        var store   = new EntityStore();
        // store.CreateEntity(1);
        var type    = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, MyComponent3>());
        var seqId   = 1;
        var evId    = 1;
        store.EnsureCapacity(repeat * count);
        type.EnsureCapacity (repeat * count);
        store.OnEntityCreate += create => {
            Mem.AreEqual(evId++, create.Entity.Id);
        };
        for (int n = 0; n < repeat; n++) {
            var start       = Mem.GetAllocatedBytes();
            var entities    = type.CreateEntities(count);
            Mem.AssertNoAlloc(start);
            foreach (var entity in entities) {
                Assert.AreEqual(entity.Id,  entity.Pid);
                Assert.AreSame(type,        entity.Archetype);
                Assert.AreEqual(seqId++,    entity.Id);
            }
        }
        var entityCount = repeat * count;
        Assert.AreEqual(entityCount, store.Count);
        Assert.AreEqual(entityCount, type.Count);
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntities_with_pids()
    {
        var count       = 5;
        var store       = new EntityStore(PidType.RandomPids);
        store.SetRandomSeed(1);
        var type        = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, MyComponent3>());
        var seqId       = 1;
        var entities    = type.CreateEntities(count);
        var pids = new long[] { 534011718, 237820880, 1002897798, 1657007234, 1412011072 };
        foreach (var entity in entities) {
            Assert.AreEqual(pids[seqId - 1], entity.Pid);
            Assert.AreSame(type,        entity.Archetype);
            Assert.AreEqual(seqId++,    entity.Id);
        }
        Assert.AreEqual(count, store.Count);
        Assert.AreEqual(count, type.Count);
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntity_Perf()
    {
        int repeat  = 10;     // 1000
        int count   = 10;     // 100_000
/*      #PC:
Entity count: 100000, repeat: 1000
EntityStore.EnsureCapacity()  duration: 0,0549166 ms
Archetype.EnsureCapacity()    duration: 0,2796653 ms
CreateEntity()                duration: 2,5453129 ms
CreateEntity() - all          duration: 2,8798948 ms
*/
        long time1      = 0;
        long time2      = 0;
        long time3      = 0;
        long memAlloc   = 0;

        for (int i = 0; i < repeat; i++)
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var arch1       = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, MyComponent3>());
            _ = arch1.CreateEntity(); // warmup
            
            var start1 = Stopwatch.GetTimestamp();
            store.EnsureCapacity(count + 1);
            var start2 = Stopwatch.GetTimestamp();
            time1 += start2 - start1;
            
            arch1.EnsureCapacity(count + 1);
            var start3 = Stopwatch.GetTimestamp();
            var memStart = Mem.GetAllocatedBytes();
            time2 += start3 - start2;
            
            var storeCapacity = store.Capacity;
            var arch1Capacity = arch1.Capacity;

            for (int n = 0; n < count; n++) {
                _ = arch1.CreateEntity();
            }
            time3 += Stopwatch.GetTimestamp() - start3;
            Mem.AreEqual(count + 1, arch1.Count);
            // assert initial capacity was sufficient
            Mem.AreEqual(storeCapacity, store.Capacity);
            Mem.AreEqual(arch1Capacity, arch1.Capacity);
            memAlloc += Mem.GetAllocatedBytes() - memStart;
        }
        var freq = repeat * Stopwatch.Frequency / 1000d;
        Console.WriteLine($"Entity count: {count}, repeat: {repeat}");
        Console.WriteLine($"EntityStore.EnsureCapacity()  duration: {time1 / freq} ms");
        Console.WriteLine($"Archetype.EnsureCapacity()    duration: {time2 / freq} ms");
        Console.WriteLine($"CreateEntity()                duration: {time3 / freq} ms");
        var all = time1 + time2 + time3;
        Console.WriteLine($"CreateEntity() - all          duration: {all   / freq} ms");
        Assert.AreEqual(0, memAlloc);
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntities_Perf()
    {
        int repeat  = 10;     // 1000
        int count   = 10;     // 100_000
/*      #PC:
Archetype.CreateEntities() Entity count: 100000, repeat: 1000, duration: 1,393 ms
*/
        var sw          = new Stopwatch();
        sw.Start();
        for (int i = 0; i < repeat; i++)
        {
            var store   = new EntityStore();
            var type    = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, MyComponent3>());
            var entities = type.CreateEntities(count);
            Mem.AreEqual(count, entities.Count);
        }
        var duration = (double)sw.ElapsedMilliseconds / repeat;
        Console.WriteLine($"Archetype.CreateEntities() Entity count: {count}, repeat: {repeat}, duration: {duration} ms");
    }
    
    [Test]
    public static void Test_Span_Clear_Pref() {
        var len     = 10_000;
        var repeat  = 1;
        var array   = new string[len];
        var span    = new Span<string>(array, 0, len);
        var sw      = new Stopwatch();
        sw.Start();
        for (int n = 0; n < repeat; n++) {
            span.Clear();
            // Array.Clear(array, 0, len);
            // span.Fill(default);
        }
        Console.WriteLine($"Span<>.Clear() count: {len},  duration: {sw.ElapsedMilliseconds} ms");
    }
    

    [Test]
    public static void Test_StructHeap_CreateEntity_Perf_100()
    {
        int count = 10; // 100_000 (UsePidAsId) ~ #PC: 3688 ms
        // --- warmup
        var store   = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        var arch1   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        arch1.EnsureCapacity(count);
        arch1.CreateEntity();

        // --- perf
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            store   = new EntityStore(PidType.UsePidAsId);
            store.EnsureCapacity(count);
            arch1   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
            arch1.EnsureCapacity(count);
            for (int n = 0; n < count; n++) {
                _ = arch1.CreateEntity();
            }
            Mem.AreEqual(count, arch1.Count);
        }
        Console.WriteLine($"CreateEntity() - Entity.  count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
    }
}

}

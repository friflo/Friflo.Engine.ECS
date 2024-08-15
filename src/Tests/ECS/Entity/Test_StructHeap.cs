using System;
using System.Diagnostics;
using System.Linq;
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
        Assert.AreEqual(10, store.ShrinkRatioThreshold);
        store.ShrinkRatioThreshold = 0 ;
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
        var maxId   = store.NodeMaxId;
        Mem.AreEqual(maxId, store.EnsureCapacity(0)); // 1 => default capacity
        var entity = store.CreateEntity();
        Mem.AreEqual(1, entity.Id);
        
        var count = maxId - 1;
        Mem.AreEqual(count, store.EnsureCapacity(0));
        
        Mem.AreEqual(count, store.EnsureCapacity(count));
        for (int n = 0; n < count; n++) {
            Mem.AreEqual(count - n, store.EnsureCapacity(0));
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
    public static void Test_StructHeap_RecycleIds_Disabled()
    {
        var store = new EntityStore { RecycleIds = false };
        store.RecycleIds = false;
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        
        var entity3 = store.CreateEntity();
        var entity4 = store.CreateEntity();
        Assert.AreEqual(3, entity3.Id);
        Assert.AreEqual(4, entity4.Id);
        
        // --- can change RecycleIds state on store
        store.RecycleIds = true;
        entity4.DeleteEntity();
        entity3.DeleteEntity();
        
        var entity3b = store.CreateEntity();
        Assert.AreEqual(3, entity3b.Id);
        
        store.RecycleIds = false;
        var entity5 = store.CreateEntity();
        Assert.AreEqual(5, entity5.Id);
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntity_RecycleIds()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        Assert.AreEqual(1, entity1.Revision);
        Assert.AreEqual(1, entity2.Revision);
        
        var entity2b = store.CreateEntity();
        var entity1b = store.CreateEntity();
        
        Assert.AreEqual(2, entity2b.Id);    Assert.AreEqual(2, entity2b.Revision);
        Assert.AreEqual(1, entity1b.Id);    Assert.AreEqual(2, entity1b.Revision);
        
        entity1b.DeleteEntity();
        entity2b.DeleteEntity();
        
        Assert.IsFalse(entity1 == entity1b);
        Assert.IsFalse(entity2 == entity2b);
        
        var type = store.GetArchetype(default);
        var entities = type.CreateEntities(3).ToArray();
        
        Assert.AreEqual(3, entities.Length);
        Assert.AreEqual("{ 2, 1, 3 }", entities.Debug()); // recycle: 2, 1   new id: 3
        Assert.AreEqual(3, entities[0].Revision);
        Assert.AreEqual(3, entities[1].Revision);
        Assert.AreEqual(1, entities[2].Revision);
        
        // --- cover case: recycled id is created manually
        entities[0].DeleteEntity(); // id: 2
        var entity2d = store.CreateEntity(2);
        Assert.AreEqual(2, entity2d.Id);    Assert.AreEqual(4, entity2d.Revision);
        Assert.IsFalse(entities[0] == entity2d);
        
        var entity4 = store.CreateEntity();
        Assert.AreEqual(4, entity4.Id);
        
        Assert.IsTrue(entities[0].IsNull);
        Assert.IsTrue(entity1.IsNull);
        Assert.IsTrue(entity2.IsNull);
        Assert.IsTrue(entity1b.IsNull);
        Assert.IsTrue(entity2b.IsNull);
    }
    
    [Test]
    public static void Test_StructHeap_RecycleIds_CommandBuffer()
    {
        var store = new EntityStore();
        var cb = store.GetCommandBuffer();
        cb.ReuseBuffer = true;
        long startMem = 0;
        for (int n = 0; n < 100; n++)
        {
            var entity1 = store.CreateEntity(); // id: 1
            var entity2 = store.CreateEntity(); // id: 2
            entity1.DeleteEntity();
            
            cb.DeleteEntity(entity2.Id);
            var id1 = cb.CreateEntity();     // recycle id: 1
            Mem.AreEqual(1, id1);
            cb.Playback();
            
            var entity1b = store.GetEntityById(1);
            Mem.IsFalse (entity1b.IsNull);
            Mem.IsTrue  (entity1.IsNull);
            Mem.IsTrue  (entity2.IsNull);
            Mem.AreEqual(1, store.Count);
            Mem.AreEqual(128, store.Capacity);
            entity1b.DeleteEntity();
            if (n == 0) {
                startMem = Mem.GetAllocatedBytes();
            }
        }
        Mem.AssertNoAlloc(startMem);
    }
    
    [Test]
    public static void Test_StructHeap_CreateEntity_RecycleIds_Perf()
    {
        int repeat      = 10; // 1_000_000;
        int count       = 100;
        // Entity count: 100, repeat: 1000000, duration: 4279 ms
        var store       = new EntityStore();
        var entities    = new Entity[count];
        var sw          = new Stopwatch();
        sw.Start();
        var start = 0L;
        for (int i = 0; i < repeat; i++)
        {
            for (int n = 0; n < count; n++) {
                entities[n] = store.CreateEntity();
            }
            for (int n = 0; n < count; n++) {
                entities[n].DeleteEntity();
            }
            if (i == 0) {
                start = Mem.GetAllocatedBytes();
            } else {
                Mem.AssertNoAlloc(start);
            }
        }
        for (int n = 0; n < count; n++) {
            Mem.AreEqual(count - n, entities[n].Id);
        }
        Console.WriteLine($"Entity count: {count}, repeat: {repeat}, duration: {sw.ElapsedMilliseconds} ms");
        Mem.AreEqual(0,     store.Count);
        Mem.AreEqual(128,   store.Capacity);
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

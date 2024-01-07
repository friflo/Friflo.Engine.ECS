using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.System;

public static class Test_Query
{
    private const long Count = 10; // 10_000_000;
        
    [Test]
    public static void Test_Query_arg_count_1()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        
        var archetype   = store.GetArchetype(Signature.Get<Position>());
        for (int n = 1; n <= 1000; n++) {
            var child = store.CreateEntity(archetype);
            child.Position = new Position(n, 0, 0);
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1", chunk.ToString());
            }
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 679 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    

    [Test]
    public static void Test_Query_arg_count_2()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new CreateSystems { argCount = 2 });
        
        var archetype   = store.GetArchetype(Signature.Get<Position, Rotation>());
        for (int n = 2; n <= 1000; n++) {
            var child = store.CreateEntity(archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1", chunk.ToString());
            }
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 670 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }

    [Test]
    public static void Test_Query_arg_count_3()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new CreateSystems { argCount = 3 });
        
        var archetype   = store.GetArchetype(Signature.Get<Position, Rotation, EntityName>());
        for (int n = 2; n <= 1000; n++) {
            var child = store.CreateEntity(archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, EntityName>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) {
                Mem.AreEqual(999, chunk.Length);
                Mem.AreEqual("Chunks[999]    Archetype: [EntityName, Position, Rotation]  Count: 999", chunk.ToString());
            }
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 845 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Query_arg_count_4()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new CreateSystems { argCount = 4 });
        
        var archetype   = store.GetArchetype(Signature.Get<Position, Rotation, Scale3, EntityName>());
        for (int n = 2; n <= 1000; n++) {
            var child = store.CreateEntity(archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, Scale3, EntityName>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1", chunk.ToString());
            }
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 966 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Query_arg_count_5()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new CreateSystems { argCount = 5 });
        
        var archetype   = store.GetArchetype(Signature.Get<Position, Rotation, Scale3, EntityName>());
        for (int n = 2; n <= 1000; n++) {
            var child = store.CreateEntity(archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, Scale3, Transform, EntityName>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1", chunk.ToString());
            }
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 1078 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _, _, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    
    private static EntityStore SetupTestStore() {
        var systems = new Systems();
        var store   = new EntityStore(PidType.UsePidAsId) { Systems = systems };
        Assert.AreSame(systems, store.Systems);
        
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("root"));
        root.AddComponent(new Position(1, 0, 0));
        root.AddComponent<Rotation>();
        root.AddComponent<Transform>();
        root.AddComponent<Scale3>();
        root.AddComponent<MyComponent1>();
        store.SetStoreRoot(root);
        return store;
    }
    
    // ReSharper disable once ConvertToConstant.Local
    private static readonly bool skipBench = true;
    
    [Test]
    public static void Test_BenchRef()
    {
        if (skipBench) return;
        
        var components = new MyComponent1[100];
        for (long i = 0; i < 10_000_000; i++) {
            bench_ref(components);
        }
        
        // --- run perf
        // 10_000_000 ~ 1098 ms
        components = new MyComponent1[100_000];
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (long i = 0; i < 1000; i++) {
            bench_ref(components);
        }
        Console.WriteLine($"ref duration: {stopwatch.ElapsedMilliseconds}");
    }
    
    private static void bench_ref(MyComponent1[] components) {
        Span<MyComponent1> comps = components;
        for (int n = 0; n < comps.Length; n++) {
            ++comps[n].a;
        }
    }
    
    [Test]
    public static void Test_Bench()
    {
        if (skipBench) return;
        
        var store   = new EntityStore(PidType.UsePidAsId);
        var child = store.CreateEntity();

        child.AddComponent(new MyComponent1());
        // --- force one time allocations
        var  query = store.Query<MyComponent1>();
        for (int i = 0; i < 10_000_000; i++) {
            bench(query);
        }
        
        for (int n = 1; n < 100_000; n++) {
            child = store.CreateEntity(child.Archetype);
        }
        // --- run perf
        var stopwatch = new Stopwatch(); stopwatch.Start();
        for (int i = 0; i < 1000; i++) {
            bench(query);
        }
        Console.WriteLine($"duration: {stopwatch.ElapsedMilliseconds}");
    }
    
    private static void bench(ArchetypeQuery<MyComponent1> query) {
        foreach (var (component, _) in query.Chunks) {
            for (int n = 0; n < component.Length; n++) {
                ++component[n].a;
            }
        }
    }
}


using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.System;

public static class Test_Query
{
    [Test]
    public static void Test_Query_arg_count_1()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        for (int n = 3; n <= 1000; n++) {
            child = store.CreateEntity(child.Archetype);
            child.Position = new Position(n, 0, 0);
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position>();
        foreach (var (_, _) in query.Chunks) { }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        long count = 10; // 10_000_000 ~ 945 ms
        for (long n = 0; n < count; n++) {
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
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        for (int n = 3; n <= 1000; n++) {
            child = store.CreateEntity(child.Archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation>();
        foreach (var (_, _, _) in query.Chunks) { }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        long count = 10; // 10_000_000 ~ 945 ms
        for (long n = 0; n < count; n++) {
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
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        child.AddComponent(new EntityName("child"));
        for (int n = 3; n <= 1000; n++) {
            child = store.CreateEntity(child.Archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, EntityName>();
        foreach (var (_, _, _, _) in query.Chunks) { }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        long count = 10; // 10_000_000 ~ 945 ms
        for (long n = 0; n < count; n++) {
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
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        child.AddComponent(new Scale3  (2, 0, 0));
        child.AddComponent(new EntityName("child"));
        for (int n = 3; n <= 1000; n++) {
            child = store.CreateEntity(child.Archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, Scale3, EntityName>();
        foreach (var (_, _, _, _, _) in query.Chunks) { }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        long count = 10; // 10_000_000 ~ 945 ms
        for (long n = 0; n < count; n++) {
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
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        child.AddComponent(new Scale3  (2, 0, 0));
        child.AddComponent<Transform>();
        child.AddComponent(new EntityName("child"));
        for (int n = 3; n <= 1000; n++) {
            child = store.CreateEntity(child.Archetype);
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, Scale3, Transform, EntityName>();
        foreach (var (_, _, _, _, _, _) in query.Chunks) { }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        long count = 10; // 10_000_000 ~ 945 ms
        for (long n = 0; n < count; n++) {
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
}


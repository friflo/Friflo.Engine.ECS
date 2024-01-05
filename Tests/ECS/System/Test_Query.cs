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
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1", chunk.ToString());
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 778 ms
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
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1", chunk.ToString());
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 933 ms
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
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) Mem.AreEqual("Chunks[512]    Archetype: [EntityName, Position, Rotation]  Count: 999", chunk.ToString());
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 968 ms
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
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1", chunk.ToString());
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 1073 ms
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
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) Mem.AreEqual("Chunks[512]    Archetype: [EntityName, Position, Rotation, Transform, Scale3]  Count: 999", chunk.ToString());
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ 1098 ms
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
}


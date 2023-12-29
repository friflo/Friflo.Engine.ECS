using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.System;


public class CreateSystems : Script
{
    public int argCount;
    
    public override void Start() {
        var store = Store;
        ComponentSystem system = argCount switch {
            1 => new MySystem_Arg1(store),
            2 => new MySystem_Arg2(store),
            3 => new MySystem_Arg3(store),
            4 => new MySystem_Arg4(store),
            5 => new MySystem_Arg5(store),
            _ => throw new ArgumentException($"value: {argCount}", nameof(argCount))
        };
        Systems.AddSystem(system);
    }
}

public class MySystem_Arg1 : ComponentSystem
{
    private readonly ArchetypeQuery<Position> query;
        
    public MySystem_Arg1(EntityStoreBase store) {
        query = store.Query<Position>();
        Assert.AreEqual("Chunks: [Position]", query.Chunks.ToString());
    }
    
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    public override void OnUpdate()
    {
        int chunkCount = 0;
        foreach (var position in query.Chunks) {
            var length = position.Values.Length;
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(1,     length);    break;
                case 1:     Mem.AreEqual(512,   length);    break;
                case 2:     Mem.AreEqual(487,   length);    break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(3, chunkCount);
    }
}

public class MySystem_Arg2 : ComponentSystem
{
    private readonly ArchetypeQuery<Position, Rotation> query;
        
    public MySystem_Arg2(EntityStoreBase store) {
        query = store.Query<Position, Rotation>();
        Assert.AreEqual("Chunks: [Position, Rotation]", query.Chunks.ToString());
    }
    
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    public override void OnUpdate()
    {
        int chunkCount = 0;
        foreach (var (position, _) in query.Chunks) {
            var length = position.Values.Length;
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(1,     length);    break;
                case 1:     Mem.AreEqual(512,   length);    break;
                case 2:     Mem.AreEqual(487,   length);    break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(3, chunkCount);
    }
}

public class MySystem_Arg3 : ComponentSystem
{
    private readonly ArchetypeQuery<Position, Rotation, EntityName> query;
        
    public MySystem_Arg3(EntityStoreBase store) {
        query = store.Query<Position, Rotation, EntityName>();
        Assert.AreEqual("Chunks: [Position, Rotation, EntityName]", query.Chunks.ToString());
    }
    
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    public override void OnUpdate()
    {
        int chunkCount = 0;
        foreach (var (position, _, _) in query.Chunks) {
            var length = position.Values.Length;
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(512,   length);    break;
                case 1:     Mem.AreEqual(487,   length);    break;
                case 2:     Mem.AreEqual(1,     length);    break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(3, chunkCount);
    }
}

public class MySystem_Arg4 : ComponentSystem
{
    private readonly ArchetypeQuery<Position, Rotation, EntityName, Scale3> query;
        
    public MySystem_Arg4(EntityStoreBase store) {
        query = store.Query<Position, Rotation, EntityName, Scale3>();
        Assert.AreEqual("Chunks: [Position, Rotation, EntityName, Scale3]", query.Chunks.ToString());
    }
    
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    public override void OnUpdate()
    {
        int chunkCount = 0;
        foreach (var (position, _, _, _) in query.Chunks) {
            var length = position.Values.Length;
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(1,     length);    break;
                case 1:     Mem.AreEqual(512,   length);    break;
                case 2:     Mem.AreEqual(487,   length);    break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(3, chunkCount);
    }
}

public class MySystem_Arg5 : ComponentSystem
{
    private readonly ArchetypeQuery<Position, Rotation, EntityName, Scale3, Transform> query;
        
    public MySystem_Arg5(EntityStoreBase store) {
        query = store.Query<Position, Rotation, EntityName, Scale3, Transform>();
        Assert.AreEqual("Chunks: [Position, Rotation, EntityName, Scale3, Transform]", query.Chunks.ToString());
    }
    
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    public override void OnUpdate()
    {
        int chunkCount = 0;
        foreach (var (position, _, _, _, _) in query.Chunks) {
            var length = position.Values.Length;
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(512,   length);    break;
                case 1:     Mem.AreEqual(487,   length);    break;
                case 2:     Mem.AreEqual(1,     length);    break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(3, chunkCount);
    }
}

public static class Test_Systems
{
    [Test]
    public static void Test_Systems_query_arg_count_1()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new CreateSystems { argCount = 1 });
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        for (int n = 3; n <= 1000; n++) {
            child = store.CreateEntity(child.Archetype);
            child.Position = new Position(n, 0, 0);
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ 774 ms
        ExecuteSystems(store.Systems, count);
    }
    
    [Test]
    public static void Test_Systems_query_arg_count_2()
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
            child.Position = new Position(n, 0, 0);
            child.Rotation = new Rotation(n, 0, 0, 0);
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ 774 ms
        ExecuteSystems(store.Systems, count);
    }
    
    [Test]
    public static void Test_Systems_query_arg_count_3()
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
            child.Position = new Position(n, 0, 0);
            child.Rotation = new Rotation(n, 0, 0, 0);
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ 774 ms
        ExecuteSystems(store.Systems, count);
    }
    
    [Test]
    public static void Test_Systems_query_arg_count_4()
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
            child.Position = new Position(n, 0, 0);
            child.Rotation = new Rotation(n, 0, 0, 0);
            child.Scale3   = new Scale3  (n, 0, 0);
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ 774 ms
        ExecuteSystems(store.Systems, count);
    }
    
    [Test]
    public static void Test_Systems_query_arg_count_5()
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
            child.Position = new Position(n, 0, 0);
            child.Rotation = new Rotation(n, 0, 0, 0);
            child.Scale3   = new Scale3  (n, 0, 0);
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ 774 ms
        ExecuteSystems(store.Systems, count);
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
    
    private static void CreateSystems(EntityStore store)
    {
        // --- start scripts to create systems
        foreach (var entityScripts in store.EntityScripts)
        {
            foreach (var script in entityScripts) {
                script.Start();
            }
        }
    }
    
    private static void ExecuteSystems(Systems systems, int count)
    {
        Assert.AreEqual("Count: 1", systems.ToString());

        // --- execute systems
        systems.UpdateSystems(); // force one time allocations

        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < count; n++) {
            systems.UpdateSystems();
        }
        Mem.AssertNoAlloc(start);
    }
}


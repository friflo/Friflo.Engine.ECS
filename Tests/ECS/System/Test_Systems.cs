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
        int count = 0;
        foreach (var position in query.Chunks) {
            switch(count++) {
                case 0:     Mem.AreEqual(512,   position.Values.Length); break;
                case 1:     Mem.AreEqual(487,   position.Values.Length); break;
                case 2:     Mem.AreEqual(1,     position.Values.Length); break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(3, count);
    }
}

public class MySystem_Arg2 : ComponentSystem
{
    private readonly ArchetypeQuery<Position, Rotation> query;
        
    public MySystem_Arg2(EntityStoreBase store) {
        query = store.Query<Position, Rotation>();
        Assert.AreEqual("Chunks: [Position]", query.Chunks.ToString());
    }
    
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    public override void OnUpdate()
    {
        int count = 0;
        foreach (var (position, _) in query.Chunks) {
            switch(count++) {
                case 0:     Mem.AreEqual(512,   position.Values.Length); break;
                case 1:     Mem.AreEqual(487,   position.Values.Length); break;
                case 2:     Mem.AreEqual(1,     position.Values.Length); break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(3, count);
    }
}

public static class Test_Systems
{
    [Test]
    public static void Test_Systems_create()
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
    
    private static EntityStore SetupTestStore() {
        var systems = new Systems();
        var store   = new EntityStore(PidType.UsePidAsId) { Systems = systems };
        Assert.AreSame(systems, store.Systems);
        
        var root    = store.CreateEntity();
        // root.AddComponent(new EntityName("root"));
        root.AddComponent(new Position(1, 0, 0));
        root.AddComponent<Rotation>();
        // root.AddComponent<Transform>();
        // root.AddComponent<Scale3>();
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


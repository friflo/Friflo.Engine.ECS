using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Systems {


public class SystemSet : Script
{
    public              int         argCount;
    private readonly    SystemRoot  Systems = new ("Systems");
    
    public override void Start() {
        QuerySystemBase system = argCount switch {
            0 => new MySystem_Arg0(),
            1 => new MySystem_Arg1(),
            2 => new MySystem_Arg2(),
            3 => new MySystem_Arg3(),
            4 => new MySystem_Arg4(),
            5 => new MySystem_Arg5(),
            _ => throw new ArgumentException($"value: {argCount}", nameof(argCount))
        };
        Systems.Add(system);
        Systems.AddStore(Store);
    }

    public override void Update() {
        Systems.Update(default);
    }
}

public class MySystem_Arg0 : QuerySystem
{
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    protected override void OnUpdate()
    {
        Mem.AreEqual(1000, Query.Count);
        Mem.AreEqual(1000, Query.Store.Count);
        Mem.AreEqual(1000, Query.Entities.Count);
    }
}

public class MySystem_Arg1 : QuerySystem<Position>
{
    
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    protected override void OnUpdate()
    {
        Mem.AreEqual(1000, Query.Count);
        var store       = Query.Store;
        var childArch   = store.GetArchetype(ComponentTypes.Get<Position>());
        int chunkCount  = 0;
        foreach (var (position, entities) in Query.Chunks) {
            var vectors     = position.AsSpanVector3();
            var length      = entities.Length;
            Mem.AreEqual(length,                    vectors.Length);
            Mem.AreEqual(length,                    position.Length);
            Mem.AreEqual(position[0].x,             entities[0]);
            Mem.AreEqual(position[length - 1].x,    entities[length - 1]);
            Mem.AreEqual(position[0].x,             entities.EntityAt(0).Id);
            Mem.AreEqual(position[length - 1].x,    entities.EntityAt(length - 1).Id);
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(1,     length);    Mem.AreSame(store.StoreRoot.Archetype,  entities.Archetype); break;
                case 1:     Mem.AreEqual(999,   length);    Mem.AreSame(childArch,                  entities.Archetype); break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(2, chunkCount);
    }
}

public class MySystem_Arg2 : QuerySystem<Position, Rotation>
{
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    protected override void OnUpdate()
    {
        var store       = Query.Store;
        var childArch   = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        int chunkCount  = 0;
        foreach (var (positions, rotations, entities) in Query.Chunks) {
            var vectors     = rotations.AsSpanQuaternion();
            var length      = entities.Length;
            Mem.AreEqual(length,                    vectors.Length);
            Mem.AreEqual(length,                    positions.Length);
            Mem.AreEqual(length,                    positions.Length);
            Mem.AreEqual(positions[0].x,            entities.EntityAt(0).Id);
            Mem.AreEqual(positions[length - 1].x,   entities.EntityAt(length - 1).Id);
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(1,     length);    Mem.AreSame(store.StoreRoot.Archetype,  entities.Archetype); break;
                case 1:     Mem.AreEqual(999,   length);    Mem.AreSame(childArch,                  entities.Archetype); break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(2, chunkCount);
    }
}

public class MySystem_Arg3 : QuerySystem<Position, Rotation, EntityName>
{
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    protected override void OnUpdate()
    {
        var store       = Query.Store;
        var childArch   = store.GetArchetype(ComponentTypes.Get<Position, Rotation, EntityName>());
        int chunkCount  = 0;
        foreach (var (positions, rotation, name, entities) in Query.Chunks) {
            var length      = entities.Length;
            Mem.AreEqual(length,                    positions.Length);
            Mem.AreEqual(length,                    rotation.Length);
            Mem.AreEqual(length,                    name.Length);
            Mem.AreEqual(positions[0].x,            entities.Ids[0]);
            Mem.AreEqual(positions[length - 1].x,   entities.Ids[length - 1]);
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(999,   length);    Mem.AreSame(childArch,                  entities.Archetype); break;
                case 1:     Mem.AreEqual(1,     length);    Mem.AreSame(store.StoreRoot.Archetype,  entities.Archetype); break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(2, chunkCount);
    }
}

public class MySystem_Arg4 : QuerySystem<Position, Rotation, EntityName, Scale3>
{
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    protected override void OnUpdate()
    {
        var store       = Query.Store;
        var childArch   = store.GetArchetype(ComponentTypes.Get<Position, Rotation, EntityName, Scale3>());
        int chunkCount  = 0;
        foreach (var (positions, rotation, name, scale, entities) in Query.Chunks) {
            var vectors     = scale.AsSpanVector3();
            var length      = entities.Length;
            Mem.AreEqual(length,                    vectors.Length);
            Mem.AreEqual(length,                    positions.Length);
            Mem.AreEqual(length,                    rotation.Length);
            Mem.AreEqual(length,                    name.Length);
            Mem.AreEqual(length,                    scale.Length);
            Mem.AreEqual(positions[0].x,            entities.Ids[0]);
            Mem.AreEqual(positions[length - 1].x,   entities.Ids[length - 1]);
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(1,     length);    Mem.AreSame(store.StoreRoot.Archetype,  entities.Archetype); break;
                case 1:     Mem.AreEqual(999,   length);    Mem.AreSame(childArch,                  entities.Archetype); break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(2, chunkCount);
    }
}

public class MySystem_Arg5 : QuerySystem<Position, Rotation, EntityName, Scale3, Transform>
{
    /// <summary> Cover <see cref="ChunkEnumerator{T1}.MoveNext"/> </summary>
    protected override void OnUpdate()
    {
        var store       = Query.Store;
        var childArch   = store.GetArchetype(ComponentTypes.Get<Position, Rotation, EntityName, Scale3, Transform>());
        int chunkCount  = 0;
        foreach (var (positions, rotation, name, scale, transform, entities) in Query.Chunks) {
            var matrix4X4   = transform.AsSpanMatrix4x4();
            var length      = entities.Length;
            Mem.AreEqual(length,                    matrix4X4.Length);
            Mem.AreEqual(length,                    positions.Length);
            Mem.AreEqual(length,                    rotation.Length);
            Mem.AreEqual(length,                    name.Length);
            Mem.AreEqual(length,                    scale.Length);
            Mem.AreEqual(length,                    transform.Length);
            Mem.AreEqual(positions[0].x,            entities.Ids[0]);
            Mem.AreEqual(positions[length - 1].x,   entities.Ids[length - 1]);
            switch(chunkCount++) {
                case 0:     Mem.AreEqual(999,   length);    Mem.AreSame(childArch,                  entities.Archetype); break;
                case 1:     Mem.AreEqual(1,     length);    Mem.AreSame(store.StoreRoot.Archetype,  entities.Archetype); break;
                default:    throw new InvalidOperationException("unexpected");
            }
        }
        Mem.AreEqual(2, chunkCount);
    }
}

public static class Test_ScriptSystems
{
    [Test]
    public static void Test_ScriptSystems_query_arg_count_0()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 0 });
        
        var child = store.CreateEntity();
        root.AddChild(child);
        for (int n = 3; n <= 1000; n++) {
            child = child.Archetype.CreateEntity();
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ #PC: 1575 ms
        ExecuteSystems(store, count);
    }
    
    [Test]
    public static void Test_ScriptSystems_query_arg_count_1()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 1 });
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        for (int n = 3; n <= 1000; n++) {
            child = child.Archetype.CreateEntity();
            child.Position = new Position(n, 0, 0);
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ #PC: 1575 ms
        ExecuteSystems(store, count);
    }
    
    [Test]
    public static void Test_ScriptSystems_query_arg_count_2()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 2 });
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        for (int n = 3; n <= 1000; n++) {
            child = child.Archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ #PC: 1387 ms
        ExecuteSystems(store, count);
    }
    
    [Test]
    public static void Test_ScriptSystems_query_arg_count_3()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 3 });
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        child.AddComponent(new EntityName("child"));
        for (int n = 3; n <= 1000; n++) {
            child = child.Archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ #PC: 1500 ms
        ExecuteSystems(store, count);
    }
    
    [Test]
    public static void Test_ScriptSystems_query_arg_count_4()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 4 });
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        child.AddComponent(new Scale3  (2, 0, 0));
        child.AddComponent(new EntityName("child"));
        for (int n = 3; n <= 1000; n++) {
            child = child.Archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ #PC: 1757 ms
        ExecuteSystems(store, count);
    }
    
    [Test]
    public static void Test_ScriptSystems_query_arg_count_5()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 5 });
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        child.AddComponent(new Rotation(2, 0, 0, 0));
        child.AddComponent(new Scale3  (2, 0, 0));
        child.AddComponent<Transform>();
        child.AddComponent(new EntityName("child"));
        for (int n = 3; n <= 1000; n++) {
            child = child.Archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        CreateSystems(store);
        int count = 10; // 10_000_000 ~ #PC: 1847 ms
        ExecuteSystems(store, count);
    }
    
    internal static EntityStore SetupTestStore() {
        var store   = new EntityStore();
        
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
    
    private static void ExecuteSystems(EntityStore store, int count)
    {
        // Assert.AreEqual("Count: 1", systems.ToString());

        // --- execute systems
        
        foreach (var scripts in store.EntityScripts) {
            foreach (var script in scripts) {
                script.Update();
            }
        }

        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < count; n++) {
            foreach (var scripts in store.EntityScripts) {
                foreach (var script in scripts) {
                    script.Update();
                }
            }
        }
        Mem.AssertNoAlloc(start);
    }
}

}

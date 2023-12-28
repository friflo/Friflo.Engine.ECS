using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.System;


public class CreateSystems : Script
{
    public override void Start() {
        var mySystem = new MySystem(Store);
        Systems.AddSystem(mySystem);
    }
}

public class MySystem : ComponentSystem
{
    private readonly ArchetypeQuery<Position> query;
        
    public MySystem(EntityStoreBase store) {
        query = store.Query<Position>();
        Assert.AreEqual("Chunks: [Position]", query.Chunks.ToString());
    }
    
    public override void OnUpdate()
    {
        int count = 0;
        foreach (var position in query.Chunks) {
            switch(count++) {
                case 0: Mem.AreEqual(9, position.Values.Length); break;
                case 1: Mem.AreEqual(1, position.Values.Length); break;
            }
        }
        Mem.AreEqual(2, count);
    }
}

public static class Test_Systems
{
    [Test]
    public static void Test_Systems_create()
    {
        // --- setup test entities
        var systems = new Systems();
        var store   = new EntityStore(PidType.UsePidAsId) { Systems = systems };
        Assert.AreSame(systems, store.Systems);
        
        var root    = store.CreateEntity(1);
        root.AddScript(new CreateSystems());
        root.AddComponent(new Position(1, 0, 0));
        root.AddComponent<Rotation>();
        for (int n = 2; n <= 10; n++) {
            var child = store.CreateEntity(n);
            child.AddComponent(new Position(n, 0, 0));
            root.AddChild(child);
        }
        store.SetStoreRoot(root);

        // --- start scripts to create systems
        foreach (var entityScripts in store.EntityScripts)
        {
            foreach (var script in entityScripts) {
                script.Start();
            }
        }
        Assert.AreEqual("Count: 1", systems.ToString());

        // --- execute systems
        systems.UpdateSystems(); // force one time allocations
        
        int count = 10; // 10_000_000 ~ 680 ms
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < count; n++) {
            systems.UpdateSystems();
        }
        Mem.AssertNoAlloc(start);
    }
}


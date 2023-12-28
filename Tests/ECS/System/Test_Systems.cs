using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.System;


public class AddSystem : Script
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
    }
    
    public override void OnUpdate()
    {
        int count = 0;
        foreach (var position in query.Chunks) {
            count++;
            Mem.AreEqual(position.Values.Length, 10);
        }
        Mem.AreEqual(1, count);
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
        var root    = store.CreateEntity(1);
        root.AddScript(new AddSystem());
        root.AddComponent(new Position(1, 0, 0));
        for (int n = 2; n <= 10; n++) {
            var child = store.CreateEntity(n);
            child.AddComponent(new Position(n, 0, 0));
            root.AddChild(child);
        }
        store.SetStoreRoot(root);

        // --- start scripts to create systems
        foreach (var entityScripts in store.EntityScripts) {
            foreach (var script in entityScripts.Scripts) {
                script.Start();
            }
        }

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


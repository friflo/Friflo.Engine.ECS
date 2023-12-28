using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.System;

public class MySystem : ComponentSystem
{
    private readonly ArchetypeQuery<Position, Rotation> query;
        
    public MySystem(EntityStoreBase store) {
        query = store.Query<Position, Rotation>();
    }
    
    public override void OnUpdate()
    {
        int count = 0;
        foreach (var (position, rotation) in query.Chunks) {
            count++;
            AreEqual(position.Values.Length, 10);
            AreEqual(rotation.Values.Length, 10);
        }
        AreEqual(1, count);
    }
}

public static class Test_Systems
{
    [Test]
    public static void Test_Systems_create()
    {
        // --- setup test entities
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity(1);
        root.AddComponent(new Position(1, 0, 0));
        root.AddComponent<Rotation>();
        for (int n = 2; n <= 10; n++) {
            var child = store.CreateEntity(n);
            child.AddComponent(new Position(n, 0, 0));
            child.AddComponent<Rotation>();
            root.AddChild(child);
        }
        store.SetStoreRoot(root);

        // --- setup systems
        var mySystem    = new MySystem(store);
        var systems     = new Systems();
        systems.AddSystem(mySystem);
        
        systems.UpdateSystems();
    }
}


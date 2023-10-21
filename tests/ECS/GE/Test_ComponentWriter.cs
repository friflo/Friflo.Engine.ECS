using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Host;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using EntityDatabase = Friflo.Fliox.Engine.ECS.Sync.EntityDatabase;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;

public static class Test_ComponentWriter
{
    private static GameEntityStore CreateGameEntityStore(out EntityDatabase database) {
        var hub     = new FlioxHub(new MemoryDatabase("test"));
        var client  = new SceneClient(hub);
        var sync    = new ClientDatabaseSync(client);
        var store   = new GameEntityStore(PidType.UsePidAsId);
        database    = new EntityDatabase(store, sync);
        return store;
    }
    
    [Test]
    public static void Test_ComponentWriter_write_components()
    {
        var store   = CreateGameEntityStore(out var database);
        var entity  = store.CreateEntity(10);
        var child   = store.CreateEntity(11);
        entity.AddChild(child);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddClassComponent(new TestRefComponent1 { val1 = 10 });
        
        var node = database.DataNodeFromEntity(entity);
        
        AreEqual(10,    node.pid);
        AreEqual(1,     node.children.Count);
        AreEqual(11,    node.children[0]);
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"testRef1\":{\"val1\":10}}", node.components.AsString());
    }
    
    [Test]
    public static void Test_ComponentWriter_write_empty_components()
    {
        var store   = CreateGameEntityStore(out var database);
        var entity  = store.CreateEntity(10);
        var node    = database.DataNodeFromEntity(entity);
        
        AreEqual(10,    node.pid);
        IsNull  (node.children);
    }
    
    [Test]
    public static void Test_ComponentWriter_write_tags()
    {
        var store   = CreateGameEntityStore(out var database);
        var entity  = store.CreateEntity(10);
        entity.AddTag<TestTag>();
        var node    = database.DataNodeFromEntity(entity);
        
        AreEqual(10,                node.pid);
        AreEqual(1,                 node.tags.Count);
        Contains(nameof(TestTag),   node.tags);
    }
    
    [Test]
    public static void Test_ComponentWriter_write_components_Perf()
    {
        var store   = CreateGameEntityStore(out var database);
        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddClassComponent(new TestRefComponent1 { val1 = 10 });

        int count = 10; // 2_000_000 ~ 1.935 ms
        DataNode node = null;
        for (int n = 0; n < count; n++) {
            node = database.DataNodeFromEntity(entity);
        }
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"testRef1\":{\"val1\":10}}", node!.components.AsString());
    }
}


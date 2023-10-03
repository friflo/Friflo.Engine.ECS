using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Hub.Host;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_ComponentWriter
{
    [Test]
    public static void Test_WriteComponents()
    {
        var hub     = new FlioxHub(new MemoryDatabase("test"));
        var client  = new EntityStoreClient(hub);
        var store   = new EntityStore(100, PidType.UsePidAsId);
        var entity  = store.CreateEntity(10);
        var child   = store.CreateEntity(11);
        entity.AddChild(child);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddClassComponent(new TestRefComponent1 { val1 = 10 });
        
        var node = store.EntityAsDataNode(entity, client);
        
        AreEqual(10,    node.pid);
        AreEqual(1,     node.children.Count);
        AreEqual(11,    node.children[0]);
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"testRef1\":{\"val1\":10}}", node.components.AsString());
    }
    
    [Test]
    public static void Test_WriteComponents_Perf()
    {
        var hub     = new FlioxHub(new MemoryDatabase("test"));
        var client  = new EntityStoreClient(hub);
        var store   = new EntityStore(100, PidType.UsePidAsId);
        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddClassComponent(new TestRefComponent1 { val1 = 10 });

        int count = 10; // 2_000_000 ~ 1.935 ms
        DataNode node = null;
        for (int n = 0; n < count; n++) {
            node = store.EntityAsDataNode(entity, client);
        }
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"testRef1\":{\"val1\":10}}", node!.components.AsString());
    }
}


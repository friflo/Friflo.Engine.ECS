using System.Collections.Generic;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Host;
using NUnit.Framework;
using Tests.ECS;
using Tests.ECS.Sync;
using static NUnit.Framework.Assert;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.Client;

public static class Test_Client
{
    private static FlioxHub CreateHub() {
        return new FlioxHub(new MemoryDatabase("test"));
    }
    
    [Test]
    public static void Test_Client_read_components()
    {
        var hub     = CreateHub();
        var client  = new GameClient(hub);
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var sync    = new GameSync(store, client);
        
        var rootNode    = new DataEntity { pid = 10L, components = Test_ComponentReader.rootComponents, children = new List<long> { 11 } };
        var childNode   = new DataEntity { pid = 11L, components = Test_ComponentReader.childComponents };
        sync.Entities.Add(rootNode);
        sync.Entities.Add(childNode);
        
        client.entities.Upsert(rootNode);
        client.entities.Upsert(childNode);
        client.SyncTasksSynchronous();
        
        int n = 0; 
        foreach (var entity in sync.Entities) {
            n++;
            NotNull(entity);
        }
        AreEqual(2, n);
        
        var root        = sync.GetGameEntity(10L, out _);
        var child       = sync.GetGameEntity(11L, out _);
        Test_ComponentReader.AssertRootEntity(root);
        Test_ComponentReader.AssertChildEntity(child);
        var type = store.GetArchetype(Signature.Get<Position, Scale3>());
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read root DataEntity again
        root.Position   = default;
        root.Scale3     = default;
        root            = sync.GetGameEntity(10L, out _);
        Test_ComponentReader.AssertRootEntity(root);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read child DataEntity again
        child.Position  = default;
        child.Scale3    = default;
        child           = sync.GetGameEntity(11L, out _);
        Test_ComponentReader.AssertChildEntity(child);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);


    }
    
    [Test]
    public static void Test_Client_write_components()
    {
        var hub     = CreateHub();
        var client  = new GameClient(hub);
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var sync    = new GameSync(store, client);

        var entity  = store.CreateEntity(10);
        var child   = store.CreateEntity(11);
        entity.AddChild(child);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddBehavior(new TestBehavior1 { val1 = 10 });
        
        var ge = sync.AddGameEntity(entity);
        AreEqual(1, sync.Entities.Count);
        
        AreEqual(10,    ge.pid);
        AreEqual(1,     ge.children.Count);
        AreEqual(11,    ge.children[0]);
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"testRef1\":{\"val1\":10}}", ge.components.AsString());
        
        ge = sync.AddGameEntity(child);
        AreEqual(2, sync.Entities.Count);
        
        AreEqual(11,    ge.pid);
        IsNull  (ge.children);
        IsTrue  (ge.components.IsNull());
        
        int n = 0; 
        foreach (var e in sync.Entities) {
            n++;
            NotNull(e.Value);
        }
        AreEqual(2, n);
    }
}
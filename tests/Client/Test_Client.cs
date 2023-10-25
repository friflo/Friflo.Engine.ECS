using System.Collections.Generic;
using System.IO;
using System.Text;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Host;
using NUnit.Framework;
using Tests.ECS;
using Tests.ECS.Sync;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.Client;

public static class Test_Client
{
    private static GameClient CreateClient() {
        var database    = new MemoryDatabase("test");
        var hub         = new FlioxHub(database);
        return new GameClient(hub);        
    }
    
    [Test]
    public static void Test_Client_load_game_entities()
    {
        var client  = CreateClient();
        var rootNode    = new DataEntity { pid = 10L, components = Test_ComponentReader.rootComponents, children = new List<long> { 11 } };
        var childNode   = new DataEntity { pid = 11L, components = Test_ComponentReader.childComponents };
        
        client.entities.Upsert(rootNode);
        client.entities.Upsert(childNode);
        client.SyncTasksSynchronous();
        
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var sync    = new GameSync(store, client);
        
        for (int n = 0; n < 2; n++) {
            sync.LoadGameEntities();
            
            var root        = store.GetNodeById(10).Entity;
            var child       = store.GetNodeById(11).Entity;
            Test_ComponentReader.AssertRootEntity(root);
            Test_ComponentReader.AssertChildEntity(child);
            var type = store.GetArchetype(Signature.Get<Position, Scale3>());
            AreEqual(2,     type.EntityCount);
            AreEqual(2,     store.EntityCount);
        }
    }
    

    [Test]
    public static void Test_Client_store_game_entities()
    {
        var client  = CreateClient();
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var sync    = new GameSync(store, client);

        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddBehavior(new TestBehavior1 { val1 = 10 });
        entity.AddTag<TestTag>();
        
        var child   = store.CreateEntity(11);
        entity.AddChild(child);
        
        for (int n = 0; n < 2; n++)
        {
            sync.StoreGameEntities();
            var fileName    = TestUtils.GetBasePath() + "assets/test_scene.json";
            var file        = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            sync.WriteScene(file);
            file.Close();
            
            AreEqual(2, store.EntityCount);

            var data10 = client.entities.Local[10];
            var data11 = client.entities.Local[11];
            
            AreEqual(10,    data10.pid);
            AreEqual(1,     data10.children.Count);
            AreEqual(11,    data10.children[0]);
            AreEqual("{\n    \"pos\": {\"x\":1,\"y\":2,\"z\":3},\n    \"testRef1\": {\"val1\":10}\n}", data10.components.AsString());
            
            AreEqual(11,    data11.pid);
            IsNull  (data11.children);
            IsTrue  (data11.components.IsNull());
        }
    }
    
    [Test]
    public static void Test_Client_empty_scene()
    {
        var client  = CreateClient();
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var sync    = new GameSync(store, client);

        for (int n = 0; n < 2; n++)
        {
            sync.StoreGameEntities();
            var stream = new MemoryStream();
            sync.WriteScene(stream);
            stream.Flush();
            var str = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            stream.Close();
            AreEqual("[]", str);
            
            AreEqual(0, store.EntityCount);
            AreEqual(0, client.entities.Local.Count);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Host.Event;
using NUnit.Framework;
using Tests.ECS;
using Tests.ECS.Sync;
using static NUnit.Framework.Assert;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.Client;

public static class Test_DataSync
{
    private static GameClient CreateClient() {
        var database    = new MemoryDatabase("test");
        var hub         = new FlioxHub(database);
        return new GameClient(hub);
    }
    
    [Test]
    public static async Task Test_DataSync_load_game_entities()
    {
        var client  = CreateClient();
        var rootNode    = new DataEntity { pid = 10L, components = Test_ComponentReader.RootComponents, children = new List<long> { 11 } };
        var childNode   = new DataEntity { pid = 11L, components = Test_ComponentReader.ChildComponents };
        
        client.entities.Upsert(rootNode);
        client.entities.Upsert(childNode);
        client.SyncTasksSynchronous();
        
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var sync    = new GameDataSync(store, client);
        AreSame(store, sync.Store); // ensure API available
        
        // load game entities via client sync
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
        
        // clear game entities in store
        store.GetNodeById(10).Entity.DeleteEntity();
        store.GetNodeById(11).Entity.DeleteEntity();
        AreEqual(0,     store.EntityCount);
        
        // load game entities via client async
        for (int n = 0; n < 2; n++) {
            await sync.LoadGameEntitiesAsync();
            
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
    public static async Task Test_DataSync_store_game_entities()
    {
        var client      = CreateClient();
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var sync        = new GameDataSync(store, client);

        var entity  = store.CreateEntity(10);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddBehavior(new TestBehavior1 { val1 = 10 });
        entity.AddTag<TestTag>();
        
        var child   = store.CreateEntity(11);
        store.ChildNodesChangedHandler = (object _, in ChildNodesChangedArgs args) => {
            AreEqual("entity: 10 - Add ChildIds[0] = 11", args.ToString());
        };
        entity.AddChild(child);
        AreEqual(2, store.EntityCount);
        
        // --- store game entities via client sync
        for (int n = 0; n < 2; n++)
        {
            sync.StoreGameEntities();

            var data10 = client.entities.Local[10];
            var data11 = client.entities.Local[11];
            
            AreEqual(10,    data10.pid);
            AreEqual(1,     data10.children.Count);
            AreEqual(11,    data10.children[0]);
            AreEqual("{\n        \"pos\": {\"x\":1,\"y\":2,\"z\":3},\n        \"testRef1\": {\"val1\":10}\n    }", data10.components.AsString());
            
            AreEqual(11,    data11.pid);
            IsNull  (data11.children);
            IsTrue  (data11.components.IsNull());
        }
        // --- store game entities via client async
        sync.ClearData();
        AreEqual(0, client.entities.Local.Count);
        for (int n = 0; n < 2; n++)
        {
            await sync.StoreGameEntitiesAsync();

            var data10 = client.entities.Local[10];
            var data11 = client.entities.Local[11];
            
            AreEqual(10,    data10.pid);
            AreEqual(1,     data10.children.Count);
            AreEqual(11,    data10.children[0]);
            AreEqual("{\n        \"pos\": {\"x\":1,\"y\":2,\"z\":3},\n        \"testRef1\": {\"val1\":10}\n    }", data10.components.AsString());
            
            AreEqual(11,    data11.pid);
            IsNull  (data11.children);
            IsTrue  (data11.components.IsNull());
        }
    }
    
    [Test]
    public static void Test_DataSync_constructor_params()
    {
        var e = Throws<ArgumentNullException>(() => {
            _ = new GameDataSync(null, null);
        });
        AreEqual("Value cannot be null. (Parameter 'store')", e!.Message);
        
        var store = new GameEntityStore();
        e = Throws<ArgumentNullException>(() => {
            _ = new GameDataSync(store, null);
        });
        AreEqual("Value cannot be null. (Parameter 'client')", e!.Message);
    }
    
    private static FlioxHub Prepare_SubscribeDatabaseChanges(out GameDataSync sync, out EventProcessorQueue processor)
    {
        var schema          = DatabaseSchema.Create<GameClient>();
        var database        = new MemoryDatabase("test", schema);
        var hub             = new FlioxHub(database);
        hub.UsePubSub();    // need currently called before SetupSubscriptions()
        hub.EventDispatcher = new EventDispatcher(EventDispatching.Send);
        var client          = new GameClient(hub);
        var store           = new GameEntityStore(PidType.UsePidAsId);
        sync                = new GameDataSync(store, client);
        processor           = new EventProcessorQueue();
        client.SetEventProcessor(processor);
        return hub;
    }
    
    /// <summary>Cover <see cref="GameDataSync.SubscribeDatabaseChanges"/></summary>
    [Test]
    public static void Test_DataSync_SubscribeDatabaseChanges()
    {
        var hub     = Prepare_SubscribeDatabaseChanges(out var sync, out var processor);
        var client  = new GameClient(hub);
        var store   = sync.Store;
        sync.SubscribeDatabaseChanges();
        
        var rootNode    = new DataEntity { pid = 10L, components = Test_ComponentReader.RootComponents, children = new List<long> { 11 } };
        var childNode   = new DataEntity { pid = 11L, components = Test_ComponentReader.ChildComponents };
        
        client.entities.Upsert(rootNode);
        client.entities.Upsert(childNode);
        client.SyncTasksSynchronous();
        processor.ProcessEvents();
        
        AreEqual(2, store.EntityCount);
        var root        = store.GetNodeById(10).Entity;
        var child       = store.GetNodeById(11).Entity;
        Test_ComponentReader.AssertRootEntity(root);
        Test_ComponentReader.AssertChildEntity(child);
        
        client.entities.Delete(10L);
        client.entities.Delete(11L);
        client.SyncTasksSynchronous();
        processor.ProcessEvents();
        AreEqual(0, store.EntityCount);
    }
    
    /// <summary>Cover <see cref="GameDataSync.SubscribeDatabaseChangesAsync"/></summary>
    [Test]
    public static async Task Test_DataSync_SubscribeDatabaseChangesAsync()
    {
        var hub     = Prepare_SubscribeDatabaseChanges(out var sync, out var processor);
        var client  = new GameClient(hub);
        var store   = sync.Store;
        await sync.SubscribeDatabaseChangesAsync();
        
        var rootNode    = new DataEntity { pid = 10L, components = Test_ComponentReader.RootComponents, children = new List<long> { 11 } };
        var childNode   = new DataEntity { pid = 11L, components = Test_ComponentReader.ChildComponents };
        
        client.entities.Upsert(rootNode);
        client.entities.Upsert(childNode);
        await client.SyncTasks();
        processor.ProcessEvents();
        
        AreEqual(2, store.EntityCount);
        var root        = store.GetNodeById(10).Entity;
        var child       = store.GetNodeById(11).Entity;
        Test_ComponentReader.AssertRootEntity(root);
        Test_ComponentReader.AssertChildEntity(child);
        
        client.entities.Delete(10L);
        client.entities.Delete(11L);
        await client.SyncTasks();
        processor.ProcessEvents();
        AreEqual(0, store.EntityCount);
    }
}
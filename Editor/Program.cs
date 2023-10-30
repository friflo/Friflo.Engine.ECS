using System.Threading;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Explorer;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Host.Event;
using Friflo.Json.Fliox.Hub.Remote;

namespace Friflo.Fliox.Editor;

public static class Program
{
    public static void Main(string[] args) {
        var schema          = DatabaseSchema.Create<GameClient>();
        var database        = new MemoryDatabase("game", schema) { Pretty = false };
        var hub             = new FlioxHub(database);
        hub.UsePubSub();    // need currently called before SetupSubscriptions()
        hub.EventDispatcher = new EventDispatcher(EventDispatching.Send);
        //
        var client      = new GameClient(hub);
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var sync        = new GameDataSync(store, client);
        var processor   = new EventProcessorQueue();
        client.SetEventProcessor(processor);
        sync.SubscribeDatabaseChanges();
        
        AddSampleEntities(sync);
        RunServer(hub);
        
        // simple event/game loop 
        while (true) {
            processor.ProcessEvents();
            Thread.Sleep(10);
        }
    }
    
    private static void RunServer(FlioxHub hub)
    {
        hub.Info.Set ("Editor", "dev", "https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine", "rgb(91,21,196)"); // optional
        hub.UseClusterDB(); // required by HubExplorer

        // --- create HttpHost
        var httpHost    = new HttpHost(hub, "/fliox/");
        httpHost.UseStaticFiles(HubExplorer.Path); // nuget: https://www.nuget.org/packages/Friflo.Json.Fliox.Hub.Explorer
    
        var thread = new Thread(_ => {
            HttpServer.RunHost("http://localhost:5000/", httpHost); // http://localhost:5000/fliox/
        });
        thread.Start();
    }
        
    private static void  AddSampleEntities(GameDataSync sync)
    {
        var store   = sync.Store;
        var root    = store.CreateEntity(1);
        root.AddComponent(new Position(1, 1, 1));
        root.AddComponent(new EntityName("root"));
        var child   = store.CreateEntity(2);
        child.AddComponent(new Position(2, 2, 2));
        root.AddChild(child);
        sync.StoreGameEntities();
    }
}


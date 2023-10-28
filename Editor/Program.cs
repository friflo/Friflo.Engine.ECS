using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
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
        hub.UsePubSub();    // need currently before SetupSubscriptions()
        hub.EventDispatcher = new EventDispatcher(EventDispatching.Send);
        //
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var client  = new GameClient(hub);
        var sync    = new GameDataSync(store, client);
        sync.SetupSubscriptions();
        
        RunServer(hub);
    }
    
    private static void RunServer(FlioxHub hub)
    {
        hub.Info.Set ("Editor", "dev", "https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine", "rgb(91,21,196)"); // optional
        hub.UseClusterDB(); // required by HubExplorer

        // --- create HttpHost
        var httpHost    = new HttpHost(hub, "/fliox/");
        httpHost.UseStaticFiles(HubExplorer.Path); // nuget: https://www.nuget.org/packages/Friflo.Json.Fliox.Hub.Explorer
    
        HttpServer.RunHost("http://localhost:5000/", httpHost); // http://localhost:5000/fliox/
    }
}


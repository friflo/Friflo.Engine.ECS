using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Hub.Explorer;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Remote;

namespace Friflo.Fliox.Editor;

using System;

public static class Program
{
    public static void Main(string[] args) {
        Console.WriteLine("Hello, World!");
        var store   = new GameEntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent(new Position());
        Console.WriteLine($"entity: {entity}");
        
        RunServer();
    }
    
    private static void RunServer()
    {
        var schema      = DatabaseSchema.Create<GameClient>();
        var database    = new MemoryDatabase("game", schema) { Pretty = false };
        var hub         = new FlioxHub(database);
        hub.Info.Set ("Editor", "dev", "https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine", "rgb(91,21,196)"); // optional
        hub.UseClusterDB(); // required by HubExplorer
        hub.UsePubSub();    // optional - enables Pub-Sub
        // --- create HttpHost
        var httpHost    = new HttpHost(hub, "/fliox/");
        httpHost.UseStaticFiles(HubExplorer.Path); // nuget: https://www.nuget.org/packages/Friflo.Json.Fliox.Hub.Explorer
    
        HttpServer.RunHost("http://localhost:5000/", httpHost); // http://localhost:5000/fliox/
    }
}


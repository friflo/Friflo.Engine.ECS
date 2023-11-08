using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Explorer;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Host.Event;
using Friflo.Json.Fliox.Hub.Remote;

// Note: Must not using imports Avalonia namespaces 

namespace Friflo.Fliox.Editor;

public interface IEditorControl
{
    Editor Editor { get; }
}

public class Editor
{
#region public properties
    public              GameEntityStore    Store    => store;

    #endregion

#region private fields
    private             GameEntityStore     store;
    private             GameDataSync        sync;
    private             event Action        OnReady;
    private             bool                isReady;
    private readonly    ManualResetEvent    signalEvent = new ManualResetEvent(false);
    private             EventProcessorQueue processor;
    private             HttpServer          server;
    #endregion

    public async Task Init()
    {
        store       = new GameEntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("Editor Root"));
        store.SetStoreRoot(root);
        
        Console.WriteLine($"--- Editor.OnReady() {Program.startTime.ElapsedMilliseconds} ms");
        isReady = true;
        EditorUtils.Post(() => {
            OnReady?.Invoke();
        });
        // --- add client and database
        var schema      = DatabaseSchema.Create<GameClient>();
        var database    = CreateDatabase(schema, "file-system");
        var hub         = new FlioxHub(database);
        hub.UsePubSub();    // need currently called before SetupSubscriptions()
        hub.EventDispatcher = new EventDispatcher(EventDispatching.Send);
        //
        var client      = new GameClient(hub);
        sync            = new GameDataSync(store, client);
        processor       = new EventProcessorQueue(ReceivedEvent);
        client.SetEventProcessor(processor);
        await sync.SubscribeDatabaseChangesAsync();
        
        await AddSampleEntities(sync);

        // todo 
        store.ChildNodesChanged += ChildNodesChangedHandler;
        
        // --- run server
        server = RunServer(hub);
    }
    
    /// <summary>SYNC: <see cref="GameEntity"/> -> <see cref="GameDataSync"/></summary>
    private void ChildNodesChangedHandler (object sender, in ChildNodesChangedArgs args)
    {
        switch (args.action) {
            case ChildNodesChangedAction.Add:
                sync.UpsertDataEntityAsync(args.parentId);
                break;
            case ChildNodesChangedAction.Remove:
                sync.UpsertDataEntityAsync(args.parentId);
                break;
        }
    }
    
    internal void Shutdown() {
        server?.Stop();
    }
    
    internal void HandleOnReady(Action onReady) {
        if (onReady == null) {
            return;
        }
        if (isReady) {
            onReady(); // could be deferred to event loop
            return;
        }
        OnReady += onReady;
    }
    
    internal void Run()
    {
        // simple event/game loop 
        while (signalEvent.WaitOne())
        {
            signalEvent.Reset();
            processor.ProcessEvents();
        }
    }
    
    private void ProcessEvents() {
        processor.ProcessEvents();
    }
    
    private void ReceivedEvent () {
        EditorUtils.Post(ProcessEvents);
    }
    
    private static HttpServer RunServer(FlioxHub hub)
    {
        hub.Info.Set ("Editor", "dev", "https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine", "rgb(91,21,196)"); // optional
        hub.UseClusterDB(); // required by HubExplorer

        // --- create HttpHost
        var httpHost    = new HttpHost(hub, "/fliox/");
        httpHost.UseStaticFiles(HubExplorer.Path); // nuget: https://www.nuget.org/packages/Friflo.Json.Fliox.Hub.Explorer
    
        var server = new HttpServer ("http://localhost:5000/", httpHost);  // http://localhost:5000/fliox/
        var thread = new Thread(_ => {
            // HttpServer.RunHost("http://localhost:5000/", httpHost); // http://localhost:5000/fliox/
            server.Start();
            server.Run();
        });
        thread.Start();
        return server;
    }
    
    private static EntityDatabase CreateDatabase(DatabaseSchema schema, string provider)
    {
        if (provider == "file-system") {
            var directory = Directory.GetCurrentDirectory() + "/DB";
            return new FileDatabase("game", directory, schema) { Pretty = false };
        }
        return new MemoryDatabase("game", schema) { Pretty = false };
    }
        
    private static async Task AddSampleEntities(GameDataSync sync)
    {
        var store   = sync.Store;
        var root    = store.StoreRoot;
        root.AddComponent(new Position(1, 1, 1));
        root.AddComponent(new EntityName("root"));
        var child   = CreateEntity(store, 2);
        child.AddComponent(new Position(2, 2, 2));

        root.AddChild(child);
        root.AddChild(CreateEntity(store, 3));
        root.AddChild(CreateEntity(store, 4));
        root.AddChild(CreateEntity(store, 5));
        root.AddChild(CreateEntity(store, 6));
        
        await sync.StoreGameEntitiesAsync();
    }
    
    private static GameEntity CreateEntity(GameEntityStore store, int id)
    {
        var entity = store.CreateEntity();
        entity.AddComponent(new EntityName("child-" + id));
        return entity;
    }
}
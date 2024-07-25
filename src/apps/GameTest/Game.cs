using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Engine.Hub;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Explorer;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Host.Event;
using Friflo.Json.Fliox.Hub.Remote;

// ReSharper disable NotAccessedField.Local
namespace GameTest;

public class Game
{
    #region public properties
    public              EntityStore             Store    => store;

    #endregion

    #region private fields
    private             EntityStore             store;
    private             StoreSync               sync;
//  private             bool                    isReady;
    private  readonly   ManualResetEvent        signalEvent = new ManualResetEvent(false);
    private             EventProcessorQueue     processor;
    private             HttpServer              server;
    
    private static readonly bool SyncDatabase = true;

    #endregion
    
    
    public async Task Init()
    {
        StoreDispatcher.AssertMainThread();
        store       = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("Editor Root"));
        store.SetStoreRoot(root);
        
        Console.WriteLine($"--- Editor.OnReady() {Program.startTime.ElapsedMilliseconds} ms");
    //  isReady = true;
    //  StoreDispatcher.Post(() => {
    //      EditorObserver.CastEditorReady(observers);
    //  });
        // --- add client and database
        var schema          = DatabaseSchema.Create<StoreClient>();
        var database        = CreateDatabase(schema, "in-memory");
        var storeCommands   = new StoreCommands(store);
        database.AddCommands(storeCommands);
        
        var hub             = new FlioxHub(database);
        hub.UsePubSub();    // need currently called before SetupSubscriptions()
        hub.EventDispatcher = new EventDispatcher(EventDispatching.Send);
        //
        var client          = new StoreClient(hub);
        if (SyncDatabase) {
            sync            = new StoreSync(store, client);
            processor       = new EventProcessorQueue(ReceivedEvent);
            client.SetEventProcessor(processor);
            await sync.SubscribeDatabaseChangesAsync();
        }
    //  TestBed.AddSampleEntities(store);
        if (SyncDatabase) {
            store.OnComponentAdded     += args => SyncEntity(args.EntityId); 
            store.OnComponentRemoved   += args => SyncEntity(args.EntityId); 
            store.OnScriptAdded        += args => SyncEntity(args.Entity.Id); 
            store.OnScriptRemoved      += args => SyncEntity(args.Entity.Id); 
            store.OnTagsChanged        += args => SyncEntity(args.EntityId);
            await sync.StoreEntitiesAsync();
        }
        store.OnChildEntitiesChanged += ChildEntitiesChangedHandler;
        
        StoreDispatcher.AssertMainThread();
        // --- run server
        server = RunServer(hub);
    }
    
    private void SyncEntity(int id)
    {
        sync.UpsertDataEntity(id);
        PostSyncChanges();
    }
    
    // ---------------------------------------- private methods ----------------------------------------
#region private methods
    /// <summary>SYNC: <see cref="Entity"/> -> <see cref="StoreSync"/></summary>
    private void ChildEntitiesChangedHandler (ChildEntitiesChanged args)
    {
        StoreDispatcher.AssertMainThread();
        switch (args.Action)
        {
            case ChildEntitiesChangedAction.Add:
                sync?.UpsertDataEntity(args.EntityId);
                PostSyncChanges();
                break;
            case ChildEntitiesChangedAction.Remove:
                sync?.UpsertDataEntity(args.EntityId);
                PostSyncChanges();
                break;
        }
    }
    
    private bool syncChangesPending;
    
    /// Accumulate change tasks and SyncTasks() at once.
    private void PostSyncChanges() {
        if (syncChangesPending) {
            return;
        }
        syncChangesPending = true;
        StoreDispatcher.Post(SyncChangesAsync);
    }
    
    private async void SyncChangesAsync() {
        syncChangesPending = false;
        StoreDispatcher.AssertMainThread();
        if (sync != null) {
            await sync.SyncChangesAsync();
        }
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
        StoreDispatcher.AssertMainThread();
        processor.ProcessEvents();
    }
    
    private void ReceivedEvent () {
        StoreDispatcher.Post(ProcessEvents);
    }
    
    private static HttpServer RunServer(FlioxHub hub)
    {
        hub.Info.Set ("Editor", "dev", "https://github.com/friflo/Friflo.Engine.ECS", "rgb(91,21,196)"); // optional
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
        if (provider == "in-memory") {
            return new MemoryDatabase("game", schema) { Pretty = false };
        }
        throw new ArgumentException($"invalid database provider: {provider}");
    }
    #endregion
}
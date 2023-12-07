// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Friflo.Fliox.Editor.UI.Panels;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Utils;
using Friflo.Fliox.Engine.Hub;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Explorer;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Host.Event;
using Friflo.Json.Fliox.Hub.Remote;

// Note! Must not using Avalonia namespaces

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Friflo.Fliox.Editor;

public partial class Editor
{
#region public properties
    public              EntityStore             Store    => store;

    #endregion

#region private fields
    private             EntityStore             store;
    private             StoreSync               sync;
    private  readonly   List<EditorObserver>    observers   = new List<EditorObserver>();
    private             bool                    isReady;
    private  readonly   ManualResetEvent        signalEvent = new ManualResetEvent(false);
    private             EventProcessorQueue     processor;
    private             HttpServer              server;
    
    private static readonly bool SyncDatabase = true;

    #endregion

    // ---------------------------------------- public methods ----------------------------------------
#region public methods
    public async Task Init()
    {
        StoreUtils.AssertMainThread();
        store       = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("Editor Root"));
        store.SetStoreRoot(root);
        
        Console.WriteLine($"--- Editor.OnReady() {Program.startTime.ElapsedMilliseconds} ms");
        isReady = true;
        StoreUtils.Post(() => {
            EditorObserver.CastEditorReady(observers);
        });
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
        TestBed.AddSampleEntities(store);
        if (SyncDatabase) {
            store.ComponentAdded     += (in ComponentChangedArgs args) => SyncEntity(args.entityId); 
            store.ComponentRemoved   += (in ComponentChangedArgs args) => SyncEntity(args.entityId); 
            store.ScriptAdded        += (in ScriptChangedArgs    args) => SyncEntity(args.entityId); 
            store.ScriptRemoved      += (in ScriptChangedArgs    args) => SyncEntity(args.entityId); 
            store.TagsChanged        += (in TagsChangedArgs      args) => SyncEntity(args.entityId);
            await sync.StoreEntitiesAsync();
        }
        store.ChildNodesChanged += ChildNodesChangedHandler;
        
        StoreUtils.AssertMainThread();
        // --- run server
        server = RunServer(hub);
    }
    
    private void SyncEntity(int id)
    {
        sync.UpsertDataEntity(id);
        PostSyncChanges();
    }
    
    public void AddObserver(EditorObserver observer)
    {
        if (observer == null) {
            return;
        }
        observers.Add(observer);
        if (isReady) {
            observer.SendEditorReady();  // could be deferred to event loop
        }
    }
    
    public void SelectionChanged(EditorSelection selection) {
        StoreUtils.Post(() => {
            EditorObserver.CastSelectionChanged(observers, selection);    
        });
    }
    
    internal void Shutdown() {
        server?.Stop();
    }
    #endregion
    
    // -------------------------------------- panel / commands --------------------------------------
    private PanelControl activePanel;
    
    internal void SetActivePanel(PanelControl panel)
    {
        if (activePanel != null) {
            activePanel.Header.PanelActive = false;
        }
        activePanel = panel;
        if (panel != null) {
            panel.Header.PanelActive = true;
        }
    }

    // ---------------------------------------- private methods ----------------------------------------
#region private methods
    /// <summary>SYNC: <see cref="Entity"/> -> <see cref="StoreSync"/></summary>
    private void ChildNodesChangedHandler (object sender, in ChildNodesChangedArgs args)
    {
        StoreUtils.AssertMainThread();
        switch (args.action)
        {
            case ChildNodesChangedAction.Add:
                sync?.UpsertDataEntity(args.parentId);
                PostSyncChanges();
                break;
            case ChildNodesChangedAction.Remove:
                sync?.UpsertDataEntity(args.parentId);
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
        StoreUtils.Post(SyncChangesAsync);
    }
    
    private async void SyncChangesAsync() {
        syncChangesPending = false;
        StoreUtils.AssertMainThread();
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
        StoreUtils.AssertMainThread();
        processor.ProcessEvents();
    }
    
    private void ReceivedEvent () {
        StoreUtils.Post(ProcessEvents);
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
        if (provider == "in-memory") {
            return new MemoryDatabase("game", schema) { Pretty = false };
        }
        throw new InvalidEnumArgumentException($"invalid database provider: {provider}");
    }
    #endregion
}
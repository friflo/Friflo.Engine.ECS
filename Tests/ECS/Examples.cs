using System;
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable UnusedVariable
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable MemberCanBePrivate.Global
namespace Tests.ECS;

public static class Examples
{
    [ComponentKey("my-component")]
    public struct MyComponent : IComponent {
        public int value;
    }
    
    [Test]
    public static void AddComponents()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        
        // add components
        entity.AddComponent(new EntityName("Hello World!"));// EntityName is a build-in component
        entity.AddComponent(new MyComponent { value = 42 });
        Console.WriteLine($"entity: {entity}");             // > entity: id: 1  "Hello World!"  [EntityName, Position]
        
        // get component
        Console.WriteLine($"name: {entity.Name.value}");    // > name: Hello World!
        var value = entity.GetComponent<MyComponent>().value;
        Console.WriteLine($"MyComponent: {value}");         // > MyComponent: 42
        
        // Serialize entity to JSON
        Console.WriteLine(entity.DebugJSON);
    }
    
    /// <summary>
    /// <see cref="EntityStoreBase.GetUniqueEntity"/> is used to reduce code coupling.
    /// It enables access to a unique entity without the need to pass the entity by external code.   
    /// </summary>
    [Test]
    public static void GetUniqueEntity()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent(new UniqueEntity("Player"));    // UniqueEntity is a build-in component
        
        var player  = store.GetUniqueEntity("Player");
        Console.WriteLine($"entity: {player}");             // > entity: id: 1  [UniqueEntity]
    }

    public struct MyTag1 : ITag { }
    public struct MyTag2 : ITag { }

    [Test]
    public static void AddTags()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        // add tags
        entity.AddTag<MyTag1>();
        entity.AddTag<MyTag2>();
        Console.WriteLine($"entity: {entity}");             // > entity: id: 1  [#MyTag1, #MyTag2]
        
        // get tag
        var tag1 = entity.Tags.Has<MyTag1>();
        Console.WriteLine($"tag1: {tag1}");                 // > tag1: True
    }
    
    public class MyScript : Script { public int data; }
    
    [Test]
    public static void AddScript()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        // add script
        entity.AddScript(new MyScript{ data = 123 });
        Console.WriteLine($"entity: {entity}");             // > entity: id: 1  [*MyScript]
        
        // get script
        var myScript = entity.GetScript<MyScript>();
        Console.WriteLine($"data: {myScript.data}");        // > data: 123
    }
    
    [Test]
    public static void AddChildEntities()
    {
        var store   = new EntityStore();
        var root    = store.CreateEntity();
        var child1  = store.CreateEntity();
        var child2  = store.CreateEntity();
        
        // add child entities
        root.AddChild(child1);
        root.AddChild(child2);
        
        Console.WriteLine($"child entities: {root.ChildEntities}"); // > child entities: Count: 2
    }
    
    [Test]
    public static void AddEventHandlers()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.OnComponentChanged     += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Component: [MyComponent]
        entity.OnTagsChanged          += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Tags: [#MyTag1]
        entity.OnScriptChanged        += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Script: [*MyScript]
        entity.OnChildEntitiesChanged += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Child[0] = 2

        entity.AddComponent(new MyComponent());
        entity.AddTag<MyTag1>();
        entity.AddScript(new MyScript());
        entity.AddChild(store.CreateEntity());
    }
    
    public readonly struct MySignal { }
    
    [Test]
    public static void AddSignalHandler()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddSignalHandler<MySignal>(signal => { Console.WriteLine(signal); }); // > entity: 1 - signal > MySignal    
        entity.EmitSignal(new MySignal());
    }
    
    [Test]
    public static void EntityQueries()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity();
        entity1.AddComponent(new EntityName("test"));
        entity1.AddTag<MyTag1>();
        
        var entity2 = store.CreateEntity();
        entity2.AddComponent(new MyComponent { value = 42 });
        entity2.AddTag<MyTag1>();
        
        var entity3 = store.CreateEntity();
        entity3.AddComponent(new MyComponent { value = 1337 });
        entity3.AddTag<MyTag1>();
        entity3.AddTag<MyTag2>();
        
        // --- query components
        var queryEntityNames = store.Query<EntityName>();
        Console.WriteLine(queryEntityNames);                // > Query: [EntityName]  EntityCount: 1

        var queryMyComponents = store.Query<MyComponent>();
        Console.WriteLine(queryMyComponents);               // > Query: [MyComponent]  EntityCount: 2
        
        // --- query tags
        var queryTag  = store.Query().AllTags(Tags.Get<MyTag1>());
        Console.WriteLine(queryTag);                        // > Query: [#MyTag1]  EntityCount: 3
        
        var queryTags = store.Query().AllTags(Tags.Get<MyTag1, MyTag2>());
        Console.WriteLine(queryTags);                       // > Query: [#MyTag1, #MyTag2]  EntityCount: 1
    }
    
    [Test]
    public static void EnumerateQueryChunks()
    {
        var store   = new EntityStore();
        for (int n = 0; n < 3; n++) {
            var entity = store.CreateEntity();
            entity.AddComponent(new MyComponent{ value = n + 42 });
        }
        var query = store.Query<MyComponent>();
        foreach (var (components, entities) in query.Chunks)
        {
            foreach (var component in components.Span) {
                Console.WriteLine($"MyComponent.value: {component.value}");
                // > MyComponent.value: 42
                // > MyComponent.value: 43
                // > MyComponent.value: 44
            }
        }
    }
    
    [Test]
    public static void ParallelQueryJob()
    {
        var runner  = new ParallelJobRunner(Environment.ProcessorCount);
        var store   = new EntityStore { JobRunner = runner };
        for (int n = 0; n < 10_000; n++) {
            var entity = store.CreateEntity().AddComponent<MyComponent>();
        }
        var query = store.Query<MyComponent>();
        var queryJob = query.ForEach((myComponents, entities) =>
        {
            // multi threaded query execution running on all available cores 
            foreach (ref var myComponent in myComponents.Span) {
                myComponent.value += 10;                
            }
        });
        queryJob.RunParallel();
        runner.Dispose();
    }
    
    [Test]
    public static void FilterEntityEvents()
    {
        var store   = new EntityStore();
        store.EventRecorder.Enabled = true; // required for EventFilter
        
        var entity1 = store.CreateEntity();
        entity1.AddComponent<MyComponent>();
        entity1.AddComponent<Position>();
        
        var entity2 = store.CreateEntity();
        entity2.AddComponent<MyComponent>();
        entity2.AddTag   <MyTag1>();
        
        var query = store.Query<MyComponent>();
        query.EventFilter.ComponentAdded<Position>();
        query.EventFilter.TagAdded<MyTag1>();
        
        foreach (var (myComponent, entities) in query.Chunks) {
            foreach (var entity in entities) {
                bool hasEvent = query.HasEvent(entity.Id);
                Console.WriteLine($"{entity} - hasEvent: {hasEvent}");                   
            }
        }
        // > id: 1  [Position, MyComponent] - hasEvent: True
        // > id: 2  [MyComponent, #MyTag1] - hasEvent: True
    }
    
    [Test]
    public static void CommandBuffer()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.AddComponent<Position>();
        
        CommandBuffer cb = store.GetCommandBuffer();
        var newEntity = cb.CreateEntity();
        cb.DeleteEntity  (entity2.Id);
        cb.AddComponent  (newEntity, new EntityName("new entity"));
        cb.RemoveComponent<Position>(entity1.Id);        
        cb.AddComponent  (entity1.Id, new EntityName("changed entity"));
        cb.AddTag<MyTag1>(entity1.Id);
        
        cb.Playback();
        
        var entity3 = store.GetEntityById(newEntity);
        Console.WriteLine(entity1);                         // > id: 1  "changed entity"  [EntityName, #MyTag1]
        Console.WriteLine(entity2);                         // > id: 2  (detached)
        Console.WriteLine(entity3);                         // > id: 3  "new entity"  [EntityName]
    }
}
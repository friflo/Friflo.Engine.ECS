using System;
using System.Collections.Generic;
using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;


// ReSharper disable UnusedVariable
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General
public static class General
{

[Test]
public static void CreateStore()
{
    var store = new EntityStore();
}

[Test]
public static void CreateEntity()
{
    var store = new EntityStore();
    store.CreateEntity();
    store.CreateEntity();
    
    foreach (var entity in store.Entities) {
        Console.WriteLine($"entity {entity}");
    }
    // > entity id: 1  []       Info:  []  shows entity has no components, tags or scripts
    // > entity id: 2  []
}

[Test]
public static void DisableEntity()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.Enabled = false;
    Console.WriteLine(entity);                          // > id: 1  [#Disabled]
    
    var query    = store.Query();
    Console.WriteLine($"default - {query}");            // > default - Query: []  Count: 0
    
    var disabled = store.Query().WithDisabled();
    Console.WriteLine($"disabled - {disabled}");        // > disabled - Query: []  Count: 1
}

[ComponentKey("my-component")]
public struct MyComponent : IComponent {
    public int value;
}

[Test]
public static void AddComponents()
{
    var store   = new EntityStore();
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
    store.CreateEntity(new UniqueEntity("Player"));     // UniqueEntity is a build-in component
    
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


[Test]
public static void EntityQueries()
{
    var store   = new EntityStore();
    store.CreateEntity(new EntityName("entity-1"));
    store.CreateEntity(new EntityName("entity-2"), Tags.Get<MyTag1>());
    store.CreateEntity(new EntityName("entity-3"), Tags.Get<MyTag1, MyTag2>());
    
    // --- query components
    var queryNames = store.Query<EntityName>();
    queryNames.ForEachEntity((ref EntityName name, Entity entity) => {
        // ... 3 matches
    });
    
    // --- query components with tags
    var queryNamesWithTags  = store.Query<EntityName>().AllTags(Tags.Get<MyTag1, MyTag2>());
    queryNamesWithTags.ForEachEntity((ref EntityName name, Entity entity) => {
        // ... 1 match
    });
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

public struct Player : IIndexedComponent<string>
{
    public  string  name;
    public  string  GetIndexedValue() => name;
}

[Test]
public static void IndexedComponents()
{
    var store   = new EntityStore();
    for (int n = 0; n < 1000; n++) {
        var entity = store.CreateEntity();
        entity.AddComponent(new Player { name = $"Player-{n,0:000}"});
    }
    var lookup = store.GetEntitiesWithComponentValue<Player,string>("Player-001");  // O(1)
    Console.WriteLine($"lookup: {lookup.Count}");                                   // > lookup: 1
    
    var query      = store.Query().HasValue    <Player,string>("Player-001");       // O(1)
    Console.WriteLine($"query: {query.Count}");                                     // > query: 1
    
    var rangeQuery = store.Query().ValueInRange<Player,string>("Player-000", "Player-099");
    Console.WriteLine($"range query: {rangeQuery.Count}");                          // > range query: 100
    
    var names = store.GetIndexedComponentValues<Player,string>();                   // O(1)
    Console.WriteLine($"unique names: {names.Count}");                              // > unique names: 1000
}

public struct FollowComponent : ILinkComponent
{
    public  Entity  target;
    public  Entity  GetIndexedValue() => target;
}

[Test]
public static void Relationships()
{
    var store    = new EntityStore();
    var entities = new List<Entity>();
    for (int n = 0; n < 2000; n++) {
        entities.Add(store.CreateEntity());
    }
    for (int n = 0; n < 1000; n++) {
        entities[n + 1000].AddComponent(new FollowComponent { target = entities[n] });
    }
    var followers = entities[0].GetLinkingEntities<FollowComponent>();          // O(1)
    Console.WriteLine($"followers: {followers.Count}");                         // > followers: 1
    
    var query = store.Query().HasValue<FollowComponent, Entity>(entities[0]);   // O(1)
    Console.WriteLine($"query: {query.Count}");                                 // > query: 1
    
    var targets = store.GetLinkedEntities<FollowComponent>();                   // O(1)
    Console.WriteLine($"unique targets: {targets.Count}");                      // > unique targets: 1000
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
public static void JsonSerialization()
{
    var store = new EntityStore();
    store.CreateEntity(new EntityName("hello JSON"));
    store.CreateEntity(new Position(1, 2, 3));

    // --- Write store entities as JSON array
    var serializer = new EntitySerializer();
    var writeStream = new FileStream("entity-store.json", FileMode.Create);
    serializer.WriteStore(store, writeStream);
    writeStream.Close();
    
    // --- Read JSON array into new store
    var targetStore = new EntityStore();
    serializer.ReadIntoStore(targetStore, new FileStream("entity-store.json", FileMode.Open));
    
    Console.WriteLine($"entities: {targetStore.Count}"); // > entities: 2
}

}

}
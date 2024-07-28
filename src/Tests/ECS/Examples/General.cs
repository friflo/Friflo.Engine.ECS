using System;
using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;


// ReSharper disable UnusedVariable
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://friflo.gitbook.io/friflo.engine.ecs/examples/general
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
    // > entity id: 1  []       Info:  [] entity has no components
    // > entity id: 2  []
}

[Test]
public static void DeleteEntity()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.DeleteEntity();
    var isDeleted = entity.IsNull;
    Console.WriteLine($"deleted: {isDeleted}");     // > deleted: True
}

[Test]
public static void DisableEntity()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.Enabled = false;
    Console.WriteLine(entity);                      // > id: 1  [#Disabled]

    var query    = store.Query();
    Console.WriteLine($"default - {query}");        // > default - Query: []  Count: 0

    var disabled = store.Query().WithDisabled();
    Console.WriteLine($"disabled - {disabled}");    // > disabled - Query: []  Count: 1
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
    entity.AddComponent(new EntityName("Hello World!"));// EntityName is build-in
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
    store.CreateEntity(new UniqueEntity("Player"));     // UniqueEntity is build-in

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
    Console.WriteLine($"entity: {entity}");     // > entity: id: 1  [#MyTag1, #MyTag2]

    // get tag
    var tag1 = entity.Tags.Has<MyTag1>();
    Console.WriteLine($"tag1: {tag1}");         // > tag1: True
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
    var namesWithTags  = store.Query<EntityName>().AllTags(Tags.Get<MyTag1, MyTag2>());
    namesWithTags.ForEachEntity((ref EntityName name, Entity entity) => {
        // ... 1 match
    });
    // --- use query.Entities in case an iteration requires no component access
    foreach (var entity in queryNames.Entities) {
        // ... 3 matches
    }
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

    Console.WriteLine($"entities: {root.ChildEntities}"); // > entities: Count: 2
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
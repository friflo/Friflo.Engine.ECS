using System;
using System.IO;
using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;

#if !UNITY_5_3_OR_NEWER
using System.Runtime.Intrinsics;
#endif


// ReSharper disable UnusedParameter.Local
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable UnusedVariable
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable MemberCanBePrivate.Global
namespace Tests.ECS {

public static class Examples
{

// --------------------------------------------------------------------------------------
// README.md: Examples
// https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#examples

public struct Velocity : IComponent { public Vector3 value; } // requires >= 1.19.0

[Test]
public static void HelloWorld()
{
    var world = new EntityStore();
    for (int n = 0; n < 10; n++) {
        world.CreateEntity(new Position(n, 0, 0), new Velocity{ value = new Vector3(0, n, 0)});
    }
    var query = world.Query<Position, Velocity>();
    query.ForEachEntity((ref Position position, ref Velocity velocity, Entity entity) => {
        position.value += velocity.value;
    });
}

[Test]
public static void HelloSystem()
{
    var world = new EntityStore();
    for (int n = 0; n < 10; n++) {
        world.CreateEntity(new Position(n, 0, 0), new Velocity(), new Scale3());
    }
    var root = new SystemRoot(world) {
        new MoveSystem(),
        // Hundreds of systems can be added. The execution order still remains clear.
    };
    root.Update(default);
}
        
class MoveSystem : QuerySystem<Position, Velocity>
{
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Position position, ref Velocity velocity, Entity _) => {
            position.value += velocity.value;
        });
    }
}

class PulseSystem : QuerySystem<Scale3>
{
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Scale3 scale, Entity _) => {
            scale.value = Vector3.One * (1 + 0.2f * MathF.Sin(4 * Tick.time));
        });
    }
}

// --------------------------------------------------------------------------------------
// Wiki: Examples - General
// https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General
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
    var entity  = store.CreateEntity(new UniqueEntity("Player")); // UniqueEntity is a build-in component
    
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
public static void JsonSerialization()
{
    var store = new EntityStore();
    var entity1 = store.CreateEntity(new EntityName("hello JSON"));
    var entity2 = store.CreateEntity(new Position(1, 2, 3));

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

[Test]
public static void EntityQueries()
{
    var store   = new EntityStore();
    
    var entity1 = store.CreateEntity(new EntityName("test"), Tags.Get<MyTag1>());
    var entity2 = store.CreateEntity(Tags.Get<MyTag1>());
    var entity3 = store.CreateEntity(Tags.Get<MyTag1, MyTag2>());
    
    // --- query components
    var queryNames = store.Query<EntityName>();
    Console.WriteLine($"EntityName:     {queryNames.Entities}");    // > EntityName:     Entity[1]
    
    // --- query tags
    var queryTag  = store.Query().AllTags(Tags.Get<MyTag1>());
    Console.WriteLine($"MyTag1:         {queryTag.Entities}");      // > MyTag1:         Entity[3]
    
    var queryTags = store.Query().AllTags(Tags.Get<MyTag1, MyTag2>());
    Console.WriteLine($"MyTag1, MyTag2: {queryTags.Entities}");     // > MyTag1, MyTag2: Entity[1]
}

// --------------------------------------------------------------------------------------
// Wiki: Examples - Optimization
// https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization

[Test]
public static void EnumerateQueryChunks()
{
    var store   = new EntityStore();
    for (int n = 0; n < 3; n++) {
        store.CreateEntity(new MyComponent{ value = n + 42 });
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
        store.CreateEntity(new MyComponent());
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

#if !UNITY_5_3_OR_NEWER
[Test]
public static void QueryVectorization()
{
    var store   = new EntityStore();
    for (int n = 0; n < 10_000; n++) {
        store.CreateEntity(new MyComponent());
    }
    var query = store.Query<MyComponent>();
    foreach (var (component, entities) in query.Chunks)
    {
        // increment all MyComponent.value's. add = <1, 1, 1, 1, 1, 1, 1, 1>
        var add     = Vector256.Create<int>(1);         // create int[8] vector - all values = 1
        var values  = component.AsSpan256<int>();       // values.Length - multiple of 8
        var step    = component.StepSpan256;            // step = 8
        for (int n = 0; n < values.Length; n += step) {
            var slice   = values.Slice(n, step);
            var result  = Vector256.Create<int>(slice) + add; // execute 8 add instructions in one CPU cycle
            result.CopyTo(slice);
        }
    }
}
#endif

[Test]
public static void FilterEntityEvents()
{
    var store   = new EntityStore();
    store.EventRecorder.Enabled = true; // required for EventFilter
    
    store.CreateEntity(new Position(), Tags.Get<MyTag1>());
    
    var query = store.Query();
    query.EventFilter.ComponentAdded<Position>();
    query.EventFilter.TagAdded<MyTag1>();
    
    foreach (var entity in store.Entities)
    {
        bool hasEvent = query.HasEvent(entity.Id);
        Console.WriteLine($"{entity} - hasEvent: {hasEvent}");
    }
    // > id: 1  [] - hasEvent: False
    // > id: 2  [Position] - hasEvent: True
    // > id: 3  [#MyTag1] - hasEvent: True
}

[Test]
public static void CreateEntityBatch()
{
    var store   = new EntityStore();
    var entity  = store.Batch()
        .Add(new EntityName("test"))
        .Add(new Position(1,1,1))
        .CreateEntity();
    Console.WriteLine($"entity: {entity}");             // > entity: id: 1  "test"  [EntityName, Position]

    // Create a batch - can be cached if needed.
    var batch = new CreateEntityBatch(store).AddTag<MyTag1>();
    for (int n = 0; n < 10; n++) {
        batch.CreateEntity();
    }
    var taggedEntities = store.Query().AllTags(Tags.Get<MyTag1>());
    Console.WriteLine(taggedEntities);                  // > Query: [#MyTag1]  Count: 10
}

[Test]
public static void EntityBatch()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    
    entity.Batch()
        .Add(new Position(1, 2, 3))
        .AddTag<MyTag1>()
        .Apply();
    
    Console.WriteLine($"entity: {entity}");             // > entity: id: 1  [Position, #MyTag1]
}

[Test]
public static void BulkBatch()
{
    var store   = new EntityStore();
    for (int n = 0; n < 1000; n++) {
        store.CreateEntity();
    }
    var batch = new EntityBatch();
    batch.Add(new Position(1, 2, 3)).AddTag<MyTag1>();
    store.Entities.ApplyBatch(batch);
    
    var query = store.Query<Position>().AllTags(Tags.Get<MyTag1>());
    Console.WriteLine(query);                           // > Query: [Position, #MyTag1]  Count: 1000
    
    // Same as: store.Entities.ApplyBatch(batch) above
    foreach (var entity in store.Entities) {
        batch.ApplyTo(entity);
    }
}

[Test]
public static void EntityList()
{
    var store   = new EntityStore();
    var root    = store.CreateEntity();
    for (int n = 0; n < 10; n++) {
        var child = store.CreateEntity();
        root.AddChild(child);
        // Add two children to each child
        child.AddChild(store.CreateEntity());
        child.AddChild(store.CreateEntity());
    }
    var list = new EntityList(store);
    // Add root and all its children to the list
    list.AddTree(root);
    Console.WriteLine($"list - {list}");                // > list - Count: 31
    
    var batch = new EntityBatch();
    batch.Add(new Position());
    list.ApplyBatch(batch);
    
    var query = store.Query<Position>();
    Console.WriteLine(query);                           // > Query: [Position]  Count: 31
}

[Test]
public static void CommandBuffer()
{
    var store   = new EntityStore();
    var entity1 = store.CreateEntity(new Position());
    var entity2 = store.CreateEntity();
    
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

}
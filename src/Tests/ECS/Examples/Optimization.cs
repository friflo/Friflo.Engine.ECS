using System;
using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static Tests.Examples.General;

#if !UNITY_5_3_OR_NEWER
using System.Runtime.Intrinsics;
#endif

// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedVariable
// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization
public static class Optimization
{

[Test]
public static void BoostedQuery()
{
    var store   = new EntityStore();
    for (int n = 0; n < 100; n++) {
        store.CreateEntity(new Position(), new Velocity());
    }
    var query = store.Query<Position, Velocity>();
    query.Each(new MoveEach()); // requires https://www.nuget.org/packages/Friflo.Engine.ECS.Boost
}

struct MoveEach : IEach<Position, Velocity>
{
    public void Execute(ref Position position, ref Velocity velocity) {
        position.value += velocity.value;
    }
} 

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
        for (int n = 0; n < entities.Length; n++) {
            Console.WriteLine(components[n].value);                  // > 42  43  44
        }
    }
    // Caution! This alternative to iterate components is much slower
    foreach (var entity in query.Entities) {
        Console.WriteLine(entity.GetComponent<MyComponent>().value); // > 42  43  44
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
        for (int n = 0; n < entities.Length; n++) {
            myComponents[n].value += 10;
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
        var add     = Vector256.Create<int>(1);     // create int[8] vector - all values = 1
        var values  = component.AsSpan256<int>();   // values.Length - multiple of 8
        var step    = component.StepSpan256;        // step = 8
        for (int n = 0; n < values.Length; n += step) {
            var slice   = values.Slice(n, step);
            var result  = Vector256.Create<int>(slice) + add; // 8 add ops in one CPU cycle
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
public static void CreateEntityOperation()
{
    var store   = new EntityStore();
    for (int n = 0; n < 10; n++) {
        store.CreateEntity(new EntityName("test"), new Position(), Tags.Get<MyTag1>());
    }
    var taggedEntities = store.Query().AllTags(Tags.Get<MyTag1>());
    Console.WriteLine(taggedEntities);      // > Query: [#MyTag1]  Count: 10
}

[Test]
public static void CreateEntities()
{
    var store     = new EntityStore();
    var archetype = store.GetArchetype(ComponentTypes.Get<Position, Scale3>(), Tags.Get<MyTag1>());
    var entities  = archetype.CreateEntities(100_000);  // ~ 0.5 ms
    Console.WriteLine(entities.Count);                  // 100000
}

/// Obsolete! Prefer using significant more performant <c>CreateEntity()</c> overloads used
/// in <see cref="CreateEntityOperation"/> above.
[Test]
public static void CreateEntityBatch_Obsolete()
{
    var store   = new EntityStore();
    var entity  = store.Batch()
        .Add(new EntityName("test"))
        .Add(new Position(1,1,1))
        .CreateEntity();
    Console.WriteLine($"{entity}");         // > id: 1  "test"  [EntityName, Position]

    // Create a batch - can be cached if needed.
    var batch = new CreateEntityBatch(store).AddTag<MyTag1>();
    for (int n = 0; n < 10; n++) {
        batch.CreateEntity();
    }
    var taggedEntities = store.Query().AllTags(Tags.Get<MyTag1>());
    Console.WriteLine(taggedEntities);      // > Query: [#MyTag1]  Count: 10
}

[Test]
public static void EntityBatchOperations()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    
    // batch: add operations
    entity.Add(
        new Position(1, 2, 3),
        new Scale3(4, 5, 6),
        new EntityName("test"),
        Tags.Get<MyTag1>());
    Console.WriteLine(entity); // id: 1  "test"  [EntityName, Position, Scale3, #MyTag1]
    
    // batch: get operations
    var data    = entity.Data; // introduced in 3.0.0-preview.7
    var pos     = data.Get<Position>();
    var scale   = data.Get<Scale3>();
    var name    = data.Get<EntityName>();
    var tags    = data.Tags;
    Console.WriteLine($"({pos}),({scale}),({name})"); // (1, 2, 3), (4, 5, 6), ('test')
    Console.WriteLine(tags);                          // Tags: [#MyTag1]
    
    // batch: remove operations
    entity.Remove<Position, Scale3, EntityName>(Tags.Get<MyTag1>());
    Console.WriteLine(entity); // id: 1  []
}

/// An EntityBatch should only be used when adding <b>AND</b> removing components / tags to the same entity.<br/>
/// If only adding <b>OR</b> removing components / tags use the <c>Add() / Remove()</c> overloads shown above.
[Test]
public static void EntityBatch()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    
    entity.Batch()
        .Add(new Position(1, 2, 3))
        .Add(new EntityName("test"))
        .Remove<Scale3>()   // <- now batch is a combination of add & remove
        .AddTag<MyTag1>()
        .Apply();
    
    Console.WriteLine(entity); // id: 1  "test"  [EntityName, Position, #MyTag1]
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
    Console.WriteLine(query);               // > Query: [Position, #MyTag1]  Count: 1000
    
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
    Console.WriteLine($"list - {list}");        // > list - Count: 31
    
    var batch = new EntityBatch();
    batch.Add(new Position());
    list.ApplyBatch(batch);
    
    var query = store.Query<Position>();
    Console.WriteLine(query);                   // > Query: [Position]  Count: 31
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
    Console.WriteLine(entity1);     // > id: 1  "changed entity"  [EntityName, #MyTag1]
    Console.WriteLine(entity2);     // > id: 2  (detached)
    Console.WriteLine(entity3);     // > id: 3  "new entity"  [EntityName]
}

}

}
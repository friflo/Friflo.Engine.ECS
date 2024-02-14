# [![JSON Fliox](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/Json-Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md)    **Friflo.Engine.ECS** ![splash](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/paint-splatter.svg)

[![nuget](https://img.shields.io/nuget/v/Friflo.Engine.ECS.svg?color=blue)](https://www.nuget.org/packages/Friflo.Engine.ECS) 
[![CI-Engine](https://github.com/friflo/Friflo.Json.Fliox/workflows/CI-Engine/badge.svg)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/engine.yml) 
[![CD-Engine](https://github.com/friflo/Friflo.Json.Fliox/workflows/CD-Engine/badge.svg)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/nuget-engine.yml) 


The package **Friflo.Engine.ECS** is part of an in-development Game Editor which is documented at [Architecture.md](Architecture.md).

**Friflo.Engine.ECS** implements an [Entity Component System - Wikipedia](https://en.wikipedia.org/wiki/Entity_component_system).

# Entity Component System - ECS

The core feature of an Entity Component System are:

1. Data is organized by a set of entities. Each entity contains an arbitrary set of components.  
  Components can be added / removed to / from an entities at any time.  
  This software pattern is used to avoid deep class inheritance -
  a characteristic specific to [OOP - Wikipedia](https://en.wikipedia.org/wiki/Object-oriented_programming).  
  It simplifies the creation of decoupled code which is harder to achieve in OOP.

2. Entity queries from an entity container are fast and efficient compared to queries in an OOP architecture.  
  E.g. The runtime complexity of a query returning 100 entities is **O(100)**.  
  Independent from the amount of entities stored in a container. E.g. 1.000.000.  
  The trivial approach in OOP would be **O(1.000.000)**.

3. Entity components are stored as `struct`s in continuous memory.   
  This improves query enumeration performance as L1 cache misses are very unlikely and  
  all bytes stored in L1 cache lines - typically 64 or 128 - are utilized.  


## Features

- Performance
    - Use array buffers and cache query instances -> no memory allocations after buffers are large enough.
    - High memory locality by storing components in continuous memory.
    - Optimized for high L1 cache line hit rate.
    - Very good benchmark results at: [Ecs.CSharp.Benchmark - GitHub](https://github.com/Doraku/Ecs.CSharp.Benchmark).
    - Processing components of large queries has the memory bandwidth as bottleneck. Either using multi threading or SIMD.  
      Alternative ECS implementations using C/C++, Rust, Zig or Mojo 🔥 cannot be faster due to the physical limits.
- Developer friendly / OOP like API by exposing the [Entity API](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/Entity.md)
  **struct** as the main interface.  
  Or compare the `Entity` API with other API's at [Engine-comparison.md](Engine-comparison.md).  
  The typical alternative of an ECS implementations is providing a `World` class and using `int` parameters as entity `id`s.
- JSON Serialization
- Record entity changes on arbitrary threads using [CommandBuffer](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/CommandBuffer.md)'s.
- Build a hierarchy of entities typically used in Games and Game Editors.
- Support multi threaded component queries (systems).
- Support for Vectorization (SIMD) of components returned by queries.  
  Returned component arrays have padding elements at the end to enable SIMD processing without a
  [scalar remainder (epilogue) loop](https://llvm.org/docs/Vectorizers.html#epilogue-vectorization).  
  It is preferred over multi threading as it uses only one core providing the same performance as multi threading running on all cores.
- Minimize times required for GC collection by using struct types for entities and components.  
  GC.Collect(1) < 0.8 ms when using 10.000.000 entities.
- Support tagging of entities and use them as a filter in queries.
- Add scripts - similar to `MonoBehavior`'s - to entities in cases OOP is preferred.
- Support observing entity changes by event handlers triggered by adding / removing: components, tags, scripts and child entities.
- Enable binding an entity hierarchy to a [TreeDataGrid - GitHub](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid)
  in [AvaloniaUI - Website](https://avaloniaui.net/). Screenshot below:    
<img src="docs/images/Friflo-Engine-Editor.png" width="677" height="371"></img>


## Latest features

Features in this list are also explained in the Examples.

- 1.10.0    Add support for [CommandBuffer](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/CommandBuffer.md)'s.  

- 1.11.0    Support to filter entity changes - like adding/removing components/tags - in queries using an
            [EventFilter](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/EventFilter.md).  

- 1.12.0    Add additional query filters like `WithoutAnyTags()` using an
            [ArchetypeQuery](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/ArchetypeQuery.md).  

- 1.13.0    Add support for target framework .NET Standard 2.1 or higher.

- 1.14.0    Add support for parallel (multi threaded) query job execution.

- 1.15.0    Reduced the number of properties shown for an entity in the debugger. See screenshot in Examples. 

- 1.16.0 (planned) **Changed behavior** of the Archetype assigned to entities without components & tags.  
            *Now:*    Entities are stored in this archetype.  
            *Before:* Entities were not stored. `Archetype.Entities` returned always an empty result.



## Development

The library supports target framework **.NET Standard 2.1** or higher.  
This includes .NET Core, .NET 5, .NET 6, .NET 7 and .NET 8.  
Tested: Windows, macOS and Linux. Untested: Android, iOS, tvOS and WASM/WebAssembly.

The library can be build on all platforms a .NET SDK is available.  
Build options:
- `dotnet` CLI       - Windows, macOS, Linux
- Rider              - Windows, macOS, Linux (untested)
- Visual Studio 2022 - Windows
- Visual Studio Code - Windows, macOS, Linux (untested)

Library:
- Build time Windows: ~ 5 seconds, macOS (M2): 2,5 seconds.
- Code coverage of the unit tests: 99,9%. See: [docs/code-coverage.md](docs/code-coverage.md).
- Unit test execution: ~ 1 second.
- Size of `Friflo.Engine.ECS.dll`: ~ 180 kb. The implementation: ~ 14.000 LOC.
- Pure C# implementation - no C/C++ bindings slowing down runtime / development performance.
- The library is not using [unsafe code](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code).
- The C# API is [CLS-compliant](https://learn.microsoft.com/en-us/dotnet/api/system.clscompliantattribute?view=net-8.0#remarks).
- No 3rd party dependencies.
- It requires **Friflo.Json.Fliox** which is part of this repository.

<br/><br/>


# Examples

Examples using **Friflo.Engine.ECS** are part of the unit tests see: [Tests/ECS/Examples.cs](Tests/ECS/Examples.cs)

When testing the examples use a debugger to check entity state changes while stepping throw the code.

<img src="docs/images/entity-debugger.png" width="742" height="305"></img>  
*Screenshot:* All relevant entity data is directly available in the debugger.

Examples showing typical use cases of the [Entity API](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/Entity.md)

- [EntityStore](#entitystore)
- [Entity](#entity)
- [Component](#component)
- [Unique entity](#unique-entity)
- [Tag](#tag)
- [Script](#script)
- [Child entities](#child-entities)
- [Event](#event)
- [Signal](#signal)
- [Query](#query)
- [Enumerate Query Chunks](#enumerate-query-chunks)
- [Parallel Query Job](#parallel-query-job)
- [Query Vectorization - SIMD](#query-vectorization---simd)
- [EventFilter](#eventfilter)
- [CommandBuffer](#commandbuffer)



## EntityStore

An `EntityStore` is a container for entities running as an in-memory database.  
It is highly optimized for efficient storage fast queries and event handling.

The entity store enables to
- create entities
- modify entities - add / remove components, tags, scripts and child entities
- query for entities with a specific set of components or tags
- subscribe events like adding / removing components, tags, scripts and child entities

Multiple stores can be used in parallel and act completely independent from each other.  
The example shows how to create a store. Mainly every example will start with this line.

```csharp
public static void CreateStore()
{
    var store = new EntityStore();
}
```


## Entity

An `Entity` has an identity - `Id` - and acts as a container for components, tags, script and child entities.  
Entities are related to a single `EntityStore` and created with `CreateEntity()`.

```csharp
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
```


## Component

`Components` are `struct`s used to store data for entity / fields.  
Multiple components can be added / removed to / from an entity.  

```csharp
[ComponentKey("my-component")]
public struct MyComponent : IComponent {
    public int value;
}

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
```

Result of `entity.DebugJSON`:
```json
{
    "id": 1,
    "components": {
        "name": {"value":"Hello World!"},
        "my-component": {"value":42}
    }
}
```


## Unique entity

Add a `UniqueEntity` component to an entity to mark it as a *"singleton"* with a unique `string` id.  
The entity can than be retrieved with `EntityStore.GetUniqueEntity()` to reduce code coupling.  
It enables access to a unique entity without the need to pass an entity by external code.   

```csharp
public static void GetUniqueEntity()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddComponent(new UniqueEntity("Player"));    // UniqueEntity is a build-in component
    
    var player  = store.GetUniqueEntity("Player");
    Console.WriteLine($"entity: {player}");             // > entity: id: 1  [UniqueEntity]
}
```


## Tag

`Tags` are `struct`s similar to components - except they store no data.  
They can be utilized in queries similar as components to restrict the amount of entities returned by a query. 

```csharp
public struct MyTag1 : ITag { }
public struct MyTag2 : ITag { }

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
```


## Script

`Script`s are similar to components and can be added / removed to / from entities.  
`Script`s are classes and can be used to store data.  
Additional to components they enable adding behavior in the common OOP style.

In case dealing only with a few thousands of entities `Script`s are fine.  
If dealing with a multiple of 10.000 components should be used for efficiency / performance.

```csharp
public class MyScript : Script { }

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
```


## Child entities

A typical use case in Games or Editor is to build up a hierarchy of entities.  

```csharp
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
```


## Event

If changing an entity by adding or removing components, tags, scripts or child entities events are emitted.  
An application can subscribe to these events like shown in the example.  
Emitting these type of events increase code decoupling.  
Without events these modifications need to be notified by direct method calls.  
The *build-in* events can be subscribed on `EntityStore` and on `Entity` level like shown in the example below.  

```csharp
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
```

## Signal

`Signal`s are similar to events. They are used to send and receive custom events on entity level in an application.  
They have the same characteristics as events described in the section above.  

```csharp
public readonly struct MySignal { } 

public static void AddSignalHandler()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddSignalHandler<MySignal>(signal => { Console.WriteLine(signal); }); // > entity: 1 - signal > MySignal    
    entity.EmitSignal(new MySignal());
}
```


## Query

As described in the intro queries are a fundamental feature of an ECS.  
**Friflo.Engine.ECS** support queries by any combination of component types and tags.

See [ArchetypeQuery - API](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/ArchetypeQuery.md)
for available query filters to reduce the number of entities / components returned by a query.


```csharp
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
```

Some optional filter snippets used to shrink the result set returned by a query.

```csharp
    .AllTags(Tags.Get<MyTag1>());                   // query will contain only entities having all given tags
    .WithoutAnyTags(Tags.Get<MyTag1, MyTag2>());    // entities having any of the given tags are excluded from query
    .AllComponents(ComponentTypes.Get<Position>);   // query will contain only entities having all given components
```


## Enumerate Query Chunks

Also as described in the intro enumeration of a query result is fundamental for an ECS.  
Components are returned as [Chunk](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/Chunk_T_.md)'s and are suitable for
[Vectorization - SIMD](https://en.wikipedia.org/wiki/Single_instruction,_multiple_data).

```csharp
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
```


## Parallel Query Job

To minimize execution time for large queries a [QueryJob](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/QueryJob.md) can be used.  
It provides the same functionality as the **foreach** loop in example above but runs on multiple cores in parallel. E.g.
```csharp
    foreach (var (components, entities) in query.Chunks) { ... }
```
To enable running a query job a [ParallelJobRunner](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/ParallelJobRunner.md) is required.  
The runner can be assigned to the `EntityStore` or directly to the `QueryJob`.  
A `ParallelJobRunner` instance is thread-safe and can / should be used for multiple / all query jobs.

```csharp
public static void ParallelQueryJob()
{
    var runner  = new ParallelJobRunner(Environment.ProcessorCount);
    var store   = new EntityStore { JobRunner = runner };
    for (int n = 0; n < 10_000; n++) {
        store.CreateEntity().AddComponent<MyComponent>();
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
```
In case of structural changes inside the `ForEach((...) => {...})` delegate use a
[CommandBuffer](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/CommandBuffer.md)
to record the changes.  
Structural changes are adding / removing components, tags or child entities and the creation / deletion of entities.  
After `RunParallel()` returns these changes can be applied to the `EntityStore` by calling `CommandBuffer.Playback()`.


## Query Vectorization - SIMD

The most efficient way to speedup query execution is vectorization.  
Vectorization is similar to loop unrolling - aka loop unwinding - but with hardware support.  
Its efficiency is superior to multi threading as it requires only a single thread to achieve the same performance gain.  
So other threads can still keep running without competing for CPU resources.  

*Note:* Vectorization can be combined with multi threading to speedup execution even more.  
In case of a system with high memory bandwidth the speedup is *speedup(SIMD) * speedup(multi threading)*.  
If SIMD or multi threading alone already reaches this bandwidth bottleneck their combination provide no performance gain.

The API provide a few methods to convert chunk components into [System.Runtime.Intrinsics - Vectors](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics).  
E.g. `AsSpan256<>` and `StepSpan256`. See all methods at the [Chunk - API](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/Chunk_T_.md).  
The `Span` retrieved from a  chunk component has padding components at the end to enable vectorization without a scalar remainder loop.

The following examples shows how to increment all `MyComponent.value`'s by 1.  

```csharp
public static void QueryVectorization()
{
    var store   = new EntityStore();
    for (int n = 0; n < 10_000; n++) {
        store.CreateEntity().AddComponent<MyComponent>();
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
```


## EventFilter

An alternative to process entity changes - see section [Event](#event) - are `EventFilter`'s.  
`EventFilter`'s can be used on its own or within a query like in the example below.  
All events that need to be filtered - like added/removed components/tags - can be added to the `EventFilter`.  
E.g. `ComponentAdded<Position>()` or `TagAdded<MyTag1>`.

```csharp
public static void FilterEntityEvents()
{
    var store   = new EntityStore();
    store.EventRecorder.Enabled = true; // required for EventFilter
    
    store.CreateEntity();
    store.CreateEntity().AddComponent<Position>();
    store.CreateEntity().AddTag      <MyTag1>();
    
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
```

## CommandBuffer

A `CommandBuffer` is used to record changes on multiple entities. E.g. `AddComponent()`.  
These changes are applied to entities when calling `Playback()`.    
Recording commands with a `CommandBuffer` instance can be done on **any** thread.  
`Playback()` must be called on the **main** thread.  
Available commands are in the [CommandBuffer - API](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/CommandBuffer.md).  

This enables recording entity changes in multi threaded application using entity systems / queries.  
In this case enumerations of query results run on multiple worker threads.  
Within these enumerations entity changes are recorded with a `CommandBuffer`.  
After a query thread has finished these changes are executed with `Playback()` on the **main** thread.

```csharp
public static void CommandBuffer()
{
    var store   = new EntityStore();
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
```
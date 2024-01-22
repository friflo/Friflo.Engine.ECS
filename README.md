# [![JSON Fliox](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/Json-Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine)    **Friflo.Engine.ECS** ![splash](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/paint-splatter.svg)

[![nuget](https://img.shields.io/nuget/v/Friflo.Engine.ECS.svg?color=blue)](https://www.nuget.org/packages/Friflo.Engine.ECS) 
[![CI-Engine](https://github.com/friflo/Friflo.Json.Fliox/workflows/CI-Engine/badge.svg)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/engine.yml) 
[![CD-Engine](https://github.com/friflo/Friflo.Json.Fliox/workflows/CD-Engine/badge.svg)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/nuget-engine.yml) 


The package **Friflo.Engine.ECS** is part of an in-development Game Editor which is documented at [Architecture.md](Architecture.md).

**Friflo.Engine.ECS** implements an [Entity Component System](https://en.wikipedia.org/wiki/Entity_component_system).

## Entity Component System - ECS

The core feature of an Entity Component System are:

1. Data is organized by a set of entities. Each entity contains an arbitrary set of components.  
  Components can be added / removed to / from an entities at any time.  
  This software pattern is used to avoid deep class inheritance -
  a characteristic specific to [OOP](https://en.wikipedia.org/wiki/Object-oriented_programming).  
  It simplifies the creation of decoupled code which is harder to achieve in OOP.

2. Entity queries from an entity container are fast and efficient compared to queries in an OOP architecture.  
  E.g. The runtime complexity of a query returning 100 entities is **O(100)**.  
  Independent from the amount of entities stored in a container. E.g. 1.000.000.  
  The trivial approach in OOP would be **O(1.000.000)**.

3. Entity components are stored as `struct`s in continuous memory.   
  This improves query enumeration performance as L1 cache misses are very unlikely and  
  all bytes stored in L1 cache lines - typically 64 or 128 - are utilized.  


## Additional library features

- JSON Serialization
- Developer friendly / OOP like API by exposing the [Entity](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/Entity.md) struct as the main interface.  
  To get an overview of the `Entity` interface see [Engine-comparison.md](Engine-comparison.md).  
  The typical alternative of an ECS implementations is providing a `World` class and using `int` parameters as entity `id`s.
- Build a hierarchy of entities typically used in Games and Game Editors.
- Support for Vectorization (SIMD) of components returned by queries.
- Minimize times required for GC collection by using struct types for entities and components.  
  GC.Collect(1) < 0.8 ms when using 10.000.000 entities.
- Support tagging of entities and use them as a filter in queries.
- Add scripts - similar to `MonoBehavior`'s - to entities in cases OOP is preferred.
- Enable binding an entity hierarchy to a [TreeDataGrid](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid)
  in [AvaloniaUI](https://avaloniaui.net/). Screenshot below:    
<img src="docs/images/Friflo-Engine-Editor.png" width="677" height="371"></img>



## Development

The library can be integrated on all **.NET** supported platforms:  
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
- Size of `Friflo.Engine.ECS.dll`: ~ 140 kb. The implementation: ~ 10.000 LOC.
- Pure C# implementation - no C/C++ bindings slowing down runtime / development performance.
- No 3rd party dependencies.
- It requires **Friflo.Json.Fliox** which is part of this repository.
- The library C# API is [CLS-compliant](https://learn.microsoft.com/en-us/dotnet/api/system.clscompliantattribute?view=net-8.0#remarks)

<br/><br/>


# Examples

Examples using **Friflo.Engine.ECS** are part of the unit tests see: [Tests/ECS/Examples.cs](Tests/ECS/Examples.cs)

When testing the examples use a debugger to check entity state changes while stepping throw the code.

<img src="docs/images/entity-debugger.png" width="593" height="270"></img>  
*Screenshot:* All relevant entity data is directly available in the debugger.


### Add components to an entity

`Components` are `struct`s used to store data for entity / fields.  
Multiple components can be added / removed to / from an entity.  

```csharp
public struct MyComponent : IComponent {
    public int value;
};

public static void AddComponents()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddComponent(new EntityName("Hello World!")); // EntityName is a build-in component
    entity.AddComponent(new MyComponent { value = 42 });
    Console.WriteLine($"entity: {entity}");     // > entity: id: 1  "Hello World!"  [EntityName, Position]
}
```


### Add tags to an entity

`Tags` are `struct`s similar to components - except they store no data.  
They can be utilized in queries similar as components to restrict the amount of entities returned by a query. 

```csharp
public struct MyTag1 : ITag { };
public struct MyTag2 : ITag { };

public static void AddTags()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddTag<MyTag1>();
    entity.AddTag<MyTag2>();
    Console.WriteLine($"entity: {entity}");     // > entity: id: 1  [#MyTag1, #MyTag2]
}
```


### Add scripts to an entity

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
    entity.AddScript(new MyScript());
    Console.WriteLine($"entity: {entity}");     // > entity: id: 1  [*MyScript]
}
```

### Add entities as children to an entity

A typical use case in Games or Editor is to build up a hierarchy of entities.  

```csharp
public static void AddChildEntities()
{
    var store   = new EntityStore();
    var root    = store.CreateEntity();
    var child1  = store.CreateEntity();
    var child2  = store.CreateEntity();
    root.AddChild(child1);
    root.AddChild(child2);
    Console.WriteLine($"child entities: {root.ChildEntities}"); // > child entities: Count: 2
}
```

### Add event handlers to an entity

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

### Add signal handlers to an entity

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

### Create entity queries

As described in the intro queries are a fundamental feature of an ECS.  
**Friflo.Engine.ECS** support queries by any combination of component types and tags.

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
    Console.WriteLine(queryEntityNames);    // > Query: [EntityName]  EntityCount: 1

    var queryMyComponents = store.Query<MyComponent>();
    Console.WriteLine(queryMyComponents);   // > Query: [MyComponent]  EntityCount: 2
    
    // --- query tags
    var queryTag  = store.Query().AllTags(Tags.Get<MyTag1>());
    Console.WriteLine(queryTag);            // > Query: [#MyTag1]  EntityCount: 3
    
    var queryTags = store.Query().AllTags(Tags.Get<MyTag1, MyTag2>());
    Console.WriteLine(queryTags);           // > Query: [#MyTag1, #MyTag2]  EntityCount: 1
}
```

### Enumerate the `Chunks` of an entity query

Also as described in the intro enumeration of a query result is fundamental for an ECS.  
Components are returned as `Chunks` and are suitable for
[Vectorization - SIMD](https://en.wikipedia.org/wiki/Single_instruction,_multiple_data)

```csharp
public static void EnumerateQueryChunks()
{
    var store   = new EntityStore();
    for (int n = 0; n < 3; n++) {
        var entity = store.CreateEntity();
        entity.AddComponent(new MyComponent{ value = n + 42 });
    }
    var queryMyComponents = store.Query<MyComponent>();
    foreach (var (components, entities) in queryMyComponents.Chunks)
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
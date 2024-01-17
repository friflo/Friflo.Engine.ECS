# [![JSON Fliox](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/Json-Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine)    **Friflo.Engine.ECS** ![splash](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/paint-splatter.svg)

[![nuget](https://img.shields.io/nuget/v/Friflo.Engine.ECS.svg?color=blue)](https://www.nuget.org/packages/Friflo.Engine.ECS) 
[![CI-Engine](https://github.com/friflo/Friflo.Json.Fliox/workflows/CI-Engine/badge.svg)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/engine.yml) 
[![CD-Engine](https://github.com/friflo/Friflo.Json.Fliox/workflows/CD-Engine/badge.svg)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/nuget-engine.yml) 


The package **Friflo.Engine.ECS** is part of an in-development Game Editor which is documented at [Architecture.md](Architecture.md).

**Friflo.Engine.ECS** implements an [Entity Component System](https://en.wikipedia.org/wiki/Entity_component_system).

The core feature of an Entity Component System - ECS - are:

1. Data is organized by a set of entities containing components.  
  Components can be added / removed to / from an entities at any time.  
  This software pattern is used to avoid deep class inheritance -
  a characteristic specific to [OOP](https://en.wikipedia.org/wiki/Object-oriented_programming).

2. Entity queries from an entity container are fast and efficient compared to queries in an OOP architecture.  
  E.g. The runtime complexity of a query returning 100 entities is **O(100)**.  
  Independent from the amount of entities in stored in a container. E.g. 1.000.000.  
  The trivial approach in OOP would be **O(1.000.000)** in this case.

3. Entity components are stored as `struct`s in continuous memory.   
  This improves query enumeration performance as L1 cache misses are very unlikely and  
  all the memory store in L1 cache lines are utilized.  
  Also the returned components are

## Additional features

- JSON Serialization
- Build a hierarchy of entities typically used in Games and Game Editors.
- Support for Vectorization (SIMD) of components returned by queries.
- Minimize times required for GC collection by using struct types for entities and components.  
  GC.Collect(1) < 0.8 ms when using 10.000.000 entities.
- Support tagging of entities and use them as a filter in queries.
- Add scripts - similar to `MonoBehavior`'s - to entities in cases OOP is preferred.
- Enable binding an entity hierarchy to a [TreeDataGrid](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid)
  in [AvaloniaUI](https://avaloniaui.net/).



## Examples

Examples using **Friflo.Engine.ECS** are part of the unit tests see: [Tests/ECS/Examples.cs](Tests/ECS/Examples.cs)


### Add components to an entity

```csharp
public struct MyComponent : IComponent {
    public int value;
};

public static void AddComponents()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddComponent(new EntityName("Hello World!"));
    entity.AddComponent(new MyComponent { value = 42 });
    Console.WriteLine($"entity: {entity}"); // entity: id: 1  "Hello World!"  [EntityName, Position]
}
```


### Add tags to an entity

```csharp
public struct MyTag1 : ITag { };
public struct MyTag2 : ITag { };

public static void AddTags()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddTag<MyTag1>();
    entity.AddTag<MyTag2>();
    Console.WriteLine($"entity: {entity}");     // entity: id: 1  [#MyTag1, #MyTag2]
}
```


### Add scripts to an entity

```csharp
public class MyScript : Script { } 

public static void AddScript()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddScript(new MyScript());
    Console.WriteLine($"entity: {entity}");     // entity: id: 1  [*MyScript]
}
```

### Add entities as children to an entity

```csharp
public static void AddChildEntities()
{
    var store   = new EntityStore();
    var root    = store.CreateEntity();
    var child1  = store.CreateEntity();
    var child2  = store.CreateEntity();
    root.AddChild(child1);
    root.AddChild(child2);
    Console.WriteLine($"child entities: {root.ChildEntities}"); // child entities: Count: 2
}
```

### Add event handlers to an entity

```csharp
public static void AddEventHandlers()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.OnComponentChanged     += ev => { Console.WriteLine(ev); }; // entity: 1 - event > Add Component: [MyComponent]
    entity.OnTagsChanged          += ev => { Console.WriteLine(ev); }; // entity: 1 - event > Add Tags: [#MyTag1]
    entity.OnScriptChanged        += ev => { Console.WriteLine(ev); }; // entity: 1 - event > Add Script: [*MyScript]
    entity.OnChildEntitiesChanged += ev => { Console.WriteLine(ev); }; // entity: 1 - event > Add Child[0] = 2

    entity.AddComponent(new MyComponent());
    entity.AddTag<MyTag1>();
    entity.AddScript(new MyScript());
    entity.AddChild(store.CreateEntity());
}
```

### Add signal handlers to an entity

```csharp
public readonly struct MySignal { } 

public static void AddSignalHandler()
{
    var store   = new EntityStore();
    var entity  = store.CreateEntity();
    entity.AddSignalHandler<MySignal>(signal => { Console.WriteLine(signal); }); // entity: 1 - signal > MySignal    
    entity.EmitSignal(new MySignal());
}
```

### Create entity queries

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
    Console.WriteLine(queryEntityNames);    // Query: [EntityName]  EntityCount: 1

    var queryMyComponents = store.Query<MyComponent>();
    Console.WriteLine(queryMyComponents);   // Query: [MyComponent]  EntityCount: 2
    
    // --- query tags
    var queryTag  = store.Query().AllTags(Tags.Get<MyTag1>());
    Console.WriteLine(queryTag);            // Query: [#MyTag1]  EntityCount: 3
    
    var queryTags = store.Query().AllTags(Tags.Get<MyTag1, MyTag2>());
    Console.WriteLine(queryTags);           // Query: [#MyTag1, #MyTag2]  EntityCount: 1
}
```

### Enumerate the chunks of an entity query

```csharp
public static void EnumerateQueryChunks()
{
    var store   = new EntityStore();
    for (int n = 0; n < 3; n++) {
        var entity = store.CreateEntity();
        entity.AddComponent(new MyComponent{ value = n + 42 });
    }
    var queryMyComponents = store.Query<MyComponent>();
    foreach (var (components, _) in queryMyComponents.Chunks)
    {
        foreach (var component in components.Span) {
            Console.WriteLine($"MyComponent.value: {component.value}");
            // MyComponent.value: 42
            // MyComponent.value: 43
            // MyComponent.value: 44
        }
    }
}
```
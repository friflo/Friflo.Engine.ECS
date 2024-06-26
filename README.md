[![JSON Fliox](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/Json-Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md)   ![splash](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/paint-splatter.svg)

[![nuget](https://img.shields.io/nuget/v/Friflo.Engine.ECS?logo=nuget&logoColor=white)](https://www.nuget.org/packages/Friflo.Engine.ECS)
[![codecov](https://img.shields.io/codecov/c/gh/friflo/Friflo.Json.Fliox?logo=codecov&logoColor=white&label=codecov)](https://app.codecov.io/gh/friflo/Friflo.Json.Fliox/tree/main/Engine%2Fsrc%2FECS)
[![CI-Engine](https://img.shields.io/github/actions/workflow/status/friflo/Friflo.Json.Fliox/.github%2Fworkflows%2Fengine.yml?logo=github&logoColor=white&label=CI)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/engine.yml)
[![docs](https://img.shields.io/badge/docs-C%23%20API-blue.svg)](https://github.com/friflo/Friflo.Engine-docs/blob/main/README.md)
[![stars](https://img.shields.io/github/stars/friflo/Friflo.Json.Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox)


# C# ECS - Friflo.Engine.ECS

Currently fastest 🔥 ECS implementation in C# / .NET - using **Ecs.CSharp.Benchmark** as reference.  
See benchmark results - Mac Mini M2 - at the bottom of this page.  

![new](docs/images/new.svg) released **Friflo.Engine.ECS v2.0.0**.  
New features, performance improvements and bug fixes listed at 
[Release ⋅ engine-v2.0.0](https://github.com/friflo/Friflo.Json.Fliox/releases/tag/engine-v2.0.0).

![new](docs/images/new.svg) released **Friflo.Engine.ECS v3.0.0-preview.1**.  
Add support for [Relationships](#relationships) and full-text [Search](#search).

*Feature highlights*
- Simple API - no boilerplate.
- High-performance 🔥 and compact ECS with low memory footprint.
- Zero allocations for entire API after buffers grown large enough.
- Fully reactive - *optional*. Subscribe change events of all or specific entities.
- JSON Serialization - *optional*.
- SIMD Support - *optional*. Multi thread capable and remainder loop free.
- Supports .NET Standard 2.1 .NET 5 .NET 6 .NET 7 .NET 8    
  WASM / WebAssembly, Unity (Mono, AOT/IL2CPP, WebGL), Godot, MonoGame, ... and ![new](docs/images/new.svg) Native AOT
- Library uses only secure and managed code. No use of unsafe code. See [Wiki ⋅ Library](https://github.com/friflo/Friflo.Json.Fliox/wiki/Library#assembly-dll).  
  App / Game can access component chunks with native or unsafe code using `Span<>`s.

Complete feature list at [Wiki ⋅ Features](https://github.com/friflo/Friflo.Json.Fliox/wiki/Features).

Get package on [nuget](https://www.nuget.org/packages/Friflo.Engine.ECS/) or use the dotnet CLI.
```
dotnet add package Friflo.Engine.ECS
```

# Contents

* [ECS definition](#ecs-definition)
* [Demos](#demos)
* [Examples](#examples)
  - [Hello World](#hello-world)
  - [Relationships](#relationships)
  - [Search](#search)
  - [Systems](#systems)
* [Wiki](#wiki)
* [Benchmarks](#ecs-benchmarks)


## ECS definition

An entity-component-system (**ECS**) is a software architecture pattern. See [ECS ⋅ Wikipedia](https://en.wikipedia.org/wiki/Entity_component_system).  
It is often used in the Gaming industry - e.g. Minecraft - and used for high performant data processing.  
An ECS provide two strengths:

1. It enables writing *highly decoupled code*. Data is stored in **Components** which are assigned to objects - aka **Entities** - at runtime.  
   Code decoupling is accomplished by dividing implementation in pure data structures (**Component types**) - and code (**Systems**) to process them.  
  
2. It enables *high performant system execution* by storing components in continuous memory to leverage CPU caches L1, L2 & L3.  
   It improves CPU branch prediction by minimizing conditional branches when processing components in tight loops.

<br/>


# Demos

MonoGame Demo is available as WASM / WebAssembly app. [**Try Demo in your browser**](https://sdl-wasm-sample-web.vercel.app/docs/MonoGame/).  

<table>
  <tr>
    <td><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/MonoGame"><img src="https://raw.githubusercontent.com/friflo/Friflo.Engine-docs/main/docs/images/MonoGame-wasm.png" width="320" height="197"/></a></td>
    <td><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Unity"   ><img src="https://raw.githubusercontent.com/friflo/Friflo.Engine-docs/main/docs/images/Unity.png"         width="320" height="197"/></a></td>
    <td><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Godot"   ><img src="https://raw.githubusercontent.com/friflo/Friflo.Engine-docs/main/docs/images/Godot.png"         width="320" height="197"/></a></td>
  </tr>
  <tr>
    <td align="center"><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/MonoGame" >MonoGame Project</a></td>
    <td align="center"><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Unity"    >Unity Project</a></td>
    <td align="center"><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Godot"    >Godot Project</a></td>
  </tr>
<table>


*Desktop Demo performance:* Godot 202 FPS, Unity 100 FPS at 65536 entities.

All example Demos - **Windows**, **macOS** & **Linux** - available as projects for **MonoGame**, **Unity** and **Godot**.  
See [Demos · GitHub](https://github.com/friflo/Friflo.Engine.ECS-Demos)

<br/>

# Examples

This section contains two typical use cases when using an ECS.  
More examples are in the GitHub Wiki.

[**Examples - General**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General)  
Explain fundamental ECS types like *Entity*, *Component*, *Tag*, *Command Buffer*, ... and how to use them.

[**Examples - Optimization**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization)  
Provide techniques how to improve ECS performance.


## **Hello World**

The hello world examples demonstrates the creation of a world, some entities with components  
and their movement using a simple `ForEachEntity()` call.  

```csharp
public struct Velocity : IComponent { public Vector3 value; }

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
```
In case of moving (updating) thousands or millions of entities an optimized approach can be used.  
See:
[Enumerate Query Chunks](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization#enumerate-query-chunks),
[Parallel Query Job](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization#parallel-query-job) and
[Query Vectorization - SIMD](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization#query-vectorization---simd).  
All query optimizations are using the same `query` but with different enumeration techniques.

<br/>


## **Relationships**

New in **v3.0.0-preview.1**

Link relationships enable creating a *reference* from one entity to another.  
This is accomplished by adding a link component to an entity referencing another entity as shown below.  
In SQL terms: A link component contains a *secondary key* field referencing an entity by id - the *primary key*.

This implementation uses a different approach than **flecs** or other C# implementations similar to **flecs**.  
It uses the same data structures and algorithms used for indexed components. See [Search](#search).  
The main differences compared with **flecs** are:

- The API to create and query relations in **flecs** is very compact but not intuitive - imho.  
  It is completely different from common component handling.
  See [flecs ⋅ Relationships](https://github.com/SanderMertens/flecs/blob/master/docs/Relationships.md)

- Adding, removing or updating a link does not cause [archetype fragmentation](https://www.flecs.dev/flecs/md_docs_2Relationships.html#fragmentation).  
  In **flecs** every relationship between two entities creates an individual archetype only containing a single entity / component.  
  So each relationship allocates ~ 1000 bytes required by the archetype stored in the heap. Only for a simple link.  
  The more significant performance penalty is the side effect in queries. Many archetypes need to be iterated if they are query matches.

- Changing an entity link does not cause a structural change. In **flecs** an new archetype needs to be created.

The example shows how to create a follow component using another entity as target.

```cs
public struct FollowComponent : ILinkComponent
{
    public  Entity  target;
    public  Entity  GetIndexedValue() => target;
}

public static void Relationships()
{
    var store     = new EntityStore();
    var targets   = new List<Entity>();
    for (int n = 0; n < 1000; n++) {
        var target   = store.CreateEntity();
        targets.Add(target);
        var follower = store.CreateEntity();
        follower.AddComponent(new FollowComponent { target = target });
    }
    // get all entities where FollowComponent.target == targets[0]. O(1)
    var followers = targets[0].GetEntityReferences<FollowComponent>();
    Console.WriteLine($"followers: {followers.Count}");                     // > followers: 1
    
    // return same result as followers using a Query(). O(1)
    var query = store.Query().HasValue<FollowComponent, Entity>(targets[0]);
    Console.WriteLine($"query: {query.Count}");                             // > query: 1
    
    // get all linked entities. O(1)
    var allTargets = store.GetAllLinkedEntities<FollowComponent>();
    Console.WriteLine($"all targets: {allTargets.Count}");                  // > all targets: 1000
}
```

This features is work in progress. todo:

- [ ] Update index by all methods adding, removing or updating a link component
- [ ] Remove link component from index if entity is deleted
- [ ] Remove link component from entity if linked entity is deleted

Big shout out to [**fenn**ecs](https://github.com/outfox/fennecs) and [**flecs**](https://github.com/SanderMertens/flecs)
for the challenge to improve the feature set and performance of this project!

<br/>


## **Search**

New in **v3.0.0-preview.1**

**Friflo.Engine.ECS** enables efficient search of indexed component values.  
This enables **full text search** by using `string` as the indexed component type like in the example below.  
Any type can be used as indexed component type. E.g. int, long, float, Guid, DateTime, enum, ... .  
A search / query for a specific value executes in O(1).

Indexed components provide the same functionality and behavior as normal components implementing `IComponent`.  
Indexing is implement using an [inverted index ⋅ Wikipedia](https://en.wikipedia.org/wiki/Inverted_index).  
Adding, removing or updating an indexed component updates the index.  
These operations are executed in O(1) but significant slower than the non indexed counterparts ~10x.

Indexed components can also be used for **range queries** in case the indexed component type implements `IComparable<>`.

```cs
public struct Player : IIndexedComponent<string>
{
    public  string  name;
    public  string  GetIndexedValue() => name;
}

public static void IndexedComponents()
{
    var store   = new EntityStore();
    for (int n = 0; n < 1000; n++) {
        var entity = store.CreateEntity();
        entity.AddComponent(new Player { name = $"Player-{n,0:000}"});
    }
    // get all entities where Player.name == "Player-001". O(1)
    var lookup = store.GetEntitiesWithComponentValue<Player,string>("Player-001");
    Console.WriteLine($"lookup: {lookup.Count}");                           // > lookup: 1
    
    // return same result as lookup using a Query(). O(1)
    var query      = store.Query().HasValue    <Player,string>("Player-001");
    Console.WriteLine($"query: {query.Count}");                             // > query: 1
    
    // return all entities with a Player.name in the given range. O(N ⋅ log N) - N: all unique player names
    var rangeQuery = store.Query().ValueInRange<Player,string>("Player-000", "Player-099");
    Console.WriteLine($"range query: {rangeQuery.Count}");                  // > range query: 100
    
    // get all unique Player.name's. O(1)
    var allNames = store.GetAllIndexedComponentValues<Player,string>();
    Console.WriteLine($"all names: {allNames.Count}");                      // > all names: 1000
}
```
This features is work in progress. todo:

- [ ] Update index by all methods adding, removing or updating an indexed component
- [ ] Remove indexed component from index if entity is deleted

<br/>


## **Systems**

Systems are new in **Friflo.Engine.ECS v2.0.0**

Systems in ECS are typically queries.  
So you can still use the `world.Query<Position, Velocity>()` shown in the "Hello World" example.  

Using Systems is optional but they have some significant advantages:

- It enables chaining multiple decoupled [QuerySystem](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/QuerySystem.md) classes.

- A system can have state - fields or properties - which can be used as parameters in `OnUpdate()`.  
  The system state can be serialized to JSON.

- Systems are added to a [SystemGroup](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/SystemGroup.md).  
  Each group provide a [CommandBuffer](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization#commandbuffer).

- Systems can be enabled/disabled or removed.  
  The order of systems in a group can be changed.

- Systems have performance monitoring build-in to measure execution times and memory allocations.  
  If enabled systems detected as bottleneck can be optimized.  
  A perf log (see example below) provide a clear overview of all systems their amount of entities and impact on performance.

- Multiple worlds can be added to a single  [SystemRoot](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/SystemRoot.md) instance.  
  `root.Update()` will execute every system on all worlds.


```csharp
public static void HelloSystem()
{
    var world = new EntityStore();
    for (int n = 0; n < 10; n++) {
        world.CreateEntity(new Position(n, 0, 0), new Velocity(), new Scale3());
    }
    var root = new SystemRoot(world) {
        new MoveSystem(),
    //  new PulseSystem(),
    //  new ... multiple systems can be added. The execution order still remains clear.
    };
    root.Update(default);
}
        
class MoveSystem : QuerySystem<Position, Velocity>
{
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Position position, ref Velocity velocity, Entity entity) => {
            position.value += velocity.value;
        });
    }
}
```

A valuable strength of an ECS is establishing a clear and decoupled code structure.  
Adding the `PulseSystem` below to the `SystemRoot` above is trivial.

```csharp
class PulseSystem : QuerySystem<Scale3>
{
    float frequency = 4f;
    
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Scale3 scale, Entity entity) => {
            scale.value = Vector3.One * (1 + 0.2f * MathF.Sin(frequency * Tick.time));
        });
    }
}
```

### System monitoring

System performance monitoring is disabled by default.  
To enable monitoring call:

```csharp
root.SetMonitorPerf(true);
```

The performance statistics available at [SystemPerf](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/SystemPerf.md).  
To get performance statistics on console use:

```csharp
root.Update(default);
Console.WriteLine(root.GetPerfLog());
```

The log result will look like:
```js
stores: 1                     on      last ms       sum ms      updates     last mem      sum mem     entities
---------------------         --     --------     --------     --------     --------     --------     --------
Systems [2]                    +        0.076        3.322           10          128         1392
| ScaleSystem                  +        0.038        2.088           10           64          696        10000
| PositionSystem               +        0.038        1.222           10           64          696        10000
```
```
on                  + enabled  - disabled
last ms, sum ms     last/sum system execution time in ms
updates             number of executions
last mem, sum mem   last/sum allocated bytes
entities            number of entities matching a QuerySystem
```

<br/>


# Wiki

The **GitHub Wiki** provide you detailed information about the ECS and illustrate them by examples.

- [**Examples - General**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General)  
  Explain fundamental ECS types like *Entity*, *Component*, *Tag*, *Command Buffer*, ... and show you how to use them.  
  Contains an example for [Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot) integration.

- [**Examples - Optimization**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization)  
  Provide you techniques how to improve ECS performance.

- [**Extensions**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Extensions)  
  Projects extending Friflo.Engine.ECS with additional features.
  
- [**Features**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Features)  
  Integration possibilities, a complete feature list and performance characteristics 🔥.

- [**Library**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Library)  
  List supported platforms, properties of the assembly dll and build statistics.

- [**Release Notes**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Release-Notes)  
  List of changes of every release available on nuget.

<br/>



# ECS Benchmarks

Two benchmarks - subset of [GitHub ⋅ Ecs.CSharp.Benchmark + PR #38](https://github.com/Doraku/Ecs.CSharp.Benchmark/pull/38)
running on a Mac Mini M2.

![new](docs/images/new.svg) **2024-05-29** - Updated benchmarks.  
Improved create entities performance by **3x** to **4x** and minimized entity memory footprint from **48** to **16** bytes.  
Published in nuget package **2.0.0-preview.3**.

Made a subset as the other benchmarks are similar only with different parameters.

1. Create 100.000 entities with three components
2. Update 100.000 entities with two components


## 1. Create 100.000 entities with three components

| Method           |  | Mean         | Gen0      | Gen1      | Gen2      | Allocated   |
|----------------- |--|-------------:|----------:|----------:|----------:|------------:|
| Arch             |  |   6,980.1 μs |         - |         - |         - |  3948.51 KB |
| SveltoECS        |  |  28,165.0 μs |         - |         - |         - |     4.97 KB |
| DefaultEcs       |  |  12,680.4 μs |         - |         - |         - | 19517.01 KB |
| Fennecs          |  |  24,922.4 μs |         - |         - |         - | 16713.45 KB |
| FlecsNet         |  |  12,114.1 μs |         - |         - |         - |     3.81 KB |
| FrifloEngineEcs  |🔥|     405.3 μs |         - |         - |         - |  3625.46 KB |
| HypEcs           |  |  22,376.5 μs | 6000.0000 |         - |         - | 68748.73 KB |
| LeopotamEcsLite  |  |   5,199.9 μs |         - |         - |         - | 11248.47 KB |
| LeopotamEcs      |  |   8,758.8 μs | 1000.0000 |         - |         - | 15736.73 KB |
| MonoGameExtended |  |  30,789.0 μs | 1000.0000 |         - |         - | 30154.38 KB |
| Morpeh_Direct    |  | 126,841.8 μs | 9000.0000 | 5000.0000 | 2000.0000 | 83805.52 KB |
| Morpeh_Stash     |  |  67,127.7 μs | 4000.0000 | 2000.0000 | 1000.0000 | 44720.38 KB |
| Myriad           |  |  15,824.5 μs |         - |         - |         - |  7705.36 KB |
| RelEcs           |  |  58,002.5 μs | 6000.0000 | 2000.0000 | 1000.0000 | 75702.71 KB |
| TinyEcs          |  |  20,190.4 μs | 2000.0000 | 1000.0000 | 1000.0000 |  21317.2 KB |

🔥 *library of this project*

## 2. Update 100.000 entities with two components

Benchmark parameter: Padding = 0

*Notable fact*  
SIMD MonoThread running on a **single core** beats MultiThread running on 8 cores.  
So other threads can still keep running without competing for CPU resources.  

| Method                          |  | Mean        | Gen0    | Allocated |
|-------------------------------- |--|------------:|--------:|----------:|
| Arch_MonoThread                 |  |    62.09 μs |       - |         - |
| Arch_MonoThread_SourceGenerated |  |    52.43 μs |       - |         - |
| Arch_MultiThread                |  |    49.57 μs |       - |         - |
| DefaultEcs_MonoThread           |  |   126.33 μs |       - |         - |
| DefaultEcs_MultiThread          |  |   128.18 μs |       - |         - |
| Fennecs_ForEach                 |  |    56.30 μs |       - |         - |
| Fennecs_Job                     |  |    69.65 μs |       - |         - |
| Fennecs_Raw                     |  |    52.34 μs |       - |         - |
| FlecsNet_Each                   |  |   103.26 μs |       - |         - |
| FlecsNet_Iter                   |  |    64.23 μs |       - |         - |
| FrifloEngineEcs_MonoThread      |🔥|    57.62 μs |       - |         - |
| FrifloEngineEcs_MultiThread     |🔥|    17.17 μs |       - |         - |
| FrifloEngineEcs_SIMD_MonoThread |🔥|    11.00 μs |       - |         - |
| HypEcs_MonoThread               |  |    57.57 μs |       - |     112 B |
| HypEcs_MultiThread              |  |    61.94 μs |  0.2441 |    2079 B |
| LeopotamEcsLite                 |  |   150.11 μs |       - |         - |
| LeopotamEcs                     |  |   134.98 μs |       - |         - |
| MonoGameExtended                |  |   467.59 μs |       - |     161 B |
| Morpeh_Direct                   |  | 1,590.35 μs |       - |       3 B |
| Morpeh_Stash                    |  | 1,023.88 μs |       - |       3 B |
| Myriad_SingleThread             |  |    46.20 μs |       - |         - |
| Myriad_MultiThread              |  |   366.27 μs | 28.8086 |  239938 B |
| Myriad_SingleThreadChunk        |  |    61.32 μs |       - |         - |
| Myriad_MultiThreadChunk         |  |    25.31 μs |  0.3662 |    3085 B |
| Myriad_Enumerable               |  |   238.59 μs |       - |         - |
| Myriad_Delegate                 |  |    73.47 μs |       - |         - |
| Myriad_SingleThreadChunk_SIMD   |  |    22.33 μs |       - |         - |
| RelEcs                          |  |   251.30 μs |       - |     169 B |
| SveltoECS                       |  |   162.92 μs |       - |         - |
| TinyEcs_Each                    |  |    37.09 μs |       - |         - |
| TinyEcs_EachJob                 |  |    23.52 μs |  0.1831 |    1552 B |


🔥 *library of this project*

<br/>


**License**

This project is licensed under LGPLv3.  

Friflo.Engine.ECS  
Copyright © 2024   Ullrich Praetz - https://github.com/friflo
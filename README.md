[![JSON Fliox](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/Json-Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md)   ![splash](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/paint-splatter.svg)

[![nuget](https://img.shields.io/nuget/v/Friflo.Engine.ECS?logo=nuget&logoColor=white)](https://www.nuget.org/packages/Friflo.Engine.ECS)
[![codecov](https://img.shields.io/codecov/c/gh/friflo/Friflo.Json.Fliox?logo=codecov&logoColor=white&label=codecov)](https://app.codecov.io/gh/friflo/Friflo.Json.Fliox/tree/main/Engine%2Fsrc%2FECS)
[![CI-Engine](https://img.shields.io/github/actions/workflow/status/friflo/Friflo.Json.Fliox/.github%2Fworkflows%2Fengine.yml?logo=github&logoColor=white&label=CI)](https://github.com/friflo/Friflo.Json.Fliox/actions/workflows/engine.yml)
[![docs](https://img.shields.io/badge/docs-C%23%20API-blue.svg)](https://github.com/friflo/Friflo.Engine-docs/blob/main/README.md)
[![stars](https://img.shields.io/github/stars/friflo/Friflo.Json.Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox)


# 🔥 Friflo.Engine.ECS

***The ECS for finishers 🏁***  
Leading performance in most ECS aspects.  
See [C# ECS Benchmark Overview](https://docs.google.com/spreadsheets/d/1170ZjOXhiQJpY-VuNxocxaxPKGTJYQL72Zcbrvq0CcY).

|                           | friflo ECS | Flecs.NET  | TinyEcs    | Arch       | fennecs    | Leopotam   | DefaultEcs | Morpeh     |
| ------------------------- | ----------:| ----------:| ----------:| ----------:| ----------:| ----------:| ----------:| ----------:|
| Average Performance Ratio |       1.00 |       2.55 |       3.42 |       6.96 |      19.02 |       2.57 |       3.81 |      21.09 |
| Notes                     |            |            |            |            |            | [^sparse]  | [^sparse]  | [^sparse]  |

[^sparse]: Sparse Set based ECS projects.

## News

- [x] Released v3.0.0-preview.5  
  New: **Entity Relationships** 1:1 and 1:many, **Relations** and full-text **Search** executing in O(1). See [Component Types](#-component-types)

- [x] New GitHub benchmark repository [ECS.CSharp.Benchmark - Common use-cases](https://github.com/friflo/ECS.CSharp.Benchmark-common-use-cases)  

## Contents

* [🔥 Friflo.Engine.ECS](#-frifloengineecs)
  - [Feature highlights](#feature-highlights)
  - [Projects using friflo ECS](#projects-using-friflo-ecs)
  - [Demos](#demos)
  - [ECS definition](#ecs-definition)
* [⏩ Examples](#-examples)
  - [🚀 Hello World](#-hello-world)
  - [⌘ Component Types](#-component-types)
  - [⚙️ Systems](#️-systems)
* [📖 Wiki](#-wiki)
* [🏁 Benchmarks](#-ecs-benchmarks)

## Feature highlights

- [x] Simple API - no boilerplate, rock-solid 🗿 and bulletproof 🛡️
- [x] High-performance 🔥 compact ECS
- [x] Low memory footprint 👣. Create 100.000.000 entities in 1.5 sec
- [x] Zero ⦰ allocations after buffers are large enough. No struct boxing
- [x] High performant / type-safe queries ∈
- [x] Efficient multithreaded queries ⇶
- [x] Fully reactive / entity events ⚡
- [x] Entity component Search in O(1) 🔎 
- [x] Fast batch / bulk operations ⫴
- [x] Command buffers ⋙
- [x] Entity relationships and relations ⌘
- [x] Entity hierarchy / tree 🞱
- [x] Systems / System groups ⚙️
- [x] Watch entities, components, tags, query results and systems in debugger 🐞
- [x] JSON Serialization 💿
- [x] SIMD Support 🧮
- [x] Supports .NET Standard 2.1 .NET 5 .NET 6 .NET 7 .NET 8    
  WASM / WebAssembly, Unity (Mono, AOT/IL2CPP, WebGL), Godot, MonoGame, ... and Native AOT
- [x] **100% secure C#** 🔒. No *unsafe code*, *native dll bindings* and *access violations*. 
  See [Wiki ⋅ Library](https://github.com/friflo/Friflo.Json.Fliox/wiki/Library#assembly-dll).  


Complete feature list at [Wiki ⋅ Features](https://github.com/friflo/Friflo.Json.Fliox/wiki/Features).


Get package on [nuget](https://www.nuget.org/packages/Friflo.Engine.ECS/) or use the dotnet CLI.
```
dotnet add package Friflo.Engine.ECS
```




## Projects using friflo ECS

### [Horse Runner DX](https://store.steampowered.com/app/2955320/Horse_Runner_DX)

<a href="https://store.steampowered.com/app/2955320/Horse_Runner_DX"><img src="https://raw.githubusercontent.com/friflo/Friflo.Engine-docs/main/docs/images/horse-runner-dx.png" width="246" height="124"/></a>  
Quote from developer: *"Just wanted to let you know that Friflo ECS 2.0.0 works like a charm in my little game.  
I use it for basically everything (landscape segments, vegetation, players, animations,  collisions and even the floating dust particles are entities).  
After some optimization there is no object allocation during gameplay - the allocation graph just stays flat - no garbage collection."*

## Demos

MonoGame Demo is available as WASM / WebAssembly app. [**Try Demo in your browser**](https://sdl-wasm-sample-web.vercel.app/docs/MonoGame/).  
Demo projects on GitHub below.
<table>
 <thead>
  <tr>
    <td><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/MonoGame"><img src="https://raw.githubusercontent.com/friflo/Friflo.Engine-docs/main/docs/images/MonoGame-wasm.png" width="160px" height="100px"/></a></td>
    <td><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Unity"   ><img src="https://raw.githubusercontent.com/friflo/Friflo.Engine-docs/main/docs/images/Unity.png"         width="160px" height="100px"/></a></td>
    <td><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Godot"   ><img src="https://raw.githubusercontent.com/friflo/Friflo.Engine-docs/main/docs/images/Godot.png"         width="160px" height="100px"/></a></td>
  </tr>
 </thead>
 <tbody>
  <tr>
    <td align="center"><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/MonoGame" >MonoGame</a></td>
    <td align="center"><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Unity"    >Unity</a></td>
    <td align="center"><a href="https://github.com/friflo/Friflo.Engine.ECS-Demos/tree/main/Godot"    >Godot</a></td>
  </tr>
 </tbody>
<table>

*Desktop Demo performance:* Godot 202 FPS, Unity 100 FPS at 65536 entities.  
All example Demos - **Windows**, **macOS** & **Linux** - available as projects for **MonoGame**, **Unity** and **Godot**.  
See [Demos · GitHub](https://github.com/friflo/Friflo.Engine.ECS-Demos)


## ECS definition

An entity-component-system (**ECS**) is a software architecture pattern. See [ECS ⋅ Wikipedia](https://en.wikipedia.org/wiki/Entity_component_system).  
It is often used in the Gaming industry - e.g. Minecraft - and used for high performant data processing.  
An ECS provide two strengths:

1. It enables writing *highly decoupled code*. Data is stored in **Components** which are assigned to objects - aka **Entities** - at runtime.  
   Code decoupling is accomplished by dividing implementation in pure data structures (**Component types**) - and code (**Systems**) to process them.  
  
2. It enables *high performant system execution* by storing components in continuous memory to leverage CPU caches L1, L2 & L3.  
   It improves CPU branch prediction by minimizing conditional branches when processing components in tight loops.

<br/>

# ⏩ Examples

This section contains two typical use cases when using an ECS.  
More examples are in the GitHub Wiki.

[**Examples - General**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-General)  
Explain fundamental ECS types like *Entity*, *Component*, *Tag*, *Command Buffer*, ... and how to use them.

[**Examples - Optimization**](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization)  
Provide techniques how to improve ECS performance.


## **🚀 Hello World**

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


## **⌘ Component Types**

![new](docs/images/new.svg) in **Friflo.Engine.ECS v3.0.0-preview.2**

For specific use cases there is now a set of specialized component interfaces providing additional features.    
*Note:* Newly added features do not affect the behavior or performance of existing features.

The specialized component types enable entity relationships, relations and full-text search.  
Typical use case for entity relationships in a game are:
- Attack systems
- Path finding / Route tracing
- Model social networks. E.g friendship, alliances or rivalries
- Build any type of a [directed graph](https://en.wikipedia.org/wiki/Directed_graph)
  using entities as *nodes* and links or relations as *edges*.

Use cases for relations:
- Inventory systems
- Add multiple components of the same type to an entity

| Use case / Example                                                                                                        | Component interface type  | Description
| ------------------------------------------------------------------------------------------------------------------------- | ------------------------- | --------------------------------------------
| [Entity Relationships](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Component-Types#entity-relationships)  | **Link Component**        | A single link on an entity referencing another entity
|                                                                                                                           | **Link Relation**         | Multiple links on an entity referencing other entities
| [Relations](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Component-Types#relations)                        | **Relation Component**    | Add multiple components of same type to an entity
| [Search & Range queries](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Component-Types#search)              | **Indexed Component**     | Full text search of component fields executed in O(1).<br/>Range queries on component fields having a sort order.

Big shout out to [**fenn**ecs](https://github.com/outfox/fennecs) and [**flecs**](https://github.com/SanderMertens/flecs)
for the challenge to improve the feature set and performance of this project!

<br/>


## **⚙️ Systems**

Systems are new in **Friflo.Engine.ECS v2.0.0**

Systems in ECS are typically queries.  
So you can still use the `world.Query<Position, Velocity>()` shown in the "Hello World" example.  

Using Systems is optional but they have some significant advantages.

**System features**

- Enable chaining multiple decoupled [QuerySystem](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/QuerySystem.md) classes in a
  [SystemGroup](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/SystemGroup.md).  
  Each group provide a [CommandBuffer](https://github.com/friflo/Friflo.Json.Fliox/wiki/Examples-~-Optimization#commandbuffer).

- A system can have state - fields or properties - which can be used as parameters in `OnUpdate()`.  
  The system state can be serialized to JSON.

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
This system uses a `foreach (var entity in Query.Entities)` as an alternative to `Query.ForEachEntity((...) => {...})`  
to iterate the query result.

```csharp
class PulseSystem : QuerySystem<Scale3>
{
    float frequency = 4f;
    
    protected override void OnUpdate() {
        foreach (var entity in Query.Entities) {
            ref var scale = ref entity.GetComponent<Scale3>().value;
            scale = Vector3.One * (1 + 0.2f * MathF.Sin(frequency * Tick.time));
        }
    }
}
```

### ⏱ System monitoring

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
stores: 1                  on   last ms    sum ms   updates  last mem   sum mem  entities
---------------------      --  --------  --------  --------  --------  --------  --------
Systems [2]                 +     0.076     3.322        10       128      1392
| ScaleSystem               +     0.038     2.088        10        64       696     10000
| PositionSystem            +     0.038     1.222        10        64       696     10000
```
```
on                  + enabled  - disabled
last ms, sum ms     last/sum system execution time in ms
updates             number of executions
last mem, sum mem   last/sum allocated bytes
entities            number of entities matching a QuerySystem
```

<br/>


# 📖 Wiki

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



# 🏁 ECS Benchmarks

## ECS.CSharp.Benchmark - Common use-cases

Created a new GitHub repository [ECS.CSharp.Benchmark - Common use-cases](https://github.com/friflo/ECS.CSharp.Benchmark-common-use-cases).  
It compares the performance of multiple ECS projects with **simple** benchmarks.  
So they can be used as a **guide to migrate** form one ECS to another.  
See discussion of [reddit announcement Post](https://www.reddit.com/r/EntityComponentSystem/comments/1e0qo62/just_published_new_github_repo_ecs_c_benchmark/).


## ECS.CSharp.Benchmark

Performance comparison in popular **ECS C# benchmark** on GitHub.  
Two benchmarks - subset of [GitHub ⋅ Ecs.CSharp.Benchmark + PR #38](https://github.com/Doraku/Ecs.CSharp.Benchmark/pull/38)
running on a Mac Mini M2.

![new](docs/images/new.svg) **2024-05-29** - Updated benchmarks.  
Improved create entities performance by **3x** to **4x** and minimized entity memory footprint from **48** to **16** bytes.  
Published in nuget package **2.0.0-preview.3**.

Made a subset as the other benchmarks are similar only with different parameters.

1. Create 100.000 entities with three components
2. Update 100.000 entities with two components


## 1. Create 100.000 entities with three components

| Method           |  | Mean         | Gen0      | Allocated   |
|----------------- |--|-------------:|----------:|------------:|
| Arch             |  |   6,980.1 μs |         - |  3948.51 KB |
| SveltoECS        |  |  28,165.0 μs |         - |     4.97 KB |
| DefaultEcs       |  |  12,680.4 μs |         - | 19517.01 KB |
| Fennecs          |  |  24,922.4 μs |         - | 16713.45 KB |
| FlecsNet         |  |  12,114.1 μs |         - |     3.81 KB |
| FrifloEngineEcs  |🔥|     405.3 μs |         - |  3625.46 KB |
| HypEcs           |  |  22,376.5 μs | 6000.0000 | 68748.73 KB |
| LeopotamEcsLite  |  |   5,199.9 μs |         - | 11248.47 KB |
| LeopotamEcs      |  |   8,758.8 μs | 1000.0000 | 15736.73 KB |
| MonoGameExtended |  |  30,789.0 μs | 1000.0000 | 30154.38 KB |
| Morpeh_Direct    |  | 126,841.8 μs | 9000.0000 | 83805.52 KB |
| Morpeh_Stash     |  |  67,127.7 μs | 4000.0000 | 44720.38 KB |
| Myriad           |  |  15,824.5 μs |         - |  7705.36 KB |
| RelEcs           |  |  58,002.5 μs | 6000.0000 | 75702.71 KB |
| TinyEcs          |  |  20,190.4 μs | 2000.0000 |  21317.2 KB |

🔥 *library of this project*

## 2. Update 100.000 entities with two components

Benchmark parameter: Padding = 0

*Notable fact*  
SIMD MonoThread running on a **single core** beats MultiThread running on 8 cores.  
So other threads can still keep running without competing for CPU resources.  

| Method                          |  | Mean        | Allocated |
|-------------------------------- |--|------------:|----------:|
| Arch_MonoThread                 |  |    62.09 μs |         - |
| Arch_MonoThread_SourceGenerated |  |    52.43 μs |         - |
| Arch_MultiThread                |  |    49.57 μs |         - |
| DefaultEcs_MonoThread           |  |   126.33 μs |         - |
| DefaultEcs_MultiThread          |  |   128.18 μs |         - |
| Fennecs_ForEach                 |  |    56.30 μs |         - |
| Fennecs_Job                     |  |    69.65 μs |         - |
| Fennecs_Raw                     |  |    52.34 μs |         - |
| FlecsNet_Each                   |  |   103.26 μs |         - |
| FlecsNet_Iter                   |  |    64.23 μs |         - |
| FrifloEngineEcs_MonoThread      |🔥|    57.62 μs |         - |
| FrifloEngineEcs_MultiThread     |🔥|    17.17 μs |         - |
| FrifloEngineEcs_SIMD_MonoThread |🔥|    11.00 μs |         - |
| HypEcs_MonoThread               |  |    57.57 μs |     112 B |
| HypEcs_MultiThread              |  |    61.94 μs |    2079 B |
| LeopotamEcsLite                 |  |   150.11 μs |         - |
| LeopotamEcs                     |  |   134.98 μs |         - |
| MonoGameExtended                |  |   467.59 μs |     161 B |
| Morpeh_Direct                   |  | 1,590.35 μs |       3 B |
| Morpeh_Stash                    |  | 1,023.88 μs |       3 B |
| Myriad_SingleThread             |  |    46.20 μs |         - |
| Myriad_MultiThread              |  |   366.27 μs |  239938 B |
| Myriad_SingleThreadChunk        |  |    61.32 μs |         - |
| Myriad_MultiThreadChunk         |  |    25.31 μs |    3085 B |
| Myriad_Enumerable               |  |   238.59 μs |         - |
| Myriad_Delegate                 |  |    73.47 μs |         - |
| Myriad_SingleThreadChunk_SIMD   |  |    22.33 μs |         - |
| RelEcs                          |  |   251.30 μs |     169 B |
| SveltoECS                       |  |   162.92 μs |         - |
| TinyEcs_Each                    |  |    37.09 μs |         - |
| TinyEcs_EachJob                 |  |    23.52 μs |    1552 B |


🔥 *library of this project*

<br/>


**License**

This project is licensed under LGPLv3.  

Friflo.Engine.ECS  
Copyright © 2024   Ullrich Praetz - https://github.com/friflo
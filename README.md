<div align="center">

  [![friflo ECS](docs/images/friflo-ECS.svg)](https://github.com/friflo/Friflo.Engine.ECS)   ![splash](docs/images/paint-splatter.svg)

</div>

<div align="center">

[![nuget](https://img.shields.io/nuget/v/Friflo.Engine.ECS?logo=nuget&logoColor=white)](https://www.nuget.org/packages/Friflo.Engine.ECS)
[![codecov](https://img.shields.io/codecov/c/gh/friflo/Friflo.Engine.ECS?logo=codecov&logoColor=white&label=codecov)](https://app.codecov.io/gh/friflo/Friflo.Engine.ECS/tree/main/src/ECS)
[![CI-Engine](https://img.shields.io/github/actions/workflow/status/friflo/Friflo.Engine.ECS/.github%2Fworkflows%2Fengine.yml?logo=github-actions&logoColor=white&label=CI)](https://github.com/friflo/Friflo.Engine.ECS/actions/workflows/engine.yml)
[![Demos](https://img.shields.io/badge/Demos-22aa22?logo=github&logoColor=white)](https://github.com/friflo/Friflo.Engine.ECS-Demos)
[![C# API](https://img.shields.io/badge/C%23%20API-22aaaa?logo=github&logoColor=white)](https://github.com/friflo/Friflo.Engine-docs)
[![Discord](https://img.shields.io/discord/1209072279904976896?label=Discord&logo=discord&logoColor=white&labelColor=5E6AE9&color=4149A1)](https://discord.gg/nFfrhgQkb8)
[![Docs](https://img.shields.io/badge/Docs-A200FF?logo=gitbook&logoColor=white)](https://friflo.gitbook.io/friflo.engine.ecs)

</div>

# Friflo.Engine.ECS

***The ECS for finishers 🏁***  
Leading performance in most ECS aspects.  
Performance Ratio - see [C# ECS Benchmark](https://github.com/friflo/ECS.CSharp.Benchmark-common-use-cases)

|         |     friflo | Flecs.NET  | TinyEcs    | Arch       | fennecs    | Leopotam   | DefaultEcs | Morpeh     |
| ------- | ----------:| ----------:| ----------:| ----------:| ----------:| ----------:| ----------:| ----------:|
| *Ratio* |       1.00 |       2.55 |       3.42 |       6.96 |      19.02 |       2.57 |       3.81 |      21.09 |
| *Notes* |            |            |            |            |            | [^sparse]  | [^sparse]  | [^sparse]  |

[^sparse]: Sparse Set based ECS projects.

## News

- [x] ![new](docs/images/new.svg) features in v3.0.0  
      Introduced: **Index / Search** with O(1), **Relationships** 1:1 and 1:many and **Relations**. See [New features in 3.0.0](#new-features-in-300)

- [x] Published [Friflo.Engine.ECS.Boost](https://www.nuget.org/packages/Friflo.Engine.ECS.Boost) to enable
      [boosted queries](https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#boosted-query) with maximum performance.

- [x] New GitHub benchmark repository [ECS.CSharp.Benchmark - Common use-cases](https://github.com/friflo/ECS.CSharp.Benchmark-common-use-cases)  

## Contents

* [🔥 Friflo.Engine.ECS](#frifloengineecs)
  - [Documentation](#documentation)
  - [Feature highlights](#feature-highlights)
  - [Projects using friflo ECS](#projects-using-friflo-ecs)
  - [Demos](#demos)
  - [ECS definition](#ecs-definition)
  - [New features in 3.0.0](#new-features-in-300)
* [⏩ Examples](#-examples)
  - [🚀 Hello World](#-hello-world)
  - [⚙️ Systems](#️-systems)
* [🏁 Benchmarks](#-ecs-benchmarks)


## Documentation

Get an overview of the feature set or explore various use-cases with examples at
[**friflo ECS · Docs**](https://friflo.gitbook.io/friflo.engine.ecs).


## Feature highlights

- [x] Simple API - no boilerplate, rock-solid 🗿 and bulletproof 🛡️
- [x] High-performance 🔥 compact ECS
- [x] Low memory footprint 👣. Create 100.000.000 entities in 1.5 sec
- [x] Zero ⦰ allocations after buffers are large enough. No struct boxing
- [x] High performant / type-safe queries ⊆
- [x] Efficient multithreaded queries ⇶
- [x] Entity component Search in O(1) ∈ 
- [x] Fast batch / bulk operations ⏩
- [x] Command buffers / deferred operations ⏭️
- [x] Entity relationships and relations ⌘
- [x] Entity hierarchy / tree ⪪
- [x] Fully reactive / entity events ⚡
- [x] Systems / System groups ⚙️
- [x] JSON Serialization 💿
- [x] SIMD Support 🧮
- [x] Supports .NET Standard 2.1 .NET 5 .NET 6 .NET 7 .NET 8    
  WASM / WebAssembly, Unity (Mono, AOT/IL2CPP, WebGL), Godot, MonoGame, ... and Native AOT
- [x] CLS Compliant API - supporting all .NET languages: C#, F#, VB.NET, ...
- [x] Debug Tools 🐞: Watch entities, components, tags, relations, query results, systems, ...
      <details>
        <summary>Screenshot - Watch ad-hoc query result in debugger</summary>
        <img src="docs/images/watch-query-result-set.png" width="656" height="202"/>
      </details>
- [x] **100%** verifiably safe 🔒 C#. No [*unsafe code*](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code),
      *native dll bindings* and *access violations*. 
      <details>
        <summary>Behavior of access violation bugs using unsafe code</summary>
        <img src="docs/images/access_violation/VS-2022_access_violation.png" width="249" height="135"/>
        <img src="docs/images/access_violation/Rider-access_violation.png"   width="230" height="115"/>
        <img src="docs/images/access_violation/Unity_access_violation.png"   width="213" height="131"/>
      </details>



Get package on [nuget](https://www.nuget.org/packages/Friflo.Engine.ECS/) or use the dotnet CLI.
```
dotnet add package Friflo.Engine.ECS
```




## Projects using friflo ECS

### [RaygueLike Challenge](https://github.com/d-bucur/rl2025)

<a href="https://github.com/d-bucur/rl2025"><img src="https://img.itch.zone/aW1nLzIzMDEyMjQ3LnBuZw==/original/us69f5.png" width="460" height="268"/></a>  
![raylib](https://img.shields.io/badge/raylib-F5F5F5?logo=raylib&logoColor=black)
![WASM](https://img.shields.io/badge/WASM-624DE8?logo=webassembly&logoColor=white)
[Play in Browser](https://sirdorius.itch.io/rayguelike-2025)  
Project is hosted on GitHub and supports Desktop and WASM.  
Rendering of 3D environment and 2D sprites is implemented with Raylib.  
Interesting note: It combines behavior trees with ECS.


### [Horse Runner DX](https://store.steampowered.com/app/2955320/Horse_Runner_DX)

<a href="https://store.steampowered.com/app/2955320/Horse_Runner_DX"><img src="https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/2955320/library_header.jpg" width="460" height="215"/></a>  
![MonoGame](https://img.shields.io/badge/MonoGame-E73C00?logo=monogame&logoColor=white)
![Steam](https://img.shields.io/badge/Steam-00ADEF?logo=steam&logoColor=white)  
Quote from developer: *"Just wanted to let you know that Friflo ECS 2.0.0 works like a charm in my little game.  
I use it for basically everything (landscape segments, vegetation, players, animations,  collisions and even the floating dust particles are entities).  
After some optimization there is no object allocation during gameplay - the allocation graph just stays flat - no garbage collection."*


**Want to add a project?**  
Create a GitHub [Discussion](https://github.com/friflo/Friflo.Engine.ECS/discussions/categories/show-and-tell) or message on [Discord](https://discord.gg/nFfrhgQkb8).


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


## **New features in 3.0.0**

- [Index / Search](https://friflo.gitbook.io/friflo.engine.ecs/documentation/component-index) used to search entities with specific component values in O(1). E.g
  - To lookup entities having a *TileComponent* with a specific tile id.
  - To lookup network entities with a *NetComponent* using a custom identifier like a `Guid`,`long` or a `string`.

- [Relationships](https://friflo.gitbook.io/friflo.engine.ecs/documentation/relationships) to create links / connections between entities. Used for:
  - Attack systems
  - Path finding / Route tracing
  - Model social networks. E.g friendship, alliances or rivalries
  - Build any type of a [directed graph](https://en.wikipedia.org/wiki/Directed_graph)
    using entities as *nodes* and links or relations as *edges*

- [Relations](https://friflo.gitbook.io/friflo.engine.ecs/documentation/relations) to add multiple *"components"* of the same type to a single entity. E.g.
  - Inventory systems
  - To model one-to-many data structures


Big shout out to [**fenn**ecs](https://github.com/outfox/fennecs) and [**flecs**](https://github.com/SanderMertens/flecs)
for the challenge to improve the feature set and performance of this project!

<br/>

# ⏩ Examples

This section contains two typical use cases when using an ECS.  
More examples describing use of various features are in the [friflo ECS - Documentation](https://friflo.gitbook.io/friflo.engine.ecs).


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
[Enumerate Query Chunks](https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#enumerate-query-chunks),
[Parallel Query Job](https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#parallel-query-job) and
[Query Vectorization - SIMD](https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#query-vectorization-simd).  
All query optimizations are using the same `query` but with different enumeration techniques.

<br/>





## **⚙️ Systems**

Systems are new in **Friflo.Engine.ECS v2.0.0**

Systems in ECS are typically queries.  
So you can still use the `world.Query<Position, Velocity>()` shown in the "Hello World" example.  

Using Systems is optional but they have some significant advantages.

**System features**

- Enable chaining multiple decoupled [QuerySystem](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/QuerySystem.md) classes in a
  [SystemGroup](https://github.com/friflo/Friflo.Engine-docs/blob/main/api/SystemGroup.md).  
  Each group provide a [CommandBuffer](https://friflo.gitbook.io/friflo.engine.ecs/documentation/query#commandbuffer).

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
struct Pulsating : ITag { }

class PulseSystem : QuerySystem<Scale3>
{
    float frequency = 4f;
    
    public PulseSystem() => Filter.AnyTags(Tags.Get<Pulsating>());
    
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

When enabled system monitoring captures
- Number of system executions.
- System execution duration in ms.
- Memory heap allocations per system in bytes.
- The number of entities matching a query system.


#### Realtime monitoring

In a game editor like Unity system monitoring is available in the **ECS System Set** component.
<details>
<summary>Screenshot: <b>ECS System Set</b> component in Play mode</summary>
<img src="docs/images/SystemSet-Unity.png" width="324" height="ddd"/>
</details>


#### Log monitoring

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


# 🏁 ECS Benchmarks

## ECS.CSharp.Benchmark - Common use-cases

Created a new GitHub repository [ECS.CSharp.Benchmark - Common use-cases](https://github.com/friflo/ECS.CSharp.Benchmark-common-use-cases).  
It compares the performance of multiple ECS projects with **simple** benchmarks.  
So they can be used as a **guide to migrate** form one ECS to another.  
See discussion of [reddit announcement Post](https://www.reddit.com/r/EntityComponentSystem/comments/1e0qo62/just_published_new_github_repo_ecs_c_benchmark/).


## ECS.CSharp.Benchmark

Performance comparison using popular **ECS C# benchmark** on GitHub.  
Two benchmarks - subset of [GitHub ⋅ Ecs.CSharp.Benchmark + PR #38](https://github.com/Doraku/Ecs.CSharp.Benchmark/pull/38)
running on a Mac Mini M2.

See [Benchmark results](docs/doraku-benchmark.md).

<br/>


**License**

This project is licensed under MIT.  

Friflo.Engine.ECS  
Copyright © 2025   Ullrich Praetz - https://github.com/friflo
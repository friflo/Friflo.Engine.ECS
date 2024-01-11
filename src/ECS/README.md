# [![JSON Fliox](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/Json-Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine)    **Friflo.Engine.ECS** ![splash](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/paint-splatter.svg)

## Package

This package is part of the project described below.


## Project

`Friflo.Engine.ECS` is an Entity Component System - ECS - optimized for performance and cache locality.  
Additional features:
- JSON Serialization
- Build a hierarchy of entities typically used in Games and Game Editors.
- Efficient component queries minimizing L1 cache misses with support for Vectorization (SIMD).
- Minimize times required for GC collection by using struct's for entities and components.  
  GC.Collect(1) < 0.8 ms when using 10.000.000 entities.
- Support tagging of entities and use them as a filter in queries.
- Add scripts - similar to `MonoBehavior`'s - to entities in cases OOP is preferred.
- Enable binding an entity hierarchy to a TreeView in AvaloniaUI.


## Links

- [Homepage](https://github.com/friflo/Friflo.Json.Fliox)
- [NuGet Package](https://www.nuget.org/packages/Friflo.Json.Fliox)
- [License](https://github.com/friflo/Friflo.Json.Fliox/blob/main/LICENSE)

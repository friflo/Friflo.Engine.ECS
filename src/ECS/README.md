# [![JSON Fliox](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/Json-Fliox.svg)](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md)    **Friflo.Engine.ECS** ![splash](https://raw.githubusercontent.com/friflo/Friflo.Json.Fliox/main/docs/images/paint-splatter.svg)


## Project

**Friflo.Engine.ECS** - C# ECS for high performance DoD.

Currently fastest 🔥 ECS implementation in C# / .NET - using **Ecs.CSharp.Benchmark** as reference.  
See benchmark results on GitHub.  
This ECS is an Archetype / AoS based Entity Component System. See: [ECS ⋅ Wikipedia](https://en.wikipedia.org/wiki/Entity_component_system).   

*Feature highlights*
- Simple API.
- High-performance 🔥 and compact ECS implementation - Friflo.Engine.ECS.dll size 250 KB
- Zero allocations for entire API after buffers grown large enough.
- Subscribe events of specific or all entities.
- JSON Serialization.
- SIMD Support - optional. Multi thread capable and remainder loop free.
- Supports .NET Standard 2.1 .NET 5 .NET 6 .NET 7 .NET 8  
  WASM / WebAssembly, Unity (Mono & AOT/IL2CPP), Godot and MonoGame.
- Library uses only secure and managed code. No use of unsafe code. See [Wiki ⋅ Library](https://github.com/friflo/Friflo.Json.Fliox/wiki/Library).  
  App / Game can access component chunks with native or unsafe code using `Span<>`s.

More at GitHub [README.md](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md)


## Links

- [Homepage](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md)
- [NuGet Package](https://www.nuget.org/packages/Friflo.Engine.ECS/)
- [License](https://github.com/friflo/Friflo.Json.Fliox/blob/main/LICENSE)

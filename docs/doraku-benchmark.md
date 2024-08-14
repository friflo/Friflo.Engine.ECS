
# ECS.CSharp.Benchmark

Performance comparison using popular **ECS C# benchmark** on GitHub.  
Two benchmarks - subset of [GitHub ⋅ Ecs.CSharp.Benchmark + PR #38](https://github.com/Doraku/Ecs.CSharp.Benchmark/pull/38)
running on a Mac Mini M2.

**2024-05-29** - Updated benchmarks.  
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

using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Friflo.Engine.ECS;
using Tests.ECS;
using Tests.Examples;
using Tests.Generators.Benchmark;


[BenchmarkCategory("float")]
[MemoryDiagnoser] // Tracks GC allocations
// [Config(typeof(Config))]
public partial class Bench_Float
{
    private class Config : ManualConfig
    {
        public Config() 
        {
            // Add the standard columns + our new Comment column
            AddColumn(new CommentColumn()); 
        }
    }
    
    private EntityStore store;
    private ArchetypeQuery<Position1,Velocity1> query;

    const int EntityCount = Constants.EntityCount;
    
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position1 position, ref Velocity1 velocity, float deltaTime) {
        position.value = velocity.value * deltaTime + position.value;
    }

    [GlobalSetup]
    public void Setup() {
        store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position1 { value = n }, new Velocity1 { value = 2 }, new FloatComponent { value = n });
        }
        query = store.Query<Position1, Velocity1>();
    }

    [Benchmark]
    public void Float_MultiplyAdd_Query()
    {
        MultiplyAddQuery(store, 0.1f, false);
    }

    [Benchmark]
    public void Float_MultiplyAdd_Vectorize()
    {
        MultiplyAddQuery(store, 0.1f);
    }
    
    [Benchmark]
    public void Float_MultiplyAdd_ForEachEntity()
    {
        var deltaTime = 0.1f;
        query.ForEachEntity((ref Position1 position, ref Velocity1 velocity, Entity entity) => {
            position.value = velocity.value * deltaTime + position.value;
        });
    }
    
    [Vectorize][Query]  [OmitHash]
    private static void ReciprocalSquareRoot(ref Position1 position, ref Velocity1 velocity, float factor) {
        position.value = factor / MathF.Sqrt(velocity.value);
    }
    
    [Benchmark]
    public void Float_ReciprocalSquareRoot_Vectorize()
    {
        ReciprocalSquareRootQuery(store, 0.1f);
    }
    
    [Benchmark]
    public void Float_ReciprocalSquareRoot_Query()
    {
        ReciprocalSquareRootQuery(store, 0.1f, false);
    }
    
    [Vectorize][Query]  [OmitHash]
    private static void SquareRoot(ref Position1 position, ref Velocity1 velocity) {
        position.value = MathF.Sqrt(velocity.value);
    }
    
    [Benchmark]
    public void Float_SquareRoot_Vectorize()
    {
        SquareRootQuery(store);
    }
    
    [Benchmark]
    public void Float_SquareRoot_Query()
    {
        SquareRootQuery(store, false);
    }
}

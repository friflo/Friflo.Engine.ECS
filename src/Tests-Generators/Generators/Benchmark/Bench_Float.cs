using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Friflo.Engine.ECS;
using Tests.ECS;
using Tests.Examples;
using Tests.Generators.Benchmark;


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

    const int EntityCount = 1000;
    
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
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Float_MultiplyAdd_Query()
    {
        MultiplyAddQuery(store, 0.1f, false);
    }

    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Float_MultiplyAdd_Vectorize()
    {
        MultiplyAddQuery(store, 0.1f);
    }
    
    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Float_MultiplyAdd_ForEachEntity()
    {
        var deltaTime = 0.1f;
        query.ForEachEntity((ref Position1 position, ref Velocity1 velocity, Entity entity) => {
            position.value = velocity.value * deltaTime + position.value;
        });
    }
}

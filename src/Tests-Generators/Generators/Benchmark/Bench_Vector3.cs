using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Friflo.Engine.ECS;
using Tests.ECS;
using Tests.Examples;
using Tests.Generators.Benchmark;


[MemoryDiagnoser] // Tracks GC allocations
[Config(typeof(Config))]
public partial class Bench_Vector3
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
    private ArchetypeQuery<Position,Velocity> query;

    const int EntityCount = 1000;
    
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position position, ref Velocity velocity, float deltaTime) {
        position.value = velocity.value * deltaTime + position.value;
    }

    [GlobalSetup]
    public void Setup() {
        store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position(n,n,n), new Velocity { value = new Vector3(1,2,3)}, new FloatComponent { value = n });
        }
        query = store.Query<Position, Velocity>();
    }

    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void AddMultiply_Query()
    {
        MultiplyAddQuery(store, 0.1f, false);
    }

    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void AddMultiply_Vectorize()
    {
        MultiplyAddQuery(store, 0.1f);
    }
    
    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void AddMultiply_ForEachEntity()
    {
        var deltaTime = 0.1f;
        query.ForEachEntity((ref Position position, ref Velocity velocity, Entity entity) => {
            position.value = velocity.value * deltaTime + position.value;
        });
    }
}

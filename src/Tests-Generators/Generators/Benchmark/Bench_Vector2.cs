using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
using Tests.ECS;
using Tests.Examples;
using Tests.Generators.Benchmark;


[BenchmarkCategory("Vector2")]
[MemoryDiagnoser] // Tracks GC allocations
// [Config(typeof(Config))]
public partial class Bench_Vector2
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
    private ArchetypeQuery<Position2,Velocity2> query;

    const int EntityCount = Constants.EntityCount;
    
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position2 position, ref Velocity2 velocity, float deltaTime) {
        position.value = velocity.value * deltaTime + position.value;
    }

    [GlobalSetup]
    public void Setup() {
        store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position2 { value = new Vector2(n,n)}, new Velocity2 { value = new Vector2(1,2)}, new FloatComponent { value = n });
        }
        query = store.Query<Position2, Velocity2>();
    }

    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Vector2_MultiplyAdd_Query()
    {
        MultiplyAddQuery(store, 0.1f, false);
    }

    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Vector2_MultiplyAdd_Vectorize()
    {
        MultiplyAddQuery(store, 0.1f);
    }
    
    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Vector2_MultiplyAdd_ForEachEntity()
    {
        var deltaTime = 0.1f;
        query.ForEachEntity((ref Position2 position, ref Velocity2 velocity, Entity entity) => {
            position.value = velocity.value * deltaTime + position.value;
        });
    }
    
    // ------------------------------------- Lerp -------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Vector2Lerp(ref Position2 position, ref Velocity2 velocity, float amount) {
        position.value = Vector2.Lerp(position.value, velocity.value, amount);
    }
    
    [Benchmark]
    public void Vector2_Lerp_Query()
    {
        Vector2LerpQuery(store, 0.1f, false);
    }
    
    [Benchmark]
    public void Vector2_Lerp_Vectorize()
    {
        Vector2LerpQuery(store, 0.1f);
    }
}

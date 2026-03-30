using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Friflo.Engine.ECS;
using Tests.ECS;
using Tests.Examples;
using Tests.Generators.Benchmark;

[BenchmarkCategory("Vector4")]
[MemoryDiagnoser] // Tracks GC allocations
// [Config(typeof(Config))]
public partial class Bench_Vector4
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
    private ArchetypeQuery<Position4,Velocity4> query;

    const int EntityCount = Constants.EntityCount;
    
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position4 position, ref Velocity4 velocity, float deltaTime) {
        position.value = velocity.value * deltaTime + position.value;
    }

    [GlobalSetup]
    public void Setup() {
        store = new EntityStore();
        for (int n = 0; n < EntityCount; n++) {
            store.CreateEntity(new Position4 { value = new Vector4(n,n,n,n)}, new Velocity4 { value = new Vector4(1,2,3,4)}, new FloatComponent { value = n });
        }
        query = store.Query<Position4, Velocity4>();
    }

    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Vector4_MultiplyAdd_Query()
    {
        MultiplyAddQuery(store, 0.1f, false);
    }

    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Vector4_MultiplyAdd_Vectorize()
    {
        MultiplyAddQuery(store, 0.1f);
    }
    
    [Benchmark]
    [Comment("pos.value = vel.value * dt + pos.value;")]
    public void Vector4_MultiplyAdd_ForEachEntity()
    {
        var deltaTime = 0.1f;
        query.ForEachEntity((ref Position4 position, ref Velocity4 velocity, Entity entity) => {
            position.value = velocity.value * deltaTime + position.value;
        });
    }
}

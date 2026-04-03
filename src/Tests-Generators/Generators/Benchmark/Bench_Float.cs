using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
using Tests.ECS;
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
    
    private Position1[] positions = new  Position1[EntityCount];
    private Velocity1[] velocities = new  Velocity1[EntityCount];
    private float       factor = 0.1f;

    const int EntityCount = Constants.EntityCount;
    
    // ---------------------------------------- (a * b) + c
    [Vectorize][Query]  [OmitHash]
    private static void MultiplyAdd(ref Position1 position, ref Velocity1 velocity, float deltaTime) {
        position.value = velocity.value * deltaTime + position.value;
    }

    [GlobalSetup]
    public void Setup() {
        store = new EntityStore();
        for (int n = 0; n < EntityCount; n++)
        {
            positions[n] = new Position1 { value = n };
            velocities[n] = new Velocity1 { value = 2 };
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
    
    // ---------------------------------------- 1 / MathF.Sqrt()
    [Vectorize][Query]  [OmitHash]
    private static void ReciprocalSqrt(ref Position1 position, ref Velocity1 velocity, float factor) {
        position.value = factor / MathF.Sqrt(velocity.value);
    }
    
    [Benchmark]
    public void Float_ReciprocalSqrt_Vectorize()
    {
        ReciprocalSqrtQuery(store, 0.1f);
    }
    
    [Benchmark]
    public void Float_ReciprocalSqrt_Query()
    {
        ReciprocalSqrtQuery(store, 0.1f, false);
    }
    
    [Benchmark]
    public void Float_ReciprocalSqrt_MathF() {
        for (int n = 0; n < positions.Length; n++) {
            positions[n].value = factor * MathF.ReciprocalSqrtEstimate(velocities[n].value);
        }
    }
    
    // ---------------------------------------- MathF.Sqrt()
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
    
    // ---------------------------------------- MathF.Log()
    [Vectorize][Query]  [OmitHash]
    private static void Log(ref Position1 position, ref Velocity1 velocity) {
        position.value = MathF.Log(velocity.value);
    }
    
    [Benchmark]
    public void Float_Log_Vectorize()
    {
        LogQuery(store);
    }
    
    [Benchmark]
    public void Float_Log_Query()
    {
        LogQuery(store, false);
    }
    
    // ---------------------------------------- MathF.Log2()
    [Vectorize][Query]  [OmitHash]
    private static void Log2(ref Position1 position, ref Velocity1 velocity) {
        position.value = MathF.Log2(velocity.value);
    }
    
    [Benchmark]
    public void Float_Log2_Vectorize()
    {
        Log2Query(store);
    }
    
    [Benchmark]
    public void Float_Log2_Query()
    {
        Log2Query(store, false);
    }
    
    // ---------------------------------------- MathF.Exp()
    [Vectorize][Query]  [OmitHash]
    private static void Exp(ref Position1 position, ref Velocity1 velocity) {
        position.value = MathF.Exp(velocity.value);
    }
    
    [Benchmark]
    public void Float_Exp_Vectorize()
    {
        ExpQuery(store);
    }
    
    [Benchmark]
    public void Float_Exp_Query()
    {
        ExpQuery(store, false);
    }
}

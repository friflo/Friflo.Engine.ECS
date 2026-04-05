using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
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
    private Matrix4x4 matrix;

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
        Matrix4x4 rot = Matrix4x4.CreateFromYawPitchRoll(
            10f * (MathF.PI / 180.0f), // Yaw
            20f * (MathF.PI / 180.0f), // Pitch
            30f * (MathF.PI / 180.0f)  // Roll
        );
        Matrix4x4 trans = Matrix4x4.CreateTranslation(new Vector3(1f, 2f, 3f));
        matrix = Matrix4x4.Multiply(rot, trans);
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

    [Vectorize][Query]  [OmitHash]
    private static void TransformMatrix4x4(ref Position4 position, Matrix4x4 matrix) {
        position.value = Vector4.Transform(position.value, matrix);
    }
    
    [Benchmark]
    public void Vector4_Transform_Matrix4x4_Query()
    {
        TransformMatrix4x4Query(store, matrix, false);
    }
    
    [Benchmark]
    public void Vector4_Transform_Matrix4x4_Vectorized()
    {
        TransformMatrix4x4Query(store, matrix);
    }
    
    // ------------------------------------- Lerp -------------------------------------
    [Vectorize][Query]  [OmitHash]
    private static void Vector4Lerp(ref Position4 position, ref Velocity4 velocity, float amount) {
        position.value = Vector4.Lerp(position.value, velocity.value, amount);
    }
    
    [Benchmark]
    public void Vector4_Lerp_Query()
    {
        Vector4LerpQuery(store, 0.1f, false);
    }
    
    [Benchmark]
    public void Vector3_Lerp_Vectorize()
    {
        Vector4LerpQuery(store, 0.1f);
    }
}

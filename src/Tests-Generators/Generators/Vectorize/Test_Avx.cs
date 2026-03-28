
using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Examples;


// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;

public static partial class Test_Avx
{
    [Vectorize][Query]  [OmitHash]
    public static void Multiply(ref Position position, in Velocity velocity) {
        position.value *= velocity.value;
    } 
        
    [Test]
    public static void Test_Avx_Multiply()
    {
        var store = CreateTestStore();
        MultiplyQuery(store, false);

        var storeVectorized = CreateTestStore();
        MultiplyQuery(storeVectorized);

        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    private static EntityStore CreateTestStore()
    {
        var store = new EntityStore();
        for (int n = 0; n < 1000; n++) {
            store.CreateEntity(new Position(n,n,n), new Velocity { value = new Vector3(1,2,3)});
        }
        return store;
    }
    
    const int RepeatCount = 1; // 1_000_000;
    
    [Test]
    public static void Test_Avx_Multiply_perf_Query_vectorized()
    {
        var store = CreateTestStore();
        for (int i = 0; i < RepeatCount; i++) {
            MultiplyQuery(store);
        }
    }
    
    [Test]
    public static void Test_Avx_Multiply_perf_Query()
    {
        var store = CreateTestStore();
        for (int i = 0; i < RepeatCount; i++) {
            MultiplyQuery(store, false);
        }
    }
    
    [Test]
    public static void Test_Avx_Multiply_perf_ForEachEntity()
    {
        var store = CreateTestStore();
        var query = store.Query<Position, Velocity>();
        for (int i = 0; i < RepeatCount; i++) {
            query.ForEachEntity(static (ref Position position, ref Velocity velocity, Entity entity) => {
                position.value *= velocity.value;
            });
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    public static void MultiplyDeltaTime(ref Position position, in Velocity velocity, float deltaTime) {
        position.value *= velocity.value * deltaTime;
    }
    
    [Test]
    public static void Test_Avx_Multiply_deltaTime()
    {
        var store = CreateTestStore();
        MultiplyDeltaTimeQuery(store, 2, false);

        var storeVectorized = CreateTestStore();
        MultiplyDeltaTimeQuery(storeVectorized, 2);

        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    public static void MultiplyVector(ref Position position, in Velocity velocity, Vector3 vector3) {
        position.value *= velocity.value * vector3;
    }
    
    [Test]
    public static void Test_Avx_MultiplyVector()
    {
        var store = CreateTestStore();
        MultiplyVectorQuery(store, new Vector3(1,2,3), false);

        var storeVectorized = CreateTestStore();
        MultiplyVectorQuery(storeVectorized, new Vector3(1,2,3));

        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
    
    // -----------------------------------------------------------------------------------------------------
    [Vectorize][Query]  [OmitHash]
    public static void MultiplyAdd(ref Position position, ref Velocity velocity, float deltaTime) {
        position.value += velocity.value * deltaTime;
    }
    
    [Test]
    public static void Test_Avx_MultiplyAdd()
    {
        var store = CreateTestStore();
        MultiplyAddQuery(store, 0.1f, false);

        var storeVectorized = CreateTestStore();
        MultiplyAddQuery(storeVectorized, 0.1f);

        foreach (var entity in store.Entities)
        {
            var entityVectorized = storeVectorized.GetEntityById(entity.Id);
            Assert.That(entity.GetComponent<Position>(), Is.EqualTo(entityVectorized.GetComponent<Position>()));
        }
    }
}

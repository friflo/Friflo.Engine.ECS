
using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Examples;


// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;

public static partial class Test_Avx
{
    [Vectorize][Query]
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
    public static void Test_Avx_Multiply_vectorized_perf()
    {
        var store = CreateTestStore();
        for (int i = 0; i < RepeatCount; i++) {
            MultiplyQuery(store);
        }
    }
    
    [Test]
    public static void Test_Avx_Multiply_perf()
    {
        var store = CreateTestStore();
        for (int i = 0; i < RepeatCount; i++) {
            MultiplyQuery(store, false);
        }
    }
}


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
        var store = new EntityStore();
        for (int n = 0; n < 100; n++) {
            store.CreateEntity(new Position(n,n,n), new Velocity { value = new Vector3(1,2,3)});
        }
        MultiplyQuery(store, true);
    }
}

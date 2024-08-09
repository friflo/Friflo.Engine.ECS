using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
namespace Tests.Examples {
    
public struct Velocity : IComponent { public Vector3 value; }

// See: https://github.com/friflo/Friflo.Engine.ECS#-examples
public static class HelloWorldExample
{

[Test]
public static void HelloWorld()
{
    var world = new EntityStore();
    for (int n = 0; n < 10; n++) {
        world.CreateEntity(new Position(n, 0, 0), new Velocity{ value = new Vector3(0, n, 0)});
    }
    var query = world.Query<Position, Velocity>();
    query.ForEachEntity((ref Position position, ref Velocity velocity, Entity entity) => {
        position.value += velocity.value;
    });
}

}
}
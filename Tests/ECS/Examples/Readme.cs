using System;
using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;

#if !UNITY_5_3_OR_NEWER
using System.Runtime.Intrinsics;
#endif

// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#examples

public static class Readme
{

public struct Velocity : IComponent { public Vector3 value; } // requires >= 1.19.0

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

[Test]
public static void HelloSystem()
{
    var world = new EntityStore();
    for (int n = 0; n < 10; n++) {
        world.CreateEntity(new Position(n, 0, 0), new Velocity(), new Scale3());
    }
    var root = new SystemRoot(world) {
        new MoveSystem(),
        // Hundreds of systems can be added. The execution order still remains clear.
    };
    root.Update(default);
}
        
class MoveSystem : QuerySystem<Position, Velocity>
{
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Position position, ref Velocity velocity, Entity _) => {
            position.value += velocity.value;
        });
    }
}

class PulseSystem : QuerySystem<Scale3>
{
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Scale3 scale, Entity _) => {
            scale.value = Vector3.One * (1 + 0.2f * MathF.Sin(4 * Tick.time));
        });
    }
}

}

}
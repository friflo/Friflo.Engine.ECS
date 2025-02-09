using System;
using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;

// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
namespace Tests.Examples {

// See: https://github.com/friflo/Friflo.Engine.ECS#-examples
public static class HelloSystemExample
{

[Test]
public static void HelloSystem()
{
    var world = new EntityStore();
    for (int n = 0; n < 10; n++) {
        world.CreateEntity(new Position(n, 0, 0), new Velocity(), new Scale3());
    }
    var root = new SystemRoot(world) {
        new MoveSystem(),
    //  new PulseSystem(),
    //  new ... multiple systems can be added. The execution order still remains clear.
    };
    root.Update(default);
}

class MoveSystem : QuerySystem<Position, Velocity>
{
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Position position, ref Velocity velocity, Entity entity) => {
            position.value += velocity.value;
        });
    }
}

struct Pulsating : ITag { }

class PulseSystem : QuerySystem<Scale3>
{
    float frequency = 4f;
    
    public PulseSystem() => Filter.AnyTags(Tags.Get<Pulsating>());
    
    protected override void OnUpdate() {
        foreach (var entity in Query.Entities) {
            ref var scale = ref entity.GetComponent<Scale3>().value;
            scale = Vector3.One * (1 + 0.2f * MathF.Sin(frequency * Tick.time));
        }
    }
}

#region custom QuerySystem

[Test]
public static void CustomizeQuerySystem()
{
    var world = new EntityStore();
    world.CreateEntity(new Position(0, 0, 0));
    var root = new SystemRoot(world) {
        new CustomQuerySystem()
    };
    root.Update(default);
}

// The example shows how to create a custom QuerySystem that:
// - creates a customQuery and
// - make structural changes via the parent group CommandBuffer.
class CustomQuerySystem : QuerySystem
{
    private ArchetypeQuery<Position> customQuery;
    
    protected override void OnAddStore(EntityStore store) {
        customQuery = store.Query<Position>();
    }
    
    protected override void OnUpdate() {
        var buffer = CommandBuffer;
        // Executes the customQuery instead of the base class Query.
        customQuery.ForEachEntity((ref Position component1, Entity entity) => {
            buffer.AddComponent(entity.Id, new Velocity());
        });
    }
}
#endregion



#region custom BaseSystem

[Test]
public static void CustomizeBaseSystem()
{
    var world = new EntityStore();
    world.CreateEntity(new UniqueEntity("Camera"), new Position(0, 0, 0));
    var root = new SystemRoot(world) {
        new CameraSystem()
    };
    root.Update(default);
}

// Example of a system that does not require a Query.
// E.g. find and access a UniqueEntity as shown below. 
class CameraSystem : BaseSystem
{
    private Entity camera;
    
    protected override void OnAddStore(EntityStore store) {
        camera = store.GetUniqueEntity("Camera");
    }
    
    protected override void OnUpdateGroup() {
        ref var position = ref camera.GetComponent<Position>();
        // Update camera position based on user input
    }
}
#endregion
}

}
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

[Test]
public static void CustomSystem()
{
    var world = new EntityStore();
    var entity = world.CreateEntity(new Position(0, 0, 0));
    var root = new SystemRoot(world) {
        new CustomQuerySystem()
    };
    root.Update(default);
    
    Console.WriteLine($"entity: {entity}");  // entity: id: 1  [Position, Velocity]
}

/// The example shows how to create a custom system that<br/>
/// - creates a customQuery and <br/>
/// - make structural changes via the parent group CommandBuffer.<br/>
/// <br/>
/// The system adds a Velocity component for every entity having a Position component. 
class CustomQuerySystem : QuerySystem
{
    private ArchetypeQuery<Position> customQuery;
    
    protected override void OnAddStore(EntityStore store) {
        customQuery = store.Query<Position>();
        base.OnAddStore(store); // must be called to ensure execution of OnUpdate()
    }
    
    /// Executes the customQuery instead of the base class Query.
    protected override void OnUpdate() {
        var buffer = CommandBuffer;
        customQuery.ForEachEntity((ref Position component1, Entity entity) => {
            buffer.AddComponent(entity.Id, new Velocity());
        });
    }
}

}

}
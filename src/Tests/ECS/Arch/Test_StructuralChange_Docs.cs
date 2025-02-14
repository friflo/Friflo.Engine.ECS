using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;


// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_StructuralChange_Docs
{
// [Test]
public static void CollectionModifiedException()
{
    var list = new List<int> { 1, 2, 3 };
    foreach (var item in list) {
        // throws InvalidOperationException : Collection was modified; enumeration operation may not execute.
        list.Add(42);
    }
}


// [Test]
public static void QueryException()
{
    var store = new EntityStore();
    store.CreateEntity(new Position());

    var query = store.Query<Position>();
    query.ForEachEntity((ref Position position, Entity entity) =>
    {
        // throws StructuralChangeException: within query loop. See: https://friflo.gitbook.io/friflo.engine.ecs/documentation/query#structuralchangeexception
        entity.AddComponent(new EntityName("test"));
    });
    
    // Solution: Using a CommandBuffer 
    var buffer = store.GetCommandBuffer();
    query.ForEachEntity((ref Position position, Entity entity) => {
        buffer.AddComponent(entity.Id, new EntityName("test"));
    });
    buffer.Playback();
}

// [Test]
public static void QuerySystemException() {
    var store = new EntityStore();
    store.CreateEntity(new Position());

    var root = new SystemRoot(store) {
        new QueryPositionSystem()
    };
    root.Update(default); 
}

class QueryPositionSystem : QuerySystem<Position>
{
    protected override void OnUpdate() {
        Query.ForEachEntity((ref Position component1, Entity entity) => {
            // throws StructuralChangeException: within query loop. See: https://friflo.gitbook.io/friflo.engine.ecs/documentation/query#structuralchangeexception
            entity.AddComponent(new EntityName("test"));
        });
        
        // Solution: Using the system CommandBuffer 
        var buffer = CommandBuffer;
        Query.ForEachEntity((ref Position component1, Entity entity) => {
            buffer.AddComponent(entity.Id, new EntityName("test"));
        });
        // changes made via CommandBuffer are applied by parent group
    }
}


}
}


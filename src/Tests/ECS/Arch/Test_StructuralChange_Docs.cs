using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;


// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

[Ignore("source for docs: https://friflo.gitbook.io/friflo.engine.ecs/documentation/query#structuralchangeexception")]
public static class Test_StructuralChange_Docs
{
[Test]
public static void CollectionModifiedException()
{
    var list = new List<int> { 1, 2, 3 };
    foreach (var item in list) {
        // throws InvalidOperationException : Collection was modified; enumeration operation may not execute.
        list.Add(42);
    }
}


[Test]
public static void QueryException()
{
    var store = new EntityStore();
    store.CreateEntity(new Position());

    var query = store.Query<Position>();
    query.ForEachEntity((ref Position position, Entity entity) =>
    {
        // throws StructuralChangeException: within query loop. See: https://friflo.gitbook.io/friflo.engine.ecs/documentation/query#structuralchangeexception
        entity.Add(new EntityName("test"));
    });
    
    // Valid approach using a CommandBuffer 
    var buffer = store.GetCommandBuffer();
    query.ForEachEntity((ref Position position, Entity entity) => {
        buffer.AddComponent(entity.Id, new EntityName("test"));
    });
    buffer.Playback();
}
}

}


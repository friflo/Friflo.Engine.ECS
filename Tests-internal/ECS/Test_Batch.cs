using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Batch
{
    [Test]
    public static void Test_Batch_Entity()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        
        entity.Batch
            .Add        (new Position())
            .Remove     <Rotation>()
            .AddTag     <TestTag>()
            .RemoveTag  <TestTag>()
            .Apply();
        
        
        var addTags     = Tags.Get<TestTag>();
        var removeTags  = Tags.Get<TestTag2>();
        
        entity.Batch
            .Add        (new Position())
            .AddTags    (addTags)
            .RemoveTags (removeTags)
            .Apply();
    }
}


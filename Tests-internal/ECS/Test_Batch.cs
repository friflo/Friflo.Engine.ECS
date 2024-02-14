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
        store.OnComponentAdded      += _ => { };
        store.OnComponentRemoved    += _ => { };
        store.OnTagsChanged         += _ => { }; 
        var entity = store.CreateEntity();
        
        entity.Batch
            .AddComponent   (new Position(1,1,1))
            .RemoveComponent<Rotation>()
            .AddTag         <TestTag>()
            .RemoveTag      <TestTag2>()
            .Apply();
        
        
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        entity.Batch
            .AddComponent   (new Position(2,2,2))
            .AddTags        (addTags)
            .RemoveTags     (removeTags)
            .Apply();
    }
}


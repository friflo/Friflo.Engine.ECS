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
            .AddComponent   (new Position(1, 1, 1))
            .AddComponent   (new EntityName("test"))
            .RemoveComponent<Rotation>()
            .AddTag         <TestTag>()
            .RemoveTag      <TestTag2>()
            .Apply();
        
        Assert.AreEqual("id: 1  \"test\"  [EntityName, Position, #TestTag]", entity.ToString());
        Assert.AreEqual(new Position(1, 1, 1), entity.Position);
        
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        entity.Batch
            .AddComponent   (new Position(2, 2, 2))
            .RemoveComponent<EntityName>()
            .AddTags        (addTags)
            .RemoveTags     (removeTags)
            .Apply();
        
        Assert.AreEqual("id: 1  [Position, #TestTag2]", entity.ToString());
        Assert.AreEqual(new Position(2, 2, 2), entity.Position);
    }
}


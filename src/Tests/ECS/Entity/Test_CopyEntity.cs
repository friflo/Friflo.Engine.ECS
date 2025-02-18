using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable EqualExpressionComparison
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_CopyEntity
{
    [Test]
    public static void Test_CopyEntity_different_stores()
    {
        var store  = new EntityStore();
        var target = new EntityStore();
        
        var entity1 = store.CreateEntity(new Position(1,1,1), Tags.Get<TestTag>());
        var entity2 = store.CreateEntity(new Position(2,2,2), Tags.Get<TestTag>());
        var entity3 = store.CreateEntity(new Position(3,3,3), Tags.Get<TestTag2>());
        
        var target3 = target.CreateEntity(entity3.Id);
        var target2 = target.CreateEntity(entity2.Id);
        var target1 = target.CreateEntity(entity1.Id);
        
        EntityStore.CopyEntity(entity1, target1);
        EntityStore.CopyEntity(entity2, target2);
        EntityStore.CopyEntity(entity3, target3);
        
        AreEqual(3, target.Count);
        
        AreEqual(new Position(1,1,1), target1.GetComponent<Position>());
        AreEqual(new Position(2,2,2), target2.GetComponent<Position>());
        AreEqual(new Position(3,3,3), target3.GetComponent<Position>());
        
        IsTrue(target1.Tags.Has<TestTag>());
        IsTrue(target2.Tags.Has<TestTag>());
        IsTrue(target3.Tags.Has<TestTag2>());
    }
    
    [Test]
    public static void Test_CopyEntity_same_stores()
    {
        var store  = new EntityStore();
        
        var entity1 = store.CreateEntity(new Position(1,1,1), Tags.Get<TestTag>());
        var entity2 = store.CreateEntity(new Position(2,2,2), Tags.Get<TestTag>());
        var entity3 = store.CreateEntity(new Position(3,3,3), Tags.Get<TestTag2>());
        
        var target4 = store.CreateEntity(4);
        var target5 = store.CreateEntity(5);
        var target6 = store.CreateEntity(6);
        
        EntityStore.CopyEntity(entity1, target4);
        EntityStore.CopyEntity(entity2, target5);
        EntityStore.CopyEntity(entity3, target6);
        
        AreEqual(6, store.Count);
        
        AreEqual(new Position(1,1,1), target4.GetComponent<Position>());
        AreEqual(new Position(2,2,2), target5.GetComponent<Position>());
        AreEqual(new Position(3,3,3), target6.GetComponent<Position>());
        
        IsTrue(target4.Tags.Has<TestTag>());
        IsTrue(target5.Tags.Has<TestTag>());
        IsTrue(target6.Tags.Has<TestTag2>());
    }
    
    [Test]
    public static void Test_CopyEntity_exceptions()
    {
        var store  = new EntityStore();
        
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        EntityStore.CopyEntity(entity1, entity2);
        
        entity2.DeleteEntity();
        var e = Throws<ArgumentException>(() => {
            EntityStore.CopyEntity(entity1, entity2);
        });
        AreEqual("entity is null. id: 2 (Parameter 'target')", e!.Message);
        
        entity1.DeleteEntity();
        e = Throws<ArgumentException>(() => {
            EntityStore.CopyEntity(entity1, entity2);
        });
        AreEqual("entity is null. id: 1 (Parameter 'source')", e!.Message);
    }
}

}

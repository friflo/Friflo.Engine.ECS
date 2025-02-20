using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS.Index;
using static NUnit.Framework.Assert;

// ReSharper disable EqualExpressionComparison
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_CopyEntity
{
    [Test]
    public static void Test_CopyEntity_CloneStore_subset()
    {
        var store       = new EntityStore();
        var targetStore = new EntityStore();
        
        store.CreateEntity(new Position(1,1,1));                        // 1
        store.CreateEntity(new Position(2,2,2), Tags.Get<TestTag>());   // 2
        store.CreateEntity(new Position(3,3,3));                        // 3
        store.CreateEntity(new Position(4,4,4), Tags.Get<TestTag>());   // 4
        store.CreateEntity(new Position(5,5,5));                        // 5
        
        // Query will copy only entities [2, 4]
        var query = store.Query().AnyTags(Tags.Get<TestTag>());
        foreach (var entity in query.Entities) {
            // preserve same entity ids in target store
            if (!targetStore.TryGetEntityById(entity.Id, out Entity targetEntity)) {
                targetEntity = targetStore.CreateEntity(entity.Id);
            }
            entity.CopyEntity(targetEntity);
        }
        AreEqual(new Position(2,2,2), targetStore.GetEntityById(2).GetComponent<Position>());
        AreEqual(new Position(4,4,4), targetStore.GetEntityById(4).GetComponent<Position>());
    }
    
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
        
        entity1.CopyEntity(target1);
        entity2.CopyEntity(target2);
        entity3.CopyEntity(target3);
        
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
        
        entity1.CopyEntity(target4);
        entity3.CopyEntity(target6);
        entity2.CopyEntity(target5);
        
        AreEqual(6, store.Count);
        
        AreEqual(new Position(1,1,1), target4.GetComponent<Position>());
        AreEqual(new Position(2,2,2), target5.GetComponent<Position>());
        AreEqual(new Position(3,3,3), target6.GetComponent<Position>());
        
        IsTrue(target4.Tags.Has<TestTag>());
        IsTrue(target5.Tags.Has<TestTag>());
        IsTrue(target6.Tags.Has<TestTag2>());
    }
    
    [Test]
    public static void Test_CopyEntity_IndexedComponent()
    {
        var store  = new EntityStore();
        var index  = store.ComponentIndex<IndexedInt,int>();
        
        var entity1 = store.CreateEntity(new IndexedInt { value = 11 });
        var entity2 = store.CreateEntity(2);
        AreEqual("{ 1 }",       index[11].Debug());
       
        entity1.CopyEntity(entity2);
        AreEqual("{ 1, 2 }",    index[11].Debug());
        AreEqual(2, store.Count);
        AreEqual(new IndexedInt { value = 11 }, entity2.GetComponent<IndexedInt>());
        
        entity1.AddComponent(new IndexedInt { value = 42 });
        AreEqual("{ 2 }",       index[11].Debug());
        AreEqual("{ 1 }",       index[42].Debug());
        
        entity1.CopyEntity(entity2);
        AreEqual("{ }",         index[11].Debug());
        AreEqual("{ 1, 2 }",    index[42].Debug());
        AreEqual(new IndexedInt { value = 42 }, entity2.GetComponent<IndexedInt>());
        
        entity1.RemoveComponent<IndexedInt>();
        AreEqual("{ }",         index[11].Debug());
        AreEqual("{ 2 }",       index[42].Debug());
        
        entity1.CopyEntity(entity2);
        AreEqual("{ }",         index[11].Debug());
        AreEqual("{ }",         index[42].Debug());
    }
    
    [Test]
    public static void Test_CopyEntity_exceptions()
    {
        var store  = new EntityStore();
        
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.CopyEntity(entity2);
        
        entity2.DeleteEntity();
        var e = Throws<ArgumentException>(() => {
            entity1.CopyEntity(entity2);
        });
        AreEqual("entity is null. id: 2 (Parameter 'target')", e!.Message);
        
        entity1.DeleteEntity();
        e = Throws<ArgumentException>(() => {
            entity1.CopyEntity(entity2);
        });
        AreEqual("entity is null. id: 1 (Parameter 'source')", e!.Message);
    }
    
    [Test]
    public static void Test_CopyEntity_Perf()
    {
        int count       = 10; // Test_CopyEntity_Perf() - count: 100000 repeat: 1000 duration: 4465 ms
        int repeat      = 100;
        var store       = new EntityStore();
        var targetStore = new EntityStore();
        
        for (int i = 0; i < count; i++) {
            store.CreateEntity(new Position(), new Scale3(), new MyComponent1(), new MyComponent2(), new MyComponent3());
        }
        var start = new Stopwatch();
        start.Start();
        for (int i = 0; i < repeat; i++) {
            foreach (var entity in store.Entities) {
                // preserve same entity ids in target store
                if (!targetStore.TryGetEntityById(entity.Id, out Entity targetEntity)) {
                    targetEntity = targetStore.CreateEntity(entity.Id);
                }
                entity.CopyEntity(targetEntity);
            }
        }
        var msg = $"Test_CopyEntity_Perf() - count: {count} repeat: {repeat} duration: {start.ElapsedMilliseconds} ms";
        Console.WriteLine(msg);
        AreEqual(count, targetStore.Count);
    }
}

}

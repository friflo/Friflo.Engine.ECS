using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch;

public static class Test_Batch
{
    [Test]
    public static void Test_Batch_Entity()
    {
        var store = new EntityStore();
        var countAdd    = 0;
        var countRemove = 0;
        var countChange = 0;
#pragma warning disable CS0618 // Type or member is obsolete
        store.OnComponentAdded += change => {
            var str = change.ToString();
            switch (countAdd++) {
                case 0:
                    AreEqual(new EntityName("test"), change.DebugComponent);
                    AreEqual("entity: 1 - event > Add Component: [EntityName]", str);
                    break;
                case 1:
                    AreEqual(new Position(1,1,1),   change.DebugComponent);
                    IsNull  (                       change.DebugOldComponent);
                    AreEqual("entity: 1 - event > Add Component: [Position]", str);
                    break;
                case 2:
                    AreEqual(new Position(2,2,2),   change.DebugComponent);
                    AreEqual(new Position(1,1,1),   change.DebugOldComponent);
                    AreEqual("entity: 1 - event > Update Component: [Position]", str);
                    break;
                default:
                    throw new InvalidOperationException("unexpected");
            }
        };
        store.OnComponentRemoved += change => {
            var str = change.ToString();
            switch (countRemove++) {
                case 0:
                    IsNull  (                          change.DebugComponent);
                    AreEqual(new EntityName("test"),   change.DebugOldComponent);
                    AreEqual("entity: 1 - event > Remove Component: [EntityName]",  str);
                    break;
                default:
                    throw new InvalidOperationException("unexpected");
            }
        };
        store.OnTagsChanged += change => {
            var str = change.ToString();
            switch (countChange++) {
                case 0:
                    AreEqual("entity: 1 - event > Add Tags: [#TestTag]", str);
                    break;
                case 1:
                    AreEqual("entity: 1 - event > Add Tags: [#TestTag2] Remove Tags: [#TestTag]", str);
                    break;
                default:
                    throw new InvalidOperationException("unexpected");
            }
        };
#pragma warning restore CS0618 // Type or member is obsolete

        var entity = store.CreateEntity();
        
        var batch = entity.Batch;
        AreEqual("empty", batch.ToString());
        
        batch.AddComponent  (new Position(1, 1, 1))
            .AddComponent   (new EntityName("test"))
            .RemoveComponent<Rotation>()
            .AddTag         <TestTag>()
            .RemoveTag      <TestTag2>();
        AreEqual("add: [EntityName, Position, #TestTag]  remove: [Rotation, #TestTag2]", batch.ToString());
        AreEqual(5, batch.CommandCount);
        batch.Apply();
        
        AreEqual("id: 1  \"test\"  [EntityName, Position, #TestTag]", entity.ToString());
        AreEqual(new Position(1, 1, 1), entity.Position);
        
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        batch = entity.Batch;
        batch.AddComponent  (new Position(2, 2, 2))
            .RemoveComponent<EntityName>()
            .AddTags        (addTags)
            .RemoveTags     (removeTags);
        AreEqual("add: [Position, #TestTag2]  remove: [EntityName, #TestTag]", batch.ToString());
        AreEqual(4, batch.CommandCount);
        batch.Apply();
        
        AreEqual("id: 1  [Position, #TestTag2]", entity.ToString());
        AreEqual(new Position(2, 2, 2), entity.Position);
        
        AreEqual(3, countAdd);
        AreEqual(1, countRemove);
        AreEqual(2, countChange);
    }
    
    [Test]
    public static void Test_Batch_ApplyTo()
    {
        var store   = new EntityStore();
        var batch   = new EntityBatch();
        batch.AddComponent(new Position());
        batch.AddTag<TestTag>();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        batch.ApplyTo(entity1)
             .ApplyTo(entity2);
        
        AreEqual("id: 1  [Position, #TestTag]", entity1.ToString());
        AreEqual("id: 2  [Position, #TestTag]", entity2.ToString());
        
        var e = Throws<InvalidOperationException>(() => {
            batch.Apply();
        });
        AreEqual("Apply() can only be used on a batch using Entity.Batch - use ApplyTo()", e!.Message);
    }
    
    [Test]
    public static void Test_Batch_QueryEntities_ApplyBatch()
    {
        var store       = new EntityStore();
        for (int n = 0; n < 10; n++) {
            store.CreateEntity();
        }
        var batch = new EntityBatch();
        batch.AddComponent(new Position(2, 3, 4));
        store.Entities.ApplyBatch(batch);
        
        var arch = store.GetArchetype(ComponentTypes.Get<Position>());
        AreEqual(10, arch.EntityCount);
        
        batch.Clear();
        batch.AddTag<TestTag>();
        
        store.Query<Position>().Entities.ApplyBatch(batch);
        
        arch = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag>());
        AreEqual(10, arch.EntityCount);
    }
    
    [Test]
    public static void Test_Batch_Entity_Perf()
    {
        int count       = 10; // 10_000_000 ~ #PC: 1691 ms
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        var sw = new Stopwatch();
        sw.Start();

        for (int n = 0; n < count; n++)
        {
            entity.Batch
                .AddComponent   (new Position(1, 1, 1))
                .AddComponent   (new EntityName("test"))
                .RemoveComponent<Rotation>()
                .AddTag         <TestTag>()
                .RemoveTag      <TestTag2>()
                .Apply();
        
            entity.Batch
                .AddComponent   (new Position(2, 2, 2))
                .RemoveComponent<EntityName>()
                .AddTags        (addTags)
                .RemoveTags     (removeTags)
                .Apply();
        }
        
        Console.WriteLine($"Entity.Batch - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual("id: 1  [Position, #TestTag2]", entity.ToString());
    }
    
    [Test]
    public static void Test_QueryEntities_ApplyBatch_Perf()
    {
        int count       = 10; // 100_000 ~ #PC: 1943 ms
        int entityCount = 100;
        var store   = new EntityStore();
        for (int n = 0; n < entityCount; n++) {
            store.CreateEntity();
        }
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        var batch1 = new EntityBatch();
        var batch2 = new EntityBatch();
        
        batch1
            .AddComponent   (new Position(1, 1, 1))
            .AddComponent   (new EntityName("test"))
            .RemoveComponent<Rotation>()
            .AddTag         <TestTag>()
            .RemoveTag      <TestTag2>();
        batch2
            .AddComponent   (new Position(2, 2, 2))
            .RemoveComponent<EntityName>()
            .AddTags        (addTags)
            .RemoveTags     (removeTags);
        
        var sw = new Stopwatch();
        sw.Start();

        for (int n = 0; n < count; n++)
        {
            store.Entities.ApplyBatch(batch1);
            store.Entities.ApplyBatch(batch2);
        }
        Console.WriteLine($"ApplyBatch() - duration: {sw.ElapsedMilliseconds} ms");
        
        var arch = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag2>());
        AreEqual(entityCount, arch.EntityCount);
    }
}


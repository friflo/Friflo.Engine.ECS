using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_Batch
{
    [Test]
    public static void Test_Batch_Entity()
    {
        var store = new EntityStore(PidType.RandomPids);
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
        
        var batch = entity.Batch();
        AreEqual("empty", batch.ToString());
        AreEqual(0, store.Info.PooledEntityBatchCount);
        
        batch.Add       (new Position(1, 1, 1))
            .Add        (new EntityName("test"))
            .Remove     <Rotation>()
            .AddTag     <TestTag>()
            .RemoveTag  <TestTag2>();
        AreEqual("entity: 1 > add: [EntityName, Position, #TestTag]  remove: [Rotation, #TestTag2]", batch.ToString());
        AreEqual(5, batch.CommandCount);
        batch.Apply();
        
        AreEqual("id: 1  \"test\"  [EntityName, Position, #TestTag]", entity.ToString());
        AreEqual(new Position(1, 1, 1), entity.Position);
        AreEqual(1, store.Info.PooledEntityBatchCount);
        
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        batch = entity.Batch();
        AreEqual(0, store.Info.PooledEntityBatchCount);
        batch.Add       (new Position(2, 2, 2))
            .Remove     <EntityName>()
            .AddTags    (addTags)
            .RemoveTags (removeTags);
        AreEqual("entity: 1 > add: [Position, #TestTag2]  remove: [EntityName, #TestTag]", batch.ToString());
        AreEqual(4, batch.CommandCount);
        batch.Apply();
        
        AreEqual("id: 1  [Position, #TestTag2]", entity.ToString());
        AreEqual(new Position(2, 2, 2), entity.Position);
        AreEqual(1, store.Info.PooledEntityBatchCount);
        
        AreEqual(3, countAdd);
        AreEqual(1, countRemove);
        AreEqual(2, countChange);
    }
    
    [Test]
    public static void Test_Batch_ApplyTo()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var batch   = new EntityBatch();
        batch.Add   (new Position());
        batch.AddTag<TestTag>();
        
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        batch.ApplyTo(entity1)
             .ApplyTo(entity2);
        AreEqual(0, store.Info.PooledEntityBatchCount);
        
        AreEqual("id: 1  [Position, #TestTag]", entity1.ToString());
        AreEqual("id: 2  [Position, #TestTag]", entity2.ToString());
        
        var e = Throws<InvalidOperationException>(() => {
            batch.Apply();
        });
        AreEqual("Apply() can only be used on a batch returned by Entity.Batch() - use ApplyTo()", e!.Message);
        AreEqual(0, store.Info.PooledEntityBatchCount);
    }
    
    [Test]
    public static void Test_Batch_ApplyBatch()
    {
        var store       = new EntityStore(PidType.RandomPids);
        for (int n = 0; n < 10; n++) {
            store.CreateEntity();
        }
        var batch = new EntityBatch();
        batch.Add(new Position(2, 3, 4));
        store.Entities.ApplyBatch(batch);
        AreEqual(0, store.Info.PooledEntityBatchCount);
        
        var arch = store.GetArchetype(ComponentTypes.Get<Position>());
        AreEqual(10, arch.Count);
        
        batch.Clear();
        batch.AddTag<TestTag>();
        
        store.Query<Position>().Entities.ApplyBatch(batch);
        
        arch = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag>());
        AreEqual(10, arch.Count);
        AreEqual(0, store.Info.PooledEntityBatchCount);
    }
    
    [Test]
    public static void Test_Batch_multiple_batches() 
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        var batch1 = entity1.Batch().Add(new Position());
        AreEqual(0, store.Info.PooledEntityBatchCount);
        
        var batch2 = entity2.Batch().Add(new Rotation());
        AreEqual(0, store.Info.PooledEntityBatchCount);
        
        AreEqual("entity: 1 > add: [Position]", batch1.ToString());
        AreEqual("entity: 2 > add: [Rotation]", batch2.ToString());
        
        batch1.Apply();
        AreEqual("batch applied", batch1.ToString());
        AreEqual(1, store.Info.PooledEntityBatchCount);
        
        batch2.Apply();
        AreEqual("batch applied", batch2.ToString());
        AreEqual(2, store.Info.PooledEntityBatchCount);
    }
    
    [Test]
    public static void Test_Batch_already_applied_exception() 
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity(1);
        
        var batch = entity.Batch().Add(new Position());
        batch.Apply();
        
        var expect = "batch already applied";
        
        var e = Throws<BatchAlreadyAppliedException>(() => batch.Apply());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyAppliedException>(() => batch.Add(new Position()));
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyAppliedException>(() => batch.Remove<Position>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyAppliedException>(() => batch.AddTag<TestTag>());
        AreEqual(expect, e!.Message);

        e = Throws<BatchAlreadyAppliedException>(() => batch.RemoveTag<TestTag>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyAppliedException>(() => batch.AddTags(default));
        AreEqual(expect, e!.Message);

        e = Throws<BatchAlreadyAppliedException>(() => batch.RemoveTags(default));
        AreEqual(expect, e!.Message);
    }
    
    [Test]
    public static void Test_Batch_Apply_Perf()
    {
        int count       = 10; // 10_000_000 ~ #PC: 1617 ms
        var store       = new EntityStore(PidType.RandomPids);
        var entity      = store.CreateEntity();
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        var sw = new Stopwatch();
        sw.Start();

        for (int n = 0; n < count; n++)
        {
            entity.Batch()
                .Add        (new Position(1, 1, 1))
                .Add        (new EntityName("test"))
                .Remove     <Rotation>()
                .AddTag     <TestTag>()
                .RemoveTag  <TestTag2>()
                .Apply();
        
            entity.Batch()
                .Add        (new Position(2, 2, 2))
                .Remove     <EntityName>()
                .AddTags    (addTags)
                .RemoveTags (removeTags)
                .Apply();
        }
        
        Console.WriteLine($"Entity.Batch - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual("id: 1  [Position, #TestTag2]", entity.ToString());
        AreEqual(1, store.Info.PooledEntityBatchCount);
    }
    
    [Test]
    public static void Test_Batch_EntityBatch_Perf()
    {
        int count       = 10; // 100_000 ~ #PC: 1796 ms
        int entityCount = 100;
        var store   = new EntityStore(PidType.RandomPids);
        for (int n = 0; n < entityCount; n++) {
            store.CreateEntity();
        }
        var addTags     = Tags.Get<TestTag2>();
        var removeTags  = Tags.Get<TestTag>();
        
        var batch1 = new EntityBatch();
        var batch2 = new EntityBatch();
        
        batch1
            .Add        (new Position(1, 1, 1))
            .Add        (new EntityName("test"))
            .Remove     <Rotation>()
            .AddTag     <TestTag>()
            .RemoveTag  <TestTag2>();
        batch2
            .Add        (new Position(2, 2, 2))
            .Remove     <EntityName>()
            .AddTags    (addTags)
            .RemoveTags (removeTags);
        
        var sw = new Stopwatch();
        sw.Start();
        long start = 0;
        for (int n = 0; n < count; n++)
        {
            store.Entities.ApplyBatch(batch1);
            store.Entities.ApplyBatch(batch2);
            if (n == 0)  start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"ApplyBatch() - duration: {sw.ElapsedMilliseconds} ms");
        
        var arch = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag2>());
        AreEqual(entityCount, arch.Count);
        AreEqual(0, store.Info.PooledEntityBatchCount);
    }
    
    [Test]
    public static void Test_Batch_obsolete() {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        entity.AddComponent(new EntityName("test"));
        
#pragma warning disable CS0618 // Type or member is obsolete
        entity.Batch()
            .AddComponent(new Position(1,2,3))
            .RemoveComponent<EntityName>()
            .Apply();
        AreEqual("id: 1  [Position]", entity.ToString());
    }
}

}
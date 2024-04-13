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

public static class Test_BatchCreate
{
    [Test]
    public static void Test_BatchCreate_CreateEntity()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var addTags = Tags.Get<TestTag2>();
        
        // --- batch 1
        var batch = store.Batch();
        AreEqual("empty", batch.ToString());
        batch.Add   <Position>()
            .Add    (new Rotation(1, 2, 3, 4))
            .AddTag <TestTag>()
            .AddTags(addTags);
        
        AreEqual("add: [Position, Rotation, #TestTag, #TestTag2]", batch.ToString());
        AreEqual(2, batch.ComponentCount);
        AreEqual(2, batch.TagCount);
        
        var entity1 = batch.CreateEntity();
        AreEqual("batch returned", batch.ToString());
        AreEqual("id: 1  [Position, Rotation, #TestTag, #TestTag2]", entity1.ToString());
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        AreEqual(new Position(),            entity1.Position);
        AreEqual(new Rotation (1, 2, 3, 4), entity1.Rotation);
        
        // --- batch 2
        batch = store.Batch();
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);
        
        AreEqual("empty", batch.ToString());
        batch.Add   (new Position(1, 2, 3))
            .Add    <Rotation>()
            .AddTag <TestTag>()
            .AddTags(addTags)
            .Disable();
        
        var entity2 = batch.CreateEntity();
        AreEqual("batch returned", batch.ToString());
        AreEqual("id: 2  [Position, Rotation, #Disabled, #TestTag, #TestTag2]", entity2.ToString());
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        AreEqual(new Position(1, 2, 3),     entity2.Position);
        AreEqual(new Rotation(),            entity2.Rotation);
    }
    
    [Test]
    public static void Test_BatchCreate_Create_multiple_entities()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var batch = new CreateEntityBatch(store);
        batch.Add<Position>()
             .Add<Rotation>();
        
        batch.Get<Position>().x = 1;
        var entity1 = batch.CreateEntity();
        AreEqual("id: 1  [Position, Rotation]", entity1.ToString());
        AreEqual(new Position(1, 0, 0), entity1.Position);
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);

        batch.Get<Position>().x = 2;
        var entity2 = batch.CreateEntity(2);
        AreEqual("id: 2  [Position, Rotation]", entity2.ToString());
        AreEqual(new Position(2, 0, 0), entity2.Position);
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);
        
        var e = Throws<InvalidOperationException>(() => {
            batch.Get<MyComponent1>();
        });
        AreEqual("Get<>() requires a preceding Add<>(). Component: [MyComponent1]", e!.Message);
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);
    }
    
    [Test]
    public static void Test_BatchCreate_return_batch()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var batch = store.Batch(false).Add<Position>().Add<Rotation>();
        var entity1 = batch.CreateEntity();
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);
        
        var entity2 = batch.CreateEntity();
        AreEqual("id: 1  [Position, Rotation]", entity1.ToString());
        AreEqual("id: 2  [Position, Rotation]", entity2.ToString());
        AreEqual("add: [Position, Rotation]",   batch.ToString());
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);

        batch.Return();
        AreEqual("batch returned", batch.ToString());
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        batch.Return();
        AreEqual("batch returned", batch.ToString());
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        var expect = "batch already returned";
        
        var e = Throws<BatchAlreadyReturnedException> (() => batch.CreateEntity());
        AreEqual(expect, e!.Message);
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add(new Position()));
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add<Position>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTag<TestTag>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTags(default));
        AreEqual(expect, e!.Message);
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
    }
    
    [Test]
    public static void Test_BatchCreate_CreateEntity_exception()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var batch = store.Batch();
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);
        var entity = batch
            .Add<Position>()
            .Add<Rotation>()
            .CreateEntity();
        AreEqual("id: 1  [Position, Rotation]", entity.ToString());
        AreEqual("batch returned",              batch.ToString());
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        var expect = "batch already returned";
        
        var e = Throws<BatchAlreadyReturnedException> (() => batch.CreateEntity());
        AreEqual(expect, e!.Message);
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.CreateEntity(42));
        AreEqual(expect, e!.Message);
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add(new Position()));
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add<Position>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTag<TestTag>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTags(default));
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Get<Position>());
        AreEqual(expect, e!.Message);
        
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
    }
    
    [Test]
    public static void Test_BatchCreate_autoReturn_true_Perf()
    {
        int count = 10;  // 10_000_000 ~ #PC: 1141 ms
        var store = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        
        var sw = new Stopwatch();
        sw.Start();
        long start = 0;
        for (int n = 0; n < count; n++)
        {
            store.Batch()
                .Add    <Position>()
                .Add    <Rotation>()
                .AddTag <TestTag>()
                .AddTag <TestTag2>()
                .CreateEntity();
            if (n == 0)  start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
        Console.WriteLine($"CreateBatch - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(count, store.Count);
    }
    
    [Test]
    public static void Test_BatchCreate_autoReturn_false_Perf()
    {
        int count = 10;  // 10 ~ #PC: 1216 ms
        var store = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        
        var sw = new Stopwatch();
        sw.Start();
        long start = 0;
        var batch = store.Batch(false);
        for (int n = 0; n < count; n++)
        {
            batch
                .Clear()
                .Add    <Position>()
                .Add    <Rotation>()
                .AddTag <TestTag>()
                .AddTag <TestTag2>()
                .CreateEntity();
            if (n == 0)  start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);
        batch.Return();
        Console.WriteLine($"CreateBatch - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(count, store.Count);
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
    }
    
    [Test]
    public static void Test_BatchCreate_autoReturn_false_Perf2()
    {
        int count = 10;  // 10_000_000 ~ #PC: 730 ms
        var store = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        
        var sw = new Stopwatch();
        sw.Start();
        var batch = store.Batch(false)
            .Add    <Position>()
            .Add    <Rotation>()
            .AddTag <TestTag>()
            .AddTag <TestTag2>();
        long start = 0;
        
        for (int n = 0; n < count; n++)
        {
            batch.CreateEntity();
            if (n == 0)  start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        AreEqual(0, store.Info.PooledCreateEntityBatchCount);
        batch.Return();
        Console.WriteLine($"CreateBatch - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(count, store.Count);
        AreEqual(1, store.Info.PooledCreateEntityBatchCount);
    }
}


}
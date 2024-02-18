using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch;

public static class Test_BatchCreate
{
    [Test]
    public static void Test_BatchCreate_CreateEntity()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        
        var addTags     = Tags.Get<TestTag2>();
        
        var batch = store.Batch();
        AreEqual("empty", batch.ToString());
        batch.Add   <Position>()
            .Add    (new Rotation(1, 2, 3, 4))
            .AddTag <TestTag>()
            .AddTags(addTags);
        
        AreEqual("add: [Position, Rotation, #TestTag, #TestTag2]", batch.ToString());
        AreEqual(2, batch.ComponentCount);
        AreEqual(2, batch.TagCount);
        
        var entity = batch.CreateEntity();
        AreEqual("id: 1  [Position, Rotation, #TestTag, #TestTag2]", entity.ToString());
        AreEqual(1, store.PendingCreateEntityBatchCount);
        
        AreEqual(new Position(),            entity.Position);
        AreEqual(new Rotation (1,2,3,4),    entity.Rotation);
        
        batch.Clear();
        AreEqual(0, batch.ComponentCount);
        AreEqual(0, batch.TagCount);
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
        AreEqual(new Position(1, 0, 0), entity1.Position);
        AreEqual(0, store.PendingCreateEntityBatchCount);

        batch.Get<Position>().x = 2;
        var entity2 = batch.CreateEntity();
        AreEqual(new Position(2, 0, 0), entity2.Position);
        AreEqual(0, store.PendingCreateEntityBatchCount);
        
        var e = Throws<InvalidOperationException>(() => {
            batch.Get<MyComponent1>();
        });
        AreEqual("Get<>() requires a preceding Add<>(). Component: [MyComponent1]", e!.Message);
    }
    
    [Test]
    public static void Test_BatchCreate_return_batch()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var batch = store.Batch(false).Add<Position>().Add<Rotation>();
        var entity1 = batch.CreateEntity();
        AreEqual(0, store.PendingCreateEntityBatchCount);
        
        var entity2 = batch.CreateEntity();
        AreEqual("id: 1  [Position, Rotation]", entity1.ToString());
        AreEqual("id: 2  [Position, Rotation]", entity2.ToString());
        AreEqual("add: [Position, Rotation]",   batch.ToString());
        AreEqual(0, store.PendingCreateEntityBatchCount);

        batch.Return();
        AreEqual("batch returned", batch.ToString());
        AreEqual(1, store.PendingCreateEntityBatchCount);
        
        batch.Return();
        AreEqual("batch returned", batch.ToString());
        AreEqual(1, store.PendingCreateEntityBatchCount);
        
        var expect = "batch already returned";
        
        var e = Throws<BatchAlreadyReturnedException> (() => batch.CreateEntity());
        AreEqual(expect, e!.Message);
        AreEqual(1, store.PendingCreateEntityBatchCount);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add(new Position()));
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add<Position>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTag<Examples.MyTag1>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTags(default));
        AreEqual(expect, e!.Message);
    }
    
    [Test]
    public static void Test_BatchCreate_CreateEntity_exception()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var batch = store.Batch();
        AreEqual(0, store.PendingCreateEntityBatchCount);
        var entity = batch
            .Add<Position>()
            .Add<Rotation>()
            .CreateEntity();
        AreEqual("id: 1  [Position, Rotation]", entity.ToString());
        AreEqual("batch returned",              batch.ToString());
        AreEqual(1, store.PendingCreateEntityBatchCount);
        
        var expect = "batch already returned";
        
        var e = Throws<BatchAlreadyReturnedException> (() => batch.CreateEntity());
        AreEqual(expect, e!.Message);
        AreEqual(1, store.PendingCreateEntityBatchCount);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add(new Position()));
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.Add<Position>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTag<Examples.MyTag1>());
        AreEqual(expect, e!.Message);
        
        e = Throws<BatchAlreadyReturnedException> (() => batch.AddTags(default));
        AreEqual(expect, e!.Message);
        AreEqual(1, store.PendingCreateEntityBatchCount);
    }
    
    
    [Test]
    public static void Test_BatchCreate_CreateEntity_Perf()
    {
        int count = 10;  // 10_000_000 ~ #PC: 983 ms
        var store = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        
        var sw = new Stopwatch();
        sw.Start();
        
        for (int n = 0; n < count; n++)
        {
            store.Batch(false)
                .Add    <Position>()
                .Add    <Rotation>()
                .AddTag <TestTag>()
                .AddTag <TestTag2>()
                .CreateEntity();
        }
        Console.WriteLine($"CreateBatch - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(count, store.Count);
        AreEqual(0, store.PendingCreateEntityBatchCount);
    }
}



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
    public static void Test_CreateBatch_CreateEntity()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        
        var addTags     = Tags.Get<TestTag2>();
        
        var batch = store.CreateBatch;
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
        
        AreEqual(new Position(),            entity.Position);
        AreEqual(new Rotation (1,2,3,4),    entity.Rotation);
        
        batch.Clear();
        AreEqual(0, batch.ComponentCount);
        AreEqual(0, batch.TagCount);
    }
    
    [Test]
    public static void Test_Batch_Create_multiple_entities()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var batch = new CreateEntityBatch(store);
        batch.Add<Position>()
             .Add<Rotation>();
        
        batch.Get<Position>().x = 1;
        var entity1 = batch.CreateEntity();
        AreEqual(new Position(1, 0, 0), entity1.Position);

        batch.Get<Position>().x = 2;
        var entity2 = batch.CreateEntity();
        AreEqual(new Position(2, 0, 0), entity2.Position);
        
        var e = Throws<InvalidOperationException>(() => {
            batch.Get<MyComponent1>();
        });
        AreEqual("Get<>() requires a preceding Add<>(). Component: [MyComponent1]", e!.Message);
    }
    
    
    [Test]
    public static void Test_Batch_CreateEntity_Perf()
    {
        int count = 10;  // 10_000_000 ~ #PC: 983 ms
        var store = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        
        var sw = new Stopwatch();
        sw.Start();
        
        for (int n = 0; n < count; n++)
        {
            store.CreateBatch
                .Add    <Position>()
                .Add    <Rotation>()
                .AddTag <TestTag>()
                .AddTag <TestTag2>()
                .CreateEntity();
        }
        Console.WriteLine($"CreateBatch - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(count, store.Count);
    }
}



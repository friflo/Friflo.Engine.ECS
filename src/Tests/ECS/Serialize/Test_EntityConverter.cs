using System;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Friflo.Engine.ECS.NodeFlags;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public static class Test_EntityConverter
{
    private static DataEntity CreateDataEntity(int id, int[] childIds)
    {
        var children = new List<long>(childIds.Length);
        foreach (var childId in childIds) {
            children.Add(childId);
        }
        return new DataEntity { pid = id, children = children };
    }
    
    [Test]
    public static void Test_EntityConverter_Load_single_entity() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        var entity2 = converter.DataEntityToEntity(new DataEntity { pid = 2 }, store, out _);
        
        AreEqual(2,             entity2.Id);
        AreEqual(0,             entity2.ChildEntities.Count);
        AreEqual(1,             store.Count);
        AreEqual("Entity[0]",   entity2.ChildEntities.ToString());
        AreEqual("entities: 1", store.ToString());
    }
    
    [Test]
    public static void Test_EntityConverter_Load_parent_child() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;

        // --- create parent 5 first
        var entity5 = converter.DataEntityToEntity(CreateDataEntity(5, new [] { 8 }), store, out _);
        AreEqual(5,                     entity5.Id);
        var ids     = entity5.ChildEntities.Ids;
        AreEqual(1,                     ids.Length);
        AreEqual(8,                     ids[0]);
        var entity8 = store.GetEntityById(8);
        IsTrue  (                       store.GetEntityById(8).IsNull);
        ref readonly var node8 = ref store.GetEntityNode(8);
        AreEqual(NullNode,              node8.Flags);      // diff_flags
    //  AreEqual(0,                     node8.Id);
        AreEqual(5,                     store.GetInternalParentId(entity8.Id));   // diff_parent
        AreEqual(1,                     store.Count);
        
        // --- create child 8
        entity8 = converter.DataEntityToEntity(new DataEntity { pid = 8 }, store, out _);
        IsTrue  (entity8 ==             store.GetEntityById(8));
        AreEqual(Created,               node8.Flags);
    //  AreEqual(8,                     node8.Id);
        AreEqual(5,                     entity8.Parent.Id);
        AreEqual(2,                     store.Count);
        //
        IsTrue  (                       store.StoreRoot.IsNull);
        store.SetStoreRoot(entity5);
        AreEqual(Created,               node8.Flags);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_child_parent() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        // --- create child 8 first
        var entity8 = converter.DataEntityToEntity(new DataEntity { pid = 8 }, store, out _);
        IsTrue  (entity8 ==             store.GetEntityById(8));
        
        ref readonly var node8 = ref store.GetEntityNode(8);
        AreEqual(Created,               node8.Flags);      // diff_flags
    //  AreEqual(8,                     node8.Id);
        AreEqual(0,                     entity8.Parent.Id);   // diff_parent
        AreEqual(1,                     store.Count);

        // --- create parent 5
        var entity5 = converter.DataEntityToEntity(CreateDataEntity(5, new [] { 8 }), store, out _);
        AreEqual(5,                     entity5.Id);
        var ids     = entity5.ChildEntities.Ids;
        AreEqual(1,                     ids.Length);
        AreEqual(8,                     ids[0]);
        AreEqual(Created,               node8.Flags);
    //  AreEqual(8,                     node8.Id);
        AreEqual(5,                     entity8.Parent.Id);
        AreEqual(2,                     store.Count);
        
        //
        IsTrue(                         store.StoreRoot.IsNull);
        store.SetStoreRoot(entity5);
        AreEqual(Created,               node8.Flags);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_CreateFrom_assertions() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        {
            var e = Throws<ArgumentException>(() => {
                var entity = new DataEntity { pid = 0 };
                converter.DataEntityToEntity(entity, store, out _);    
            });
            AreEqual("pid must be in range [1, 2147483647] when using PidType.UsePidAsId. was: 0 (Parameter 'DataEntity.pid')", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                var entity = new DataEntity { pid = 1, children = new List<long> { 2147483647L + 1 }};
                converter.DataEntityToEntity(entity, store, out _);    
            });
            AreEqual("pid must be in range [1, 2147483647] when using PidType.UsePidAsId. was: 2147483648 (Parameter 'DataEntity.children')", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                var entity = new DataEntity { pid = 1, children = new List<long> { 0 }};
                converter.DataEntityToEntity(entity, store, out _);    
            });
            AreEqual("pid must be in range [1, 2147483647] when using PidType.UsePidAsId. was: 0 (Parameter 'DataEntity.children')", e!.Message);
        }
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_multiple_parents_1() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DataEntityToEntity(CreateDataEntity(1, new [] { 2, 3 }), store, out _);

        var e = Throws<InvalidOperationException> (() => {
            _ = converter.DataEntityToEntity(CreateDataEntity(2, new [] { 3 }), store, out _);
        });
        AreEqual("child has already a parent. child: 3 current parent: 1, new parent: 2", e!.Message);
    }

    [Test]
    public static void Test_EntityConverter_Load_error_multiple_parents_2() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DataEntityToEntity(CreateDataEntity(1, new [] { 2 }), store, out _);

        var e = Throws<InvalidOperationException> (() => {
            converter.DataEntityToEntity(CreateDataEntity(3, new [] { 2 }), store, out _);
        });
        AreEqual("child has already a parent. child: 2 current parent: 1, new parent: 3", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_cycle_1() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var e = Throws<InvalidOperationException> (() => {
            converter.DataEntityToEntity(CreateDataEntity(1, new [] { 1 }), store, out _);
        });
        AreEqual("self reference in entity: 1", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_cycle_2() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DataEntityToEntity(CreateDataEntity(1, new [] { 2 }), store, out _);
        
        var e = Throws<InvalidOperationException> (() => {
            converter.DataEntityToEntity(CreateDataEntity(2, new [] { 1 }), store, out _);
        });
        AreEqual("cycle in entity children: 2 -> 1 -> 2", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_cycle_3() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DataEntityToEntity(CreateDataEntity(1, new [] { 2 }), store, out _);
        converter.DataEntityToEntity(CreateDataEntity(2, new [] { 3 }), store, out _);

        var e = Throws<InvalidOperationException> (() => {
            converter.DataEntityToEntity(CreateDataEntity(3, new [] { 1 }), store, out _);
        });
        AreEqual("cycle in entity children: 3 -> 2 -> 1 -> 3", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_Perf() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        int count       = 10; // 10_000_000 ~ #PC: 2.199 ms
        var entity = new DataEntity();
        for (int n = 1; n <= count; n++) {
            entity.pid = n;
            _ = converter.DataEntityToEntity(entity, store, out _);
        }
        AreEqual(count, store.Count);
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int entityCount = 0;
        var nodeMax     = store.NodeMaxId;
        for (int id = 1; id <= nodeMax; id++) {
            var node = store.GetEntityNode(id);
            if (node.Archetype != null) {
                entityCount++;
            }
        }
        AreEqual(count, entityCount);
        Console.WriteLine($"{count} iterations: {stopwatch.ElapsedMilliseconds} ms");
    }
}

}
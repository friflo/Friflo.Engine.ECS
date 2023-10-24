using System;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Sync;

public static class Test_EntityConverter
{
    private static DatabaseEntity CreateDbEntity(int id, int[] childIds)
    {
        List<long> children = null;
        if (childIds != null) {
            children = new List<long>(childIds.Length);
            foreach (var childId in childIds) {
                children.Add(childId);
            }
        }
        var entity = new DatabaseEntity {
            pid = id,
            children = children
        };
        return entity;
    }
    
    [Test]
    public static void Test_EntityConverter_Load_single_entity() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        var entity2 = converter.DatabaseToGameEntity(new DatabaseEntity { pid = 2 }, store, out _);
        
        AreEqual(2, entity2.Id);
        AreEqual(0, entity2.ChildNodes.Length);
        AreEqual(1, store.EntityCount);
        AreEqual("Length: 0", entity2.ChildNodes.ToString());
    }
    
    [Test]
    public static void Test_EntityConverter_Load_parent_child() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;

        // --- create parent 5 first
        var entity5 = converter.DatabaseToGameEntity(CreateDbEntity(5, new [] { 8 }), store, out _);
        AreEqual(5,                     entity5.Id);
        var ids     = entity5.ChildNodes.Ids;
        AreEqual(1,                     ids.Length);
        AreEqual(8,                     ids[0]);
        IsNull  (                       store.Nodes[8].Entity);
        AreEqual(NullNode,              store.Nodes[8].Flags);      // diff_flags
        AreEqual(8,                     store.Nodes[8].Id);
        AreEqual(5,                     store.Nodes[8].ParentId);   // diff_parent
        AreEqual(1,                     store.EntityCount);
        
        // --- create child 8
        var entity8 = converter.DatabaseToGameEntity(new DatabaseEntity { pid = 8 }, store, out _);
        AreSame (entity8,               store.Nodes[8].Entity);
        AreEqual(Created,               store.Nodes[8].Flags);
        AreEqual(8,                     store.Nodes[8].Id);
        AreEqual(5,                     store.Nodes[8].ParentId);
        AreEqual(2,                     store.EntityCount);
        //
        IsNull(                         store.StoreRoot);
        store.SetStoreRoot(entity5);
        AreEqual(Created | TreeNode,store.Nodes[8].Flags);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_child_parent() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        // --- create child 8 first
        var entity8 = converter.DatabaseToGameEntity(new DatabaseEntity { pid = 8 }, store, out _);
        AreSame (entity8,               store.Nodes[8].Entity);
        AreEqual(Created,               store.Nodes[8].Flags);      // diff_flags
        AreEqual(8,                     store.Nodes[8].Id);
        AreEqual(0,                     store.Nodes[8].ParentId);   // diff_parent
        AreEqual(1,                     store.EntityCount);

        // --- create parent 5
        var entity5 = converter.DatabaseToGameEntity(CreateDbEntity(5, new [] { 8 }), store, out _);
        AreEqual(5,                     entity5.Id);
        var ids     = entity5.ChildNodes.Ids;
        AreEqual(1,                     ids.Length);
        AreEqual(8,                     ids[0]);
        AreEqual(Created,               store.Nodes[8].Flags);
        AreEqual(8,                     store.Nodes[8].Id);
        AreEqual(5,                     store.Nodes[8].ParentId);
        AreEqual(2,                     store.EntityCount);
        
        //
        IsNull(                         store.StoreRoot);
        store.SetStoreRoot(entity5);
        AreEqual(Created | TreeNode,store.Nodes[8].Flags);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_CreateFrom_assertions() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        {
            var e = Throws<ArgumentException>(() => {
                var entity = new DatabaseEntity { pid = 0 };
                converter.DatabaseToGameEntity(entity, store, out _);    
            });
            AreEqual("pid must be in range [1, 2147483647] when using PidType.UsePidAsId. was: 0 (Parameter 'DatabaseEntity.pid')", e!.Message);
        } {
            var e = Throws<ArgumentException>(() => {
                var entity = new DatabaseEntity { pid = 1, children = new List<long> { 2147483647L + 1 }};
                converter.DatabaseToGameEntity(entity, store, out _);    
            });
            AreEqual("pid must be in range [1, 2147483647] when using PidType.UsePidAsId. was: 2147483648 (Parameter 'DatabaseEntity.children')", e!.Message);
        }
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_multiple_parents_1() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DatabaseToGameEntity(CreateDbEntity(1, new [] { 2, 3 }), store, out _);

        var e = Throws<InvalidOperationException> (() => {
            _ = converter.DatabaseToGameEntity(CreateDbEntity(2, new [] { 3 }), store, out _);
        });
        AreEqual("child has already a parent. child: 3 current parent: 1, new parent: 2", e!.Message);
    }

    [Test]
    public static void Test_EntityConverter_Load_error_multiple_parents_2() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DatabaseToGameEntity(CreateDbEntity(1, new [] { 2 }), store, out _);

        var e = Throws<InvalidOperationException> (() => {
            converter.DatabaseToGameEntity(CreateDbEntity(3, new [] { 2 }), store, out _);
        });
        AreEqual("child has already a parent. child: 2 current parent: 1, new parent: 3", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_cycle_1() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var e = Throws<InvalidOperationException> (() => {
            converter.DatabaseToGameEntity(CreateDbEntity(1, new [] { 1 }), store, out _);
        });
        AreEqual("self reference in entity: 1", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_cycle_2() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DatabaseToGameEntity(CreateDbEntity(1, new [] { 2 }), store, out _);
        
        var e = Throws<InvalidOperationException> (() => {
            converter.DatabaseToGameEntity(CreateDbEntity(2, new [] { 1 }), store, out _);
        });
        AreEqual("dependency cycle in entity children: 2 -> 1 -> 2", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_error_cycle_3() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        converter.DatabaseToGameEntity(CreateDbEntity(1, new [] { 2 }), store, out _);
        converter.DatabaseToGameEntity(CreateDbEntity(2, new [] { 3 }), store, out _);

        var e = Throws<InvalidOperationException> (() => {
            converter.DatabaseToGameEntity(CreateDbEntity(3, new [] { 1 }), store, out _);
        });
        AreEqual("dependency cycle in entity children: 3 -> 2 -> 1 -> 3", e!.Message);
    }
    
    [Test]
    public static void Test_EntityConverter_Load_Perf() {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        int count       = 10; // 10_000_000 ~ 2.199 ms
        var entity = new DatabaseEntity();
        for (int n = 1; n <= count; n++) {
            entity.pid = n;
            _ = converter.DatabaseToGameEntity(entity, store, out _);
        }
        AreEqual(count, store.EntityCount);
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int entityCount = 0;
        var nodes       = store.Nodes;
        var nodeMax     = store.NodeMaxId;
        for (int n = 1; n <= nodeMax; n++) {
            if (nodes[n].Entity != null) {
                entityCount++;
            }
        }
        AreEqual(count, entityCount);
        Console.WriteLine($"{count} iterations: {stopwatch.ElapsedMilliseconds} ms");
    }
}
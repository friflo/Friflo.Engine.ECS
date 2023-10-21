using System;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Database;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;

public static class Test_GameDatabase
{
    [Test]
    public static void Test_GameDatabase_Load_DatabaseEntity_UsePidAsId()
    {
        var store   = TestUtils.CreateGameEntityStore(out var database);
        var node    = new DatabaseEntity{ pid = 10, children = new List<long> { 20 } };
        var entity  = database.LoadEntity(node, out _);
        
        AreEqual(10,    store.PidToId(10));
        AreEqual(10,    store.GetNodeByPid(10).Pid);
        AreEqual(10,    entity.Id);
        AreEqual(1,     entity.ChildNodes.Length);
        AreEqual(1,     store.Nodes[10].ChildCount);
        AreEqual(10,    store.Nodes[10].Pid);
        AreEqual(20,    store.Nodes[10].ChildIds[0]);
        AreEqual(10,    store.Nodes[20].ParentId);
        AreEqual(20,    store.Nodes[20].Pid);
        AreEqual(1,     store.EntityCount);
    }
    
    [Test]
    public static void Test_GameDatabase_Load_DatabaseEntity_RandomPids() {
        var store   = TestUtils.CreateGameEntityStore(out var database, PidType.RandomPids);
        var node    = new DatabaseEntity{ pid = 10, children = new List<long> { 20 } };
        var entity  = database.LoadEntity(node, out _);
        
        AreEqual(1,     store.PidToId(10));
        AreEqual(1,     store.GetNodeByPid(10).Id);
        AreEqual(1,     entity.Id);
        AreEqual(1,     entity.ChildNodes.Length);
        AreEqual(1,     store.Nodes[1].ChildCount);
        AreEqual(10,    store.Nodes[1].Pid);
        AreEqual(2,     store.Nodes[1].ChildIds[0]);
        AreEqual(1,     store.Nodes[2].ParentId);
        AreEqual(20,    store.Nodes[2].Pid);
        AreEqual(1,     store.EntityCount);
    }
    
    [Test]
    public static void Test_GameDatabase_Load_single_entity() {
        var store   = new GameEntityStore();
        var entity2 = store.CreateFrom(2);
        
        AreEqual(2, entity2.Id);
        AreEqual(0, entity2.ChildNodes.Length);
        AreEqual(1, store.EntityCount);
        AreEqual("Length: 0", entity2.ChildNodes.ToString());
    }
    
    [Test]
    public static void Test_GameDatabase_Load_parent_child() {
        var store   = new GameEntityStore();

        // --- create parent 5 first
        var entity5 = store.CreateFrom(5, new [] { 8 });
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
        var entity8 = store.CreateFrom(8);
        AreSame (entity8,               store.Nodes[8].Entity);
        AreEqual(Created,               store.Nodes[8].Flags);
        AreEqual(8,                     store.Nodes[8].Id);
        AreEqual(5,                     store.Nodes[8].ParentId);
        AreEqual(2,                     store.EntityCount);
        //
        IsNull(                         store.GraphOrigin);
        store.SetGraphOrigin(entity5);
        AreEqual(Created | TreeNode,store.Nodes[8].Flags);
    }
    
    [Test]
    public static void Test_GameDatabase_Load_child_parent() {
        var store   = new GameEntityStore();
        
        // --- create child 8 first
        var entity8 = store.CreateFrom(8);
        AreSame (entity8,               store.Nodes[8].Entity);
        AreEqual(Created,               store.Nodes[8].Flags);      // diff_flags
        AreEqual(8,                     store.Nodes[8].Id);
        AreEqual(0,                     store.Nodes[8].ParentId);   // diff_parent
        AreEqual(1,                     store.EntityCount);

        // --- create parent 5
        var entity5 = store.CreateFrom(5, new [] { 8 });
        AreEqual(5,                     entity5.Id);
        var ids     = entity5.ChildNodes.Ids;
        AreEqual(1,                     ids.Length);
        AreEqual(8,                     ids[0]);
        AreEqual(Created,               store.Nodes[8].Flags);
        AreEqual(8,                     store.Nodes[8].Id);
        AreEqual(5,                     store.Nodes[8].ParentId);
        AreEqual(2,                     store.EntityCount);
        
        //
        IsNull(                         store.GraphOrigin);
        store.SetGraphOrigin(entity5);
        AreEqual(Created | TreeNode,store.Nodes[8].Flags);
    }
    
    [Test]
    public static void Test_GameDatabase_Load_CreateFrom_assertions() {
        var store   = new GameEntityStore();
        {
            var e = Throws<ArgumentException>(() => {
                store.CreateFrom(0);    
            });
            AreEqual("invalid entity id <= 0. was: 0 (Parameter 'id')", e!.Message);
        } {
            store.CreateFrom(1);
            var e = Throws<ArgumentException>(() => {
                store.CreateFrom(1);    
            });
            AreEqual("id already in use in EntityStore. id: 1 (Parameter 'id')", e!.Message);
        }
    }
    
    [Test]
    public static void Test_GameDatabase_Load_error_multiple_parents_1() {
        var store   = new GameEntityStore();
        
        store.CreateFrom(1, new [] { 2, 3 });

        var e = Throws<InvalidOperationException> (() => {
            _ = store.CreateFrom(2, new [] { 3 });
        });
        AreEqual("child has already a parent. child: 3 current parent: 1, new parent: 2", e!.Message);
    }

    [Test]
    public static void Test_GameDatabase_Load_error_multiple_parents_2() {
        var store   = new GameEntityStore();
        
        store.CreateFrom(1, new [] { 2 });

        var e = Throws<InvalidOperationException> (() => {
            store.CreateFrom(3, new [] { 2 });
        });
        AreEqual("child has already a parent. child: 2 current parent: 1, new parent: 3", e!.Message);
    }
    
    [Test]
    public static void Test_GameDatabase_Load_error_cycle_1() {
        var store   = new GameEntityStore();
        
        var e = Throws<InvalidOperationException> (() => {
            store.CreateFrom(1, new [] { 1 });
        });
        AreEqual("self reference in entity: 1", e!.Message);
    }
    
    [Test]
    public static void Test_GameDatabase_Load_error_cycle_2() {
        var store   = new GameEntityStore();
        
        store.CreateFrom(1, new [] { 2 });
        
        var e = Throws<InvalidOperationException> (() => {
            store.CreateFrom(2, new [] { 1 });
        });
        AreEqual("dependency cycle in entity children: 2 -> 1 -> 2", e!.Message);
    }
    
    [Test]
    public static void Test_GameDatabase_Load_error_cycle_3() {
        var store   = new GameEntityStore();
        
        store.CreateFrom(1, new [] { 2 });
        store.CreateFrom(2, new [] { 3 });

        var e = Throws<InvalidOperationException> (() => {
            store.CreateFrom(3, new [] { 1 });
        });
        AreEqual("dependency cycle in entity children: 3 -> 2 -> 1 -> 3", e!.Message);
    }
    
    [Test]
    public static void Test_GameDatabase_assertions() {
        TestUtils.CreateGameEntityStore(out var database);
        {
            var e = Throws<ArgumentNullException>(() => {
                database.LoadEntity(null, out _);    
            });
            AreEqual("Value cannot be null. (Parameter 'databaseEntity')", e!.Message);
        } {
            var e = Throws<ArgumentNullException>(() => {
                database.StoreEntity(null);    
            });
            AreEqual("Value cannot be null. (Parameter 'entity')", e!.Message);
        } {
            var store2 = new GameEntityStore(PidType.UsePidAsId);
            var entity = store2.CreateEntity();
            var e = Throws<ArgumentException>(() => {
                database.StoreEntity(entity);    
            });
            AreEqual("entity is owned by a different store (Parameter 'entity')", e!.Message);
        }
    }
    
    [Test]
    public static void Test_GameDatabase_Load_Perf() {
        var store       = new GameEntityStore();
        int count       = 10; // 10_000_000 ~ 2.117 ms
        for (int n = 1; n <= count; n++) {
            _ = store.CreateFrom(n);
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
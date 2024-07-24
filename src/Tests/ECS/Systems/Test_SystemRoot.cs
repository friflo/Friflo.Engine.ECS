// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemRoot
    {
        [Test]
        public static void Test_SystemRoot_minimal()
        {
            var store   = new EntityStore();
            var entity  = store.CreateEntity(new Position());
            var root    = new SystemRoot(store) { new TestMoveSystem() };
            root.Update(default);
            AreEqual(new Position(1,0,0), entity.Position);
        }
        
        class TestMoveSystem : QuerySystem<Position>
        {
            protected override void OnUpdate() {
                Query.ForEachEntity((ref Position position, Entity _) => {
                    position.x++;
                });
            }
        }
        
        [Test]
        public static void Test_SystemRoot_Tick() {
            var tick = new UpdateTick(42, 0);
            AreEqual("deltaTime: 42", tick.ToString());
        }
        
        [Test]
        public static void Test_SystemRoot_constructor_default_name()
        {
            var root1   = new SystemRoot();
            AreEqual("Systems", root1.Name);
            
            var store   = new EntityStore(PidType.UsePidAsId);
            var root2   = new SystemRoot(store);
            AreEqual("Systems", root2.Name);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_Group()
        {
            var store       = new EntityStore();
            var root        = new SystemRoot("Systems");
            var group1      = new SystemGroup("Group1");
            var testGroup   = new TestGroup();
            
            IsNull(root.FindGroup("Group1",     true));
            IsNull(root.FindGroup("TestGroup",  true));
            
            
            root.Add(group1);
            group1.Add(testGroup);
            
            AreEqual("'Systems' Root - child systems: 1", root.ToString());
            AreEqual("'Group1' Group - child systems: 1", group1.ToString());
            AreEqual("'TestGroup' Group - child systems: 0", testGroup.ToString());
            
            AreSame(group1,     root.FindGroup("Group1",    true));
            AreSame(group1,     root.FindGroup("Group1",    false));
            AreSame(testGroup,  root.FindGroup("TestGroup", true));
            IsNull (            root.FindGroup("TestGroup", false));
            
            AreEqual(1, root.ChildSystems.Count);
            AreEqual(1, group1.ChildSystems.Count);
            
            root.AddStore(store);
            AreEqual(1, root.Stores.Count);
            
            root.SetMonitorPerf(true);
            var tick = new UpdateTick(42, 0);
            root.Update(tick);
            
            Console.WriteLine(root.GetPerfLog());
            
            AreEqual(1, testGroup.beginCalled);
            AreEqual(1, testGroup.endCalled);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_System()
        {
            var store       = new EntityStore();
            var entity      = store.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot(store);    // create SystemRoot with store
            var testGroup   = new SystemGroup("Update");
            root.Add(testGroup);
            var testSystem1 = new TestSystem1();
            AreEqual("TestSystem1 - [Position]", testSystem1.ToString());
            AreEqual("Components: [Position]", testSystem1.ComponentTypes.ToString());
            AreEqual(0,     testSystem1.Queries.Count);
            testGroup.Add(testSystem1);
            AreEqual(1,     testSystem1.Queries.Count);
            AreEqual(1,     testSystem1.EntityCount);
            AreSame(root,   testSystem1.SystemRoot);
            
            root.SetMonitorPerf(true);
            Console.WriteLine(root.GetPerfLog());
            AreEqual(
@"stores: 1                     on      last ms       sum ms      updates     last mem      sum mem     entities
---------------------         --     --------     --------     --------     --------     --------     --------
Systems [1]                    +       -1.000        0.000            0            0            0
| Update [1]                   +       -1.000        0.000            0            0            0
|   TestSystem1                +       -1.000        0.000            0            0            0            1
", root.GetPerfLog());
            
            AreEqual(
@"stores: 1                     on      last ms       sum ms      updates     last mem      sum mem     entities
---------------------         --     --------     --------     --------     --------     --------     --------
TestSystem1                    +       -1.000        0.000            0            0            0            1
", testSystem1.GetPerfLog());
            
            var tick = new UpdateTick(42, 0);
            root.Update(tick);
            
            AreEqual(new Scale3(4,5,6), entity.Scale3); // added by testSystem1
            AreEqual(42, testSystem1.Tick.deltaTime);
            AreEqual(42, testGroup.Tick.deltaTime);
            AreEqual(42, root.Tick.deltaTime);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_RemoveStore()
        {
            var store1      = new EntityStore();
            var store2      = new EntityStore();
            store1.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot("Systems");   // create SystemRoot without store
            var group       = new SystemGroup("Group");
            var testSystem2 = new TestSystem2();
            group.Add(testSystem2);
            root.Add(group);
            
            // --- add store
            AreEqual(0, testSystem2.Queries.Count);
            root.AddStore(store1);                      // add store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem2.Queries.Count);
            AreEqual(1, testSystem2.EntityCount);
            root.Update(default);
            
            root.AddStore(store2);                      // add store after system setup
            AreEqual(2, root.Stores.Count);
            AreEqual(2, testSystem2.Queries.Count);
            
            // --- remove store
            root.RemoveStore(store1);                   // remove store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem2.Queries.Count);
            
            root.RemoveStore(store1);                   // remove same store again for coverage
            
            root.RemoveStore(store2);                   // remove store after system setup
            AreEqual(0, root.Stores.Count);
            AreEqual(0, testSystem2.Queries.Count);
        }
        
        [Test]
        public static void Test_SystemRoot_Name()
        {
            var group = new SystemGroup("TestGroup");
            AreEqual("TestGroup", group.Name);
            
            group.SetName("changed name");
            AreEqual("changed name", group.Name);
            
            var testSystem1 = new TestSystem1();
            AreEqual("TestSystem1", testSystem1.Name);
            
            var mySystem1 = new MySystem1();
            AreEqual("MySystem1", mySystem1.Name);
            
            var mySystem2 = new MySystem2();
            AreEqual("MySystem2 - custom name", mySystem2.Name);
        }
        
        [Test]
        public static void Test_Systems_BaseSystem_coverage()
        {
            var root            = new SystemRoot("Systems");
            var customSystem    = new MySystem1();
            root.Add(customSystem);
            var store = new EntityStore();
            root.AddStore(store);
            root.RemoveStore(store);
            root.Update(default);
        }
        
        [Test]
        public static void Test_SystemRoot_exceptions()
        {
            var group = new SystemRoot("Systems");
            Throws<ArgumentNullException>(() => {
                group.AddStore(null);
            });
            Throws<ArgumentNullException>(() => {
                group.RemoveStore(null);
            });
        }
        
        [Test]
        public static void Test_SystemRoot_Update_Perf()
        {
            bool monitorPerf    = false;
            int  count          = 10;
                                // 100_000_000 - monitorPerf: false ~ #PC: 3682 ms (overhead of perf conditions ~ 250 ms)
                                // 100_000_000 - monitorPerf: true  ~ #PC: 8128 ms
            var store       = new EntityStore();
            var root        = new SystemRoot("Systems");
            var testSystem2 = new TestSystem2();
            root.Add(testSystem2);
            root.AddStore(store);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            root.SetMonitorPerf(monitorPerf);
            root.Update(default);

            var sw      = new Stopwatch();
            var start   = Mem.GetAllocatedBytes();
            sw.Start();
            for (int n = 0; n < count; n++) {
                root.Update(default);
            }
            Mem.AssertNoAlloc(start);
            Console.WriteLine($"Test_SystemRoot_Update_Perf - count: {count}, duration: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"SystemRoot  - DurationSumMs: {root.Perf.SumMs}");
            Console.WriteLine($"TestSystem2 - DurationSumMs: {testSystem2.Perf.SumMs}");
        }
    }
    

}
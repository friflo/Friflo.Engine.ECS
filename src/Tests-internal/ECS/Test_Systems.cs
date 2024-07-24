// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Text;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using Tests.ECS.Systems;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Internal.ECS {
    
    // ReSharper disable once InconsistentNaming
    public static class Test_Systems
    {
        [Test]
        public static void Test_Systems_SystemGroup_default_name()
        {
            var system = new SystemGroup();
            AreEqual("System", system.Name);
        }
        
        [Test]
        public static void Test_Systems_SystemChange_cover()
        {
            var system = new TestSystem1();
            var changed = new SystemChanged((SystemChangedAction)99, system, null, null);
            AreEqual("99 - System 'TestSystem1'", changed.ToString());
        }
        
        [Test]
        public static void Test_Systems_View()
        {
            var root        = new SystemRoot("Systems");
            var querySystem = new TestQuerySystem();
            root.Add(querySystem);
            
            var view = querySystem.System;
            AreEqual("TestQuerySystem",         view.Name);
            AreEqual("Enabled: True  Id: 1",    view.ToString());
            AreEqual(1,                         view.Id);
            AreEqual(true,                      view.Enabled);
            AreEqual(new UpdateTick(),          view.Tick);
            AreSame (root,                      view.SystemRoot);
            AreSame (root,                      view.ParentGroup);
            AreEqual(0,                         view.Perf.UpdateCount);
            AreEqual(-1d,                       view.Perf.LastMs);
            AreEqual(0,                         view.Perf.SumMs);
            AreEqual(10,                        view.Perf.history.Length);
            
            NotNull(root.System);
            NotNull(root.CommandBuffers);
        }
        
        
        [Test]
        public static void Test_Systems_Id()
        {
            var root        = new SystemRoot("Systems");
            var group1      = new SystemGroup("Test 1");
            var group2      = new SystemGroup("Test 2");
            var querySystem = new TestQuerySystem();
            AreEqual(0, root.Id);
            AreEqual(0, group1.Id);
            AreEqual(0, querySystem.Id);
            
            group1.id       = 1;
            group2.id       = 1;
            querySystem.id  = 3;
            
            root.Add(group1);
            root.Add(group2);
            root.Add(querySystem);
            
            AreEqual(1, group1.Id);
            AreEqual(2, group2.Id);
            AreEqual(3, querySystem.Id);
        }
        
        [Test]
        public static void Test_Systems_constructor()
        {
            var store = new EntityStore(PidType.UsePidAsId);
            var root  = new SystemRoot(store, "Systems");
            var child = new SystemGroup();
            child.SetName("Child");
            
            root.Add(child);
            AreSame(store, child.CommandBuffers[0].EntityStore);
            
            var testQuerySystem = new TestQuerySystem();
            child.Add(testQuerySystem);
            AreSame(testQuerySystem, child.ChildSystems[0]);
        }
        
        [Test]
        public static void Test_Systems_DebugView()
        {
            var root        = new SystemRoot("Systems");
            var rootView    = new SystemRootDebugView(root);
            AreEqual(0, rootView.ChildSystems.Count);
            AreEqual(0, rootView.Stores.Count);
            
            var group       =  new SystemGroup("Update");
            var groupView   = new SystemGroupDebugView(group);
            AreEqual(0, groupView.ChildSystems.Count);
        }
        
        [Test]
        public static void Test_Systems_AllSystems()
        {
            var group = new SystemGroup("Update") {
                new TestSystem1(),
                new MySystem1 { Enabled = false }
            };
            var root  = new SystemRoot("Systems") { group };
            var rootSystems = root.AllSystems;
            AreEqual(4, rootSystems.Length);
            AreEqual("0 - 'Systems' Root - child systems: 1",   rootSystems[0].ToString());
            AreEqual("1 - 'Update' Group - child systems: 2",   rootSystems[1].ToString());
            AreEqual("2 - TestSystem1 - entities: 0",           rootSystems[2].ToString());
            AreEqual("3 - MySystem1",           rootSystems[3].ToString());
            
            var groupSystems = group.AllSystems;
            AreEqual(3, groupSystems.Length);
            AreEqual("1 - 'Update' Group - child systems: 2",   groupSystems[0].ToString());
            AreEqual("2 - TestSystem1 - entities: 0",           groupSystems[1].ToString());
            AreEqual("3 - MySystem1",                           groupSystems[2].ToString());
            
            // --- assert log allocations
            root.SetMonitorPerf(true);
            var sb = new StringBuilder();
            root.AppendPerfLog(sb);
            var log = sb.ToString();
            AreEqual(
@"stores: 0                     on      last ms       sum ms      updates     last mem      sum mem     entities
---------------------         --     --------     --------     --------     --------     --------     --------
Systems [1]                    +       -1.000        0.000            0            0            0
| Update [2]                   +       -1.000        0.000            0            0            0
|   TestSystem1                +       -1.000        0.000            0            0            0            0
|   MySystem1                  -       -1.000        0.000            0            0            0
", log);
            
            sb.Clear();
            
            var start = Mem.GetAllocatedBytes();
            root.AppendPerfLog(sb);
            Mem.AssertNoAlloc(start);
        }
    }
    
    class TestQuerySystem : QuerySystem<Position> {
        protected override void OnUpdate() { }
    }
}
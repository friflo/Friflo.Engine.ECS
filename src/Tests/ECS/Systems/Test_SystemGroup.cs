// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToLocalFunction
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemGroup
    {
        [Test]
        public static void Test_SystemGroup_collection_initializer()
        {
            var root = new SystemRoot("Systems") {
                new SystemGroup("Start") {
                    new TestSystem1(),
                },
                new SystemGroup("Update") {
                    new ScaleSystem(),
                    new PositionSystem()
                }
            };
            AreEqual(2,         root.ChildSystems.Count);
            AreEqual("Start",   root.ChildSystems[0].Name);
            AreEqual("Update",  root.ChildSystems[1].Name);

            int count = 0;
            foreach (var _ in root) {
                count++;
            }
            AreEqual(2, count);
            
            count = 0;
            IEnumerable enumerable = root;
            foreach (var _ in enumerable) {
                count++;
            }
            AreEqual(2, count);
        }
    
        [Test]
        public static void Test_SystemGroup_MoveTo()
        {
            int changesRoot     = 0;
            int changesGroup1   = 0;
            int changesGroup2   = 0;
            int changesGroup3   = 0;
            var root    = new SystemRoot("Systems");
            var group1  = new SystemGroup("Group1");
            var group2  = new SystemGroup("Group2");
            var group3  = new SystemGroup("Group3");
            Action<SystemChanged> rootChanged = changed => {
                var str = changed.ToString();
                switch (changesRoot++) {
                    case 0: AreEqual("Add - Group 'Group1' to 'Systems'",                  str);   return;
                    case 1: AreEqual("Add - Group 'Group2' to 'Systems'",                  str);   return;
                    case 2: AreEqual("Add - Group 'Group3' to 'Systems'",                  str);   return;
                    case 3: AreEqual("Move - Group 'Group1' from 'Systems' to 'Group3'",  str);   return;
                    case 4: AreEqual("Move - Group 'Group2' from 'Systems' to 'Group3'",  str);   return;
                    case 5: AreEqual("Move - Group 'Group1' from 'Group3' to 'Group3'",   str);   return;
                    case 6: AreEqual("Move - Group 'Group2' from 'Group3' to 'Group3'",   str);   return;
                }
            };
            Action<SystemChanged> group1Changed = _ => {
                changesGroup1++;
            };
            Action<SystemChanged> group2Changed = _ => {
                changesGroup2++;
            };
            Action<SystemChanged> group3Changed = changed => {
                var str = changed.ToString();
                switch (changesGroup3++) {
                    case 0: AreEqual("Move - Group 'Group1' from 'Systems' to 'Group3'",  str);   return;
                    case 1: AreEqual("Move - Group 'Group2' from 'Systems' to 'Group3'",  str);   return;
                    case 2: AreEqual("Move - Group 'Group1' from 'Group3' to 'Group3'",   str);   return;
                    case 3: AreEqual("Move - Group 'Group2' from 'Group3' to 'Group3'",   str);   return;
                }
            };
            root.OnSystemChanged    += rootChanged;
            group1.OnSystemChanged  += group1Changed;
            group2.OnSystemChanged  += group2Changed;
            group3.OnSystemChanged  += group3Changed;
            
            root.Add(group1);
            root.Add(group2);
            root.Add(group3);
            
            AreEqual(0, group1.MoveSystemTo(group3, -1)); // -1  => add at tail
            AreEqual(1, group2.MoveSystemTo(group3,  1));
            AreEqual(1, group1.MoveSystemTo(group3,  2)); // returned index != passed index
            AreEqual(1, group2.MoveSystemTo(group3, -1)); // move to tail within same group

            AreEqual(1, root.ChildSystems.Count);
            AreEqual(2, group3.ChildSystems.Count);
            
            root.OnSystemChanged    -= rootChanged;
            group1.OnSystemChanged  -= group1Changed;
            group2.OnSystemChanged  -= group2Changed;
            group3.OnSystemChanged  -= group3Changed;
            
            group1.MoveSystemTo(root,   -1);
            group2.MoveSystemTo(root,    1);
            group2.MoveSystemTo(root,    0);
            
            AreEqual(7, changesRoot);
            AreEqual(0, changesGroup1);
            AreEqual(0, changesGroup2);
            AreEqual(4, changesGroup3);
        }
        
        [Test]
        public static void Test_SystemGroup_InsertSystemAt()
        {
            var root        = new SystemRoot("Systems");
            var group1      = new SystemGroup("Group1");
            var group2      = new SystemGroup("Group2");
            var testSystem1 = new TestSystem1();
            var testSystem2 = new TestSystem1();
            root.Add(group1);
            root.Add(group2);
            root.Insert(2,  testSystem1);
            root.Insert(-1, testSystem2);
            
            AreEqual(4,             root.ChildSystems.Count);
            AreSame(group1,         root.ChildSystems[0]);
            AreSame(group2,         root.ChildSystems[1]);
            AreSame(testSystem1,    root.ChildSystems[2]);
            AreSame(testSystem2,    root.ChildSystems[3]);
            
            AreSame(root, testSystem1.ParentGroup);
        }
        
        [Test]
        public static void Test_SystemGroup_InsertSystemAt_exceptions()
        {
            var group = new SystemGroup("Systems");
            Throws<ArgumentNullException>(() => {
                group.Insert(0, null);
            });
            
            Exception e = Throws<ArgumentException>(() => {
                group.Insert(-2, new TestSystem1());
            });
            AreEqual("invalid index: -2 (Parameter 'index')", e!.Message);
            
            e = Throws<ArgumentException>(() => {
                group.Insert(1, new TestSystem1());
            });
            AreEqual("invalid index: 1 (Parameter 'index')", e!.Message);

            e = Throws<ArgumentException>(() => {
                group.Insert(0, new SystemRoot("Systems"));
            });
            AreEqual("SystemRoot must not be a child system (Parameter 'system')", e!.Message);
            
            var testSystem1 = new TestSystem1();
            group.Add(testSystem1);
            e = Throws<ArgumentException>(() => {
                group.Insert(0, testSystem1);
            });
            AreEqual("system already added to Group 'Systems' (Parameter 'system')", e!.Message);
        }
        
        [Test]
        public static void Test_SystemGroup_RemoveSystem_with_root()
        {
            var store       = new EntityStore();
            var root        = new SystemRoot (store, "Systems");
            var group       = new SystemGroup("Update");
            var testSystem1 = new TestSystem1();
            var testSystem2 = new TestSystem2();
            var rootCount   = 0;
            var grouptCount = 0;
            Action<SystemChanged> rootChanged = changed => {
                var str = changed.ToString();
                switch (rootCount++) {
                    case 0: AreEqual("Add - System 'TestSystem1' to 'Systems'",         str);   return;
                    case 1: AreEqual("Remove - System 'TestSystem1' from 'Systems'",    str);   return;
                }
            };
            Action<SystemChanged> groupChanged = changed => {
                var str = changed.ToString();
                switch (grouptCount++) {
                    case 0: AreEqual("Add - System 'TestSystem2' to 'Update'",          str);   return;
                    case 1: AreEqual("Remove - System 'TestSystem2' from 'Update'",     str);   return;
                }
            };
            root. OnSystemChanged   += rootChanged;
            group.OnSystemChanged   += groupChanged;
            root.Add(testSystem1);
            group.Add(testSystem2);
            AreEqual(1, testSystem1.Queries.Count);
            AreEqual(1, root.ChildSystems.Count);
            
            root.Remove(testSystem1);
            AreEqual(0, testSystem1.Queries.Count);
            AreEqual(0, root.ChildSystems.Count);
            group.Remove(testSystem2);
            
            root. OnSystemChanged   -= rootChanged;
            group.OnSystemChanged   -= groupChanged;
            root.Add    (testSystem1);
            root.Remove (testSystem1);
            group.Add   (testSystem2);
            group.Remove(testSystem2);
            
            AreEqual(2, rootCount);
            AreEqual(2, grouptCount);
        }
        
        [Test]
        public static void Test_SystemGroup_RemoveSystem_without_root()
        {
            int count = 0;
            var baseGroup   = new SystemGroup ("Base");
            var testSystem1 = new TestSystem1();
            baseGroup.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Add - System 'TestSystem1' to 'Base'",     str);      return;
                    case 1: AreEqual("Remove - System 'TestSystem1' from 'Base'", str);     return;
                }
            };
            baseGroup.  Add(testSystem1);
            AreEqual(0, testSystem1.Queries.Count);
            AreEqual(1, baseGroup.ChildSystems.Count);
            
            baseGroup.  Remove(testSystem1);
            AreEqual(0, testSystem1.Queries.Count);
            AreEqual(0, baseGroup.ChildSystems.Count);
            
            AreEqual(2, count);
        }
        
        [Test]
        public static void Test_SystemGroup_MoveTo_exception()
        {
            var root1   = new SystemRoot("Systems-1");
            var group1  = new SystemGroup("Group1");
            root1.Add(group1);
            
            Throws<ArgumentNullException>(() => {
                group1.MoveSystemTo(null, 0);
            });
            
            Exception e = Throws<ArgumentException>(() => {
                group1.MoveSystemTo(root1, -2);
            });
            AreEqual("invalid index: -2", e!.Message);
            
            e = Throws<ArgumentException>(() => {
                group1.MoveSystemTo(root1, 2);
            });
            AreEqual("invalid index: 2", e!.Message);
            
            var group2      = new SystemGroup("Group2");
            e = Throws<InvalidOperationException>(() => {
                group2.MoveSystemTo(root1, 1);
            });
            AreEqual("System 'Group2' has no parent", e!.Message);
            
            var root2   = new SystemRoot("Systems-2");
            root2.Add(group2);
            e = Throws<InvalidOperationException>(() => {
                group2.MoveSystemTo(root1, -1);
            });
            AreEqual("Expect targetGroup == SystemRoot. Expected: 'Systems-2' was: 'Systems-1'", e!.Message);
        }
        
        [Test]
        public static void Test_SystemGroup_CastSystemChanged()
        {
            var testSystem1 = new TestSystem1();
            var root        = new SystemRoot("Systems") { testSystem1 };
            var rootCount = 0;
            var systemCount = 0;
            Action<SystemChanged> testChanged = changed => {
                var str = changed.ToString();
                switch (systemCount++) {
                    case 0: AreEqual("Update - System 'TestSystem1' field: enabled, value: True", str);   return;
                }
            };
            Action<SystemChanged> rootChanged = changed => {
                var str = changed.ToString();
                switch (rootCount++) {
                    case 0: AreEqual("Update - System 'TestSystem1' field: enabled, value: True",   str);   return;
                }
            };
            testSystem1.OnSystemChanged += testChanged;
            root.       OnSystemChanged += rootChanged;
            testSystem1.Enabled = true;
            testSystem1.CastSystemUpdate("enabled", true);
            
            testSystem1.OnSystemChanged -= testChanged;
            root.       OnSystemChanged -= rootChanged;
            testSystem1.CastSystemUpdate("enabled", false);

            AreEqual(1, rootCount);
            AreEqual(1, systemCount);
        }
        

        
        [Test]
        public static void Test_SystemGroup_FindGroup()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var child2      = new SystemGroup("Child2");
            var child1_1    = new SystemGroup("Child1_1");
            
            root.Add(child1);
            root.Add(child2);
            child1.Add(child1_1);
            
            AreSame(child1,     root.FindGroup("Child1",  true));
            AreSame(child1_1,   root.FindGroup("Child1_1",true));
            IsNull (            root.FindGroup("Unknown", true));
        }
        
        [Test]
        public static void Test_SystemGroup_FindSystem()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var mySystem1   = new MySystem1();
            var mySystem2   = new MySystem2();
            
            root.Add(child1);
            root.Add(mySystem1);
            child1.Add(mySystem2);
            
            AreSame(mySystem1,  root.FindSystem<MySystem1>(true));
            AreSame(mySystem1,  root.FindSystem<MySystem1>(false));
            AreSame(mySystem2,  root.FindSystem<MySystem2>(true));
            IsNull (            root.FindSystem<MySystem2>(false));
            IsNull (            root.FindSystem<TestSystem1>(true));
        }
        
        [Test]
        public static void Test_SystemGroup_IsAncestorOf()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var child1_1    = new SystemGroup("Child1_1");
            
            root.Add(child1);
            child1.Add(child1_1);
            
            IsTrue (root.IsAncestorOf(child1));
            IsTrue (root.IsAncestorOf(child1_1));
            
            IsFalse(root.IsAncestorOf(root));
            IsFalse(child1.IsAncestorOf(root));
            
            Throws<ArgumentNullException>(() => {
                root.IsAncestorOf(null);
            });
        }
        
        [Test]
        public static void Test_SystemGroup_Perf()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var perfSystem1 = new PerfSystem();
            var perfSystem2 = new PerfSystem();
            
            var emptyLog    = perfSystem1.GetPerfLog();
            AreEqual(
@"stores: 0                     on      last ms       sum ms      updates     last mem      sum mem     entities
---------------------         --     --------     --------     --------     --------     --------     --------
PerfSystem                     +       -1.000        0.000            0            0            0
", emptyLog);
            
            root.Add(child1);
            root.Add(perfSystem1);
            child1.Add(perfSystem2);
            
            IsFalse(root.       MonitorPerf);
            IsFalse(child1.     MonitorPerf);
            
            // --- by default perf is disabled
            root.Update(default);
            IsTrue(root.        Perf.LastMs == -1);  AreEqual(0, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs == -1);  AreEqual(0, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == -1);  AreEqual(0, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs == -1);  AreEqual(0, perfSystem2. Perf.UpdateCount);
            
            IsTrue(perfSystem2. Perf.LastTicks == -1);
            
            AreEqual("updates: 0  last: -1 ms  sum: 0 ms",  root.Perf.ToString());
            AreEqual(-1,                                    root.Perf.LastAvgMs(20)); // no updates until now
            
            // --- enable perf for entire hierarchy
            root.SetMonitorPerf(true);
            IsTrue(root.        MonitorPerf);
            IsTrue(child1.      MonitorPerf);
            
            root.Update(default);
            IsTrue(root.        Perf.LastMs > 0);   AreEqual(1, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs > 0);   AreEqual(1, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs > 0);   AreEqual(1, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs > 0);   AreEqual(1, perfSystem2. Perf.UpdateCount);
            
            AreEqual(root.          Perf.LastMs, root.       Perf.SumMs);
            AreEqual(child1.        Perf.LastMs, child1.     Perf.SumMs);
            AreEqual(perfSystem1.   Perf.LastMs, perfSystem1.Perf.SumMs);
            AreEqual(perfSystem2.   Perf.LastMs, perfSystem2.Perf.SumMs);
            
            AreEqual(root.          Perf.LastMs, root.          Perf.LastAvgMs(20));
            AreEqual(child1.        Perf.LastMs, child1.        Perf.LastAvgMs(20));
            AreEqual(perfSystem1.   Perf.LastMs, perfSystem1.   Perf.LastAvgMs(20));
            AreEqual(perfSystem2.   Perf.LastMs, perfSystem2.   Perf.LastAvgMs(20));
            
            Console.WriteLine(root.GetPerfLog());
            
            // --- Update() again to test Perf Sum
            var rootSum         = root.  Perf.SumTicks;
            var child1Sum       = child1.Perf.SumTicks;
            var perfSystem1Sum  = perfSystem1.Perf.SumTicks;
            var perfSystem2Sum  = perfSystem2.Perf.SumTicks;
            root.Update(default);
            AreEqual(rootSum        + root.         Perf.LastTicks, root.       Perf.SumTicks);
            AreEqual(child1Sum      + child1.       Perf.LastTicks, child1.     Perf.SumTicks);
            AreEqual(perfSystem1Sum + perfSystem1.  Perf.LastTicks, perfSystem1.Perf.SumTicks);
            AreEqual(perfSystem2Sum + perfSystem2.  Perf.LastTicks, perfSystem2.Perf.SumTicks);
            
            // --- disable / enable systems
            perfSystem1.Enabled = false;
            root.Update(default);
            IsTrue(root.        Perf.LastMs > 0);       AreEqual(3, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs > 0);       AreEqual(3, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == -1);     AreEqual(2, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs > 0);       AreEqual(3, perfSystem2. Perf.UpdateCount);
            
            child1.Enabled = false;
            root.Update(default);
            IsTrue(root.        Perf.LastMs > 0);       AreEqual(4, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs == -1);     AreEqual(3, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == -1);     AreEqual(2, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs == -1);     AreEqual(3, perfSystem2. Perf.UpdateCount);
            
            perfSystem1.Enabled = true;
            child1.Enabled      = true;
            root.Enabled        = false;
            root.Update(default);
            IsTrue(root.        Perf.LastMs == -1);     AreEqual(4, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs == -1);     AreEqual(3, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == -1);     AreEqual(2, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs == -1);     AreEqual(3, perfSystem2. Perf.UpdateCount);
        }
        
        [Test]
        public static void Test_SystemGroup_GetPerfLog()
        {
            var store       = new EntityStore();
            for (int n = 0; n < 10000; n++) {
                store.CreateEntity(new Position(), new Scale3());
            }
            var root        = new SystemRoot(store);
            var perfSystem1 = new ScaleSystem();
            var perfSystem2 = new PositionSystem();
            
            root.Add(perfSystem1);
            root.Add(perfSystem2);
            
            root.SetMonitorPerf(true);
            for (int n = 0; n < 10; n++) {
                root.Update(default);
            }
            Console.WriteLine(root.GetPerfLog());
/*
stores: 1                     on      last ms       sum ms      updates     last mem      sum mem     entities
---------------------         --     --------     --------     --------     --------     --------     --------
Systems [2]                    +        0.073        5.534           10          128         1392
| ScaleSystem                  +        0.037        3.197           10           64          696        10000
| PositionSystem               +        0.036        1.782           10           64          696        10000

*/
        }
        
        [Test]
        public static void Test_SystemGroup_Update_Tick()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var perfSystem1 = new PerfSystem();
            var perfSystem2 = new PerfSystem();
            
            root.Add(child1);
            root.Add(perfSystem1);
            child1.Add(perfSystem2);
            
            AreEqual(0,     root.       Tick.time);
            AreEqual(0,     child1.     Tick.time);
            AreEqual(0,     perfSystem1.Tick.time);
            AreEqual(0,     perfSystem2.Tick.time);
            
            var tick = new UpdateTick(0, 11);
            root.Update(tick);
            AreEqual(11,    root.       Tick.time);
            AreEqual(11,    child1.     Tick.time);
            AreEqual(11,    perfSystem1.Tick.time);
            AreEqual(11,    perfSystem2.Tick.time);
            
            root.SetMonitorPerf(true);
            
            child1.Enabled = false;
            tick = new UpdateTick(0, 22);
            root.Update(tick);
            AreEqual(22,    root.       Tick.time);
            AreEqual(11,    child1.     Tick.time);
            AreEqual(22,    perfSystem1.Tick.time);
            AreEqual(11,    perfSystem2.Tick.time);
        }
        
        
        [Test]
        public static void Test_SystemGroup_exceptions_add_remove()
        {
            var group = new SystemGroup("Group1");
            Throws<ArgumentNullException>(() => {
                group.Add(null);
            });
            
            Exception e = Throws<ArgumentException>(() => {
                group.Add(new SystemRoot("Root"));
            });
            AreEqual("SystemRoot must not be a child system (Parameter 'system')", e!.Message);
            
            var testSystem = new TestSystem1();
            group.Add(testSystem);
            e = Throws<ArgumentException>(() => {
                group.Add(testSystem);
            });
            AreEqual("system already added to Group 'Group1' (Parameter 'system')", e!.Message);
            
            Throws<ArgumentNullException>(() => {
                group.Remove(null);
            });
            
            var group2 = new SystemGroup("Group2");
            e = Throws<ArgumentException>(() => {
                group2.Remove(testSystem);
            });
            AreEqual("system not child of Group 'Group2' (Parameter 'system')", e!.Message);
        }
        
        [Test]
        public static void Test_SystemRoot_exceptions_name()
        {
            var e = Throws<ArgumentException>(() => {
                _ = new SystemRoot("");
            });
            AreEqual("group name must not be null or empty", e!.Message);
            
            var group = new SystemGroup("Test");
            e = Throws<ArgumentException>(() => {
                group.SetName(null);
            });
            AreEqual("group name must not be null or empty", e!.Message);
            
            e = Throws<ArgumentException>(() => {
                group.SetName("");
            });
            AreEqual("group name must not be null or empty", e!.Message);   
        }
    }
}

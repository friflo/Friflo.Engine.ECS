// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;
// ReSharper disable CompareOfFloatsByEqualityOperator

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemGroup
    {
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
            root.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (changesRoot++) {
                    case 0: AreEqual("Add - Group 'Group1' to: 'Systems'",                  str);   return;
                    case 1: AreEqual("Add - Group 'Group2' to: 'Systems'",                  str);   return;
                    case 2: AreEqual("Add - Group 'Group3' to: 'Systems'",                  str);   return;
                    case 3: AreEqual("Move - Group 'Group1' from: 'Systems' to: 'Group3'",  str);   return;
                    case 4: AreEqual("Move - Group 'Group2' from: 'Systems' to: 'Group3'",  str);   return;
                    case 5: AreEqual("Move - Group 'Group1' from: 'Group3' to: 'Group3'",   str);   return;
                    case 6: AreEqual("Move - Group 'Group2' from: 'Group3' to: 'Group3'",   str);   return;
                }
            };
            group1.OnSystemChanged += _ => {
                changesGroup1++;
            };
            group2.OnSystemChanged += _ => {
                changesGroup2++;
            };
            group3.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (changesGroup3++) {
                    case 0: AreEqual("Move - Group 'Group1' from: 'Systems' to: 'Group3'",  str);   return;
                    case 1: AreEqual("Move - Group 'Group2' from: 'Systems' to: 'Group3'",  str);   return;
                    case 2: AreEqual("Move - Group 'Group1' from: 'Group3' to: 'Group3'",   str);   return;
                    case 3: AreEqual("Move - Group 'Group2' from: 'Group3' to: 'Group3'",   str);   return;
                }
            };
            root.AddSystem(group1);
            root.AddSystem(group2);
            root.AddSystem(group3);
            
            AreEqual(0, group1.MoveSystemTo(group3, -1)); // -1  => add at tail
            AreEqual(1, group2.MoveSystemTo(group3,  1));
            AreEqual(1, group1.MoveSystemTo(group3,  2)); // returned index != passed index
            AreEqual(1, group2.MoveSystemTo(group3, -1)); // move to tail within same group

            AreEqual(1, root.ChildSystems.Count);
            AreEqual(2, group3.ChildSystems.Count);
            
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
            root.AddSystem(group1);
            root.AddSystem(group2);
            root.InsertSystemAt(2,  testSystem1);
            root.InsertSystemAt(-1, testSystem2);
            
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
                group.InsertSystemAt(0, null);
            });
            
            Exception e = Throws<ArgumentException>(() => {
                group.InsertSystemAt(-2, new TestSystem1());
            });
            AreEqual("invalid index: -2", e!.Message);
            
            e = Throws<ArgumentException>(() => {
                group.InsertSystemAt(1, new TestSystem1());
            });
            AreEqual("invalid index: 1", e!.Message);

            e = Throws<ArgumentException>(() => {
                group.InsertSystemAt(0, new SystemRoot("Systems"));
            });
            AreEqual("SystemRoot must not be a child system (Parameter 'system')", e!.Message);
            
            var testSystem1 = new TestSystem1();
            group.AddSystem(testSystem1);
            e = Throws<ArgumentException>(() => {
                group.InsertSystemAt(0, testSystem1);
            });
            AreEqual("system already added to Group 'Systems' (Parameter 'system')", e!.Message);
        }
        
        [Test]
        public static void Test_SystemGroup_RemoveSystem_with_root()
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var root        = new SystemRoot (store, "Systems");
            var testSystem1 = new TestSystem1();
            var count = 0;
            root.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Add - System 'TestSystem1' to: 'Systems'",     str);      return;
                    case 1: AreEqual("Remove - System 'TestSystem1' from: 'Systems'", str);     return;
                }
            };
            root.AddSystem(testSystem1);
            AreEqual(1, testSystem1.Queries.Count);
            AreEqual(1, root.RootSystems.Count);
            AreEqual(1, root.ChildSystems.Count);
            
            root.RemoveSystem(testSystem1);
            AreEqual(0, testSystem1.Queries.Count);
            AreEqual(0, root.RootSystems.Count);
            AreEqual(0, root.ChildSystems.Count);
            
            AreEqual(2, count);
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
                    case 0: AreEqual("Add - System 'TestSystem1' to: 'Base'",     str);      return;
                    case 1: AreEqual("Remove - System 'TestSystem1' from: 'Base'", str);     return;
                }
            };
            baseGroup.  AddSystem(testSystem1);
            AreEqual(0, testSystem1.Queries.Count);
            AreEqual(1, baseGroup.ChildSystems.Count);
            
            baseGroup.  RemoveSystem(testSystem1);
            AreEqual(0, testSystem1.Queries.Count);
            AreEqual(0, baseGroup.ChildSystems.Count);
            
            AreEqual(2, count);
        }
        
        [Test]
        public static void Test_SystemGroup_MoveTo_exception()
        {
            var root1   = new SystemRoot("Systems-1");
            var group1  = new SystemGroup("Group1");
            root1.AddSystem(group1);
            
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
            root2.AddSystem(group2);
            e = Throws<InvalidOperationException>(() => {
                group2.MoveSystemTo(root1, -1);
            });
            AreEqual("Expect targetGroup == SystemRoot. Expected: 'Systems-2' was: 'Systems-1'", e!.Message);
        }
        
        [Test]
        public static void Test_SystemGroup_CastSystemChanged()
        {
            var root        = new SystemRoot("Systems");
            var testSystem1 = new TestSystem1();
            root.AddSystem(testSystem1);
            var count = 0;
            testSystem1.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Update - System 'TestSystem1' field: enabled, value: True", str);   return;
                }
            };
            root.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Update - System 'TestSystem1' field: enabled, value: True", str);   return;
                }
            };
            testSystem1.Enabled = true;
            testSystem1.CastSystemUpdate("enabled", true);
            
            AreEqual(2, count);
        }
        
        [Test]
        public static void Test_SystemGroup_FindGroup()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var child2      = new SystemGroup("Child2");
            var child1_1    = new SystemGroup("Child1_1");
            
            root.AddSystem(child1);
            root.AddSystem(child2);
            child1.AddSystem(child1_1);
            
            AreSame(child1,     root.FindGroup("Child1"));
            AreSame(child1_1,   root.FindGroup("Child1_1"));
            IsNull (            root.FindGroup("Unknown"));
        }
        
        [Test]
        public static void Test_SystemGroup_FindSystem()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var mySystem1   = new MySystem1();
            var mySystem2   = new MySystem2();
            
            root.AddSystem(child1);
            root.AddSystem(mySystem1);
            child1.AddSystem(mySystem2);
            
            
            AreSame(mySystem1,  root.FindSystem<MySystem1>());
            AreSame(mySystem2,  root.FindSystem<MySystem2>());
            IsNull (            root.FindSystem<TestSystem1>());
        }
        
        [Test]
        public static void Test_SystemGroup_IsAncestorOf()
        {
            var root        = new SystemRoot("Systems");
            var child1      = new SystemGroup("Child1");
            var child1_1    = new SystemGroup("Child1_1");
            
            root.AddSystem(child1);
            child1.AddSystem(child1_1);
            
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
            
            root.AddSystem(child1);
            root.AddSystem(perfSystem1);
            child1.AddSystem(perfSystem2);
            
            IsFalse(root.       PerfEnabled);
            IsFalse(child1.     PerfEnabled);
            
            // --- by default perf is disabled
            root.Update(default);
            IsTrue(root.        Perf.LastMs == 0);  AreEqual(0, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs == 0);  AreEqual(0, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == 0);  AreEqual(0, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs == 0);  AreEqual(0, perfSystem2. Perf.UpdateCount);
            
            AreEqual(-1, root.   Perf.LastAvgMs(20)); // no updates until now
            
            // --- enable perf for entire hierarchy
            root.SetPerfEnabled(true);
            IsTrue(root.        PerfEnabled);
            IsTrue(child1.      PerfEnabled);
            
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
            
            // --- disable / enable systems
            perfSystem1.Enabled = false;
            root.Update(default);
            IsTrue(root.        Perf.LastMs > 0);       AreEqual(2, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs > 0);       AreEqual(2, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == -1);     AreEqual(1, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs > 0);       AreEqual(2, perfSystem2. Perf.UpdateCount);
            
            child1.Enabled = false;
            root.Update(default);
            IsTrue(root.        Perf.LastMs > 0);       AreEqual(3, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs == -1);     AreEqual(2, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == -1);     AreEqual(1, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs == -1);     AreEqual(2, perfSystem2. Perf.UpdateCount);
            
            perfSystem1.Enabled = true;
            child1.Enabled      = true;
            root.Enabled        = false;
            root.Update(default);
            IsTrue(root.        Perf.LastMs == -1);     AreEqual(3, root.        Perf.UpdateCount);
            IsTrue(child1.      Perf.LastMs == -1);     AreEqual(2, child1.      Perf.UpdateCount);
            IsTrue(perfSystem1. Perf.LastMs == -1);     AreEqual(1, perfSystem1. Perf.UpdateCount);
            IsTrue(perfSystem2. Perf.LastMs == -1);     AreEqual(2, perfSystem2. Perf.UpdateCount);
        }
        
        [Test]
        public static void Test_SystemGroup_exceptions_add_remove()
        {
            var group = new SystemGroup("Group1");
            Throws<ArgumentNullException>(() => {
                group.AddSystem(null);
            });
            
            Exception e = Throws<ArgumentException>(() => {
                group.AddSystem(new SystemRoot("Root"));
            });
            AreEqual("SystemRoot must not be a child system (Parameter 'system')", e!.Message);
            
            var testSystem = new TestSystem1();
            group.AddSystem(testSystem);
            e = Throws<ArgumentException>(() => {
                group.AddSystem(testSystem);
            });
            AreEqual("system already added to Group 'Group1' (Parameter 'system')", e!.Message);
            
            Throws<ArgumentNullException>(() => {
                group.RemoveSystem(null);
            });
            
            var group2 = new SystemGroup("Group2");
            e = Throws<ArgumentException>(() => {
                group2.RemoveSystem(testSystem);
            });
            AreEqual("system not child of Group 'Group2' (Parameter 'system')", e!.Message);
        }
        
        [Test]
        public static void Test_SystemRoot_exceptions_name()
        {
            Exception e = Throws<ArgumentException>(() => {
                _ = new SystemRoot(null);
            });
            AreEqual("group name must not be null or empty", e!.Message);
            
            e = Throws<ArgumentException>(() => {
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
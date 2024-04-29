// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemGroup
    {
        [Test]
        public static void Test_SystemGroup_MoveTo()
        {
            var baseGroup   = new SystemRoot("Systems");
            int count = 0;
            var group1      = new SystemGroup("Group1");
            var group2      = new SystemGroup("Group2");
            var group3      = new SystemGroup("Group3");
            baseGroup.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Add - Group 'Group1'", str);   return;
                    case 1: AreEqual("Add - Group 'Group2'", str);   return;
                    case 2: AreEqual("Add - Group 'Group3'", str);   return;
                    case 3: AreEqual("Move - Group 'Group1'", str);  return;
                    case 4: AreEqual("Move - Group 'Group2'", str);  return;
                    case 5: AreEqual("Move - Group 'Group1'", str);  return;
                }
            };
            baseGroup.AddSystem(group1);
            baseGroup.AddSystem(group2);
            baseGroup.AddSystem(group3);
            
            AreEqual(0, group1.MoveSystemTo(group3, -1)); // -1  => add at tail
            AreEqual(1, group2.MoveSystemTo(group3,  1));
            AreEqual(1, group1.MoveSystemTo(group3,  2)); // returned index != passed index

            AreEqual(1, baseGroup.ChildSystems.Count);
            AreEqual(2, group3.ChildSystems.Count);
            
            AreEqual(6, count);
        }
        
        [Test]
        public static void Test_SystemGroup_RemoveSystem()
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var root        = new SystemRoot (store, "base");
            var testSystem1 = new TestSystem1();
            var count = 0;
            root.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Add - System 'TestSystem1'",       str);   return;
                    case 1: AreEqual("Remove - System 'TestSystem1'",    str);   return;
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
        public static void Test_SystemGroup_MoveTo_exception()
        {
            var baseGroup   = new SystemGroup("Base");
            var group1      = new SystemGroup("Group1");
            baseGroup.AddSystem(group1);
            
            Throws<ArgumentNullException>(() => {
                group1.MoveSystemTo(null, 0);
            });
            
            Exception e = Throws<ArgumentException>(() => {
                group1.MoveSystemTo(baseGroup, -2);
            });
            AreEqual("invalid index: -2", e!.Message);
            
            e = Throws<ArgumentException>(() => {
                group1.MoveSystemTo(baseGroup, 2);
            });
            AreEqual("invalid index: 2", e!.Message);
            
            var group2      = new SystemGroup("Group2");
            e = Throws<InvalidOperationException>(() => {
                group2.MoveSystemTo(baseGroup, 1);
            });
            AreEqual("Group 'Group2' has no parent", e!.Message);
        }
        
        [Test]
        public static void Test_SystemGroup_CastSystemChanged()
        {
            var group       = new SystemGroup("Base");
            var testSystem1 = new TestSystem1();
            group.AddSystem(testSystem1);
            
            var count = 0;
            group.OnSystemChanged += changed => {
                var str = changed.ToString();
                switch (count++) {
                    case 0: AreEqual("Update - System 'TestSystem1' field: enabled, value: True", str);   return;
                }
            };
            testSystem1.Enabled = true;
            testSystem1.CastSystemUpdate("enabled", true);
            
            AreEqual(1, count);
        }
        
        [Test]
        public static void Test_SystemRoot_exceptions_add_remove()
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
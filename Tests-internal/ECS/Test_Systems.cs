// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_Systems
    {
        [Test]
        public static void Test_Systems_View()
        {
            var root        = new SystemRoot("Systems");
            var querySystem = new TestQuerySystem();
            root.AddSystem(querySystem);
            
            var view = querySystem.System;
            AreEqual("TestQuerySystem",         view.Name);
            AreEqual("Enabled: True  Id: 1",    view.ToString());
            AreEqual(1,                         view.Id);
            AreEqual(true,                      view.Enabled);
            AreEqual(new Tick(),                view.Tick);
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
            
            root.AddSystem(group1);
            root.AddSystem(group2);
            root.AddSystem(querySystem);
            
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
            
            root.AddSystem(child);
            AreSame(store, child.CommandBuffers[0].EntityStore);
            
            var testQuerySystem = new TestQuerySystem();
            child.AddSystem(testQuerySystem);
            AreSame(testQuerySystem, child.ChildSystems[0]);
        }
    }
    
    class TestQuerySystem : QuerySystem<Position> {
        protected override void OnUpdate() { }
    }
}
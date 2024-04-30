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
            AreEqual(1,                 view.Id);
            AreEqual(true,              view.Enabled);
            AreEqual(new Tick(),        view.Tick);
            AreSame (root,              view.SystemRoot);
            AreSame (root,              view.ParentGroup);
            
            NotNull(root.System);
            
            NotNull(root.CommandBuffers);
        }
        
        [Test]
        public static void Test_Systems_RemoveSystem_coverage()
        {
            var root        = new SystemGroup("Test");
            var querySystem = new TestQuerySystem();
            querySystem.parentGroup = root;
            root.RemoveSystem(querySystem);
        }
    }
    
    class TestQuerySystem : QuerySystem<Position> {
        protected override void OnUpdate() { }
    }
}
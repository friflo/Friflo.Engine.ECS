// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemRoot
    {
        [Test]
        public static void Test_SystemRoot_Add_System_minimal()
        {
            var store   = new EntityStore(PidType.UsePidAsId);
            var entity  = store.CreateEntity(new Position());
            var root    = new SystemRoot(store);
            root.AddSystem(new TestSystem1());
            root.Update(default);
            AreEqual(new Position(1,0,0), entity.Position);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_Group()
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var root        = new SystemRoot("Systems");
            var group1      = new SystemGroup("Group1");
            var testGroup   = new TestGroup();
            
            IsNull(root.FindGroup("group1"));
            IsNull(root.FindGroup("TestGroup"));
            root.AddSystem(group1);
            root.AddSystem(testGroup);
            
            AreEqual("Root 'Systems' systems: 2", root.ToString());
            AreEqual("Group 'Group1' systems: 0", group1.ToString());
            AreEqual("Group 'TestGroup' systems: 0", testGroup.ToString());
            
            AreEqual(2,     root.RootSystems.Count);
            AreSame(group1, root.FindGroup("Group1"));
            AreSame(testGroup, root.FindGroup("TestGroup"));
            
            AreEqual(2, root.ChildSystems.Count);
            
            root.AddStore(store);
            AreEqual(1, root.Stores.Count);
            
            root.Update(default);
            AreEqual(1, testGroup.beginCalled);
            AreEqual(1, testGroup.endCalled);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_System()
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var entity      = store.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot(store);    // create SystemRoot with store
            var group       = new SystemGroup("Group");
            root.AddSystem(group);
            var testSystem1 = new TestSystem1();
            AreEqual("TestSystem1 - Components: [Position]", testSystem1.ToString());
            AreEqual("Components: [Position]", testSystem1.ComponentTypes.ToString());
            AreEqual(0,     testSystem1.Queries.Count);
            group.AddSystem(testSystem1);
            AreEqual(1,     testSystem1.Queries.Count);
            AreEqual(1,     testSystem1.EntityCount);
            AreSame(root,   testSystem1.SystemRoot);
            
            root.Update(default);
            AreEqual(new Scale3(4,5,6), entity.Scale3);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_RemoveStore()
        {
            var store1      = new EntityStore(PidType.UsePidAsId);
            var store2      = new EntityStore(PidType.UsePidAsId);
            store1.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot("Systems");   // create SystemRoot without store
            var group       = new SystemGroup("Group");
            var testSystem1 = new TestSystem1();
            group.AddSystem(testSystem1);
            root.AddSystem(group);
            
            // --- add store
            AreEqual(0, testSystem1.Queries.Count);
            root.AddStore(store1);                      // add store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem1.Queries.Count);
            AreEqual(1, testSystem1.EntityCount);
            root.Update(default);
            
            root.AddStore(store2);                      // add store after system setup
            AreEqual(2, root.Stores.Count);
            AreEqual(2, testSystem1.Queries.Count);
            
            // --- remove store
            root.RemoveStore(store1);                   // remove store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem1.Queries.Count);
            
            root.RemoveStore(store2);                   // remove store after system setup
            AreEqual(0, root.Stores.Count);
            AreEqual(0, testSystem1.Queries.Count); 
        }
        
        [Test]
        public static void Test_System_Name()
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
        public static void Test_SystemRoot_Update_Perf()
        {
            int count   = 10;   // 100_000_000 ~ #PC: 3337 ms
            var store   = new EntityStore(PidType.UsePidAsId);
            var root    = new SystemRoot("Systems");
            root.AddSystem(new TestSystem2());
            root.AddStore(store);

            var sw = new Stopwatch();
            sw.Start();
            for (int n = 0; n < count; n++) {
                root.Update(default);
            }
            Console.WriteLine($"Test_SystemRoot_Update_Perf - count: {count}, duration: {sw.ElapsedMilliseconds} ms");
        }
    }
    
    public class TestSystem1 : QuerySystem<Position>
    {
        protected override void OnUpdate(Tick tick) {
            Query.ForEachEntity((ref Position position, Entity entity) => {
                position.x++;
                CommandBuffer.AddComponent(entity.Id, new Scale3(4,5,6));
            });
        }
    }
    
    public class TestSystem2 : QuerySystem<Position>
    {
        protected override void OnUpdate(Tick tick) {
            foreach (var (positions, _)  in Query.Chunks) {
                foreach (ref var position in positions.Span) {
                    position.x++;
                }
            }
        }
    }
    
    public class TestGroup : SystemGroup {
        internal int beginCalled;
        internal int endCalled;
        
        public TestGroup() : base("TestGroup") { }

        protected override void OnUpdateGroupBegin(Tick tick) {
            AreEqual(1, SystemRoot.Stores.Count);
            beginCalled++;
        }

        protected override void OnUpdateGroupEnd(Tick tick) {
            AreEqual(1, SystemRoot.Stores.Count);
            endCalled++;
        }
    }
    
    // Ensure a custom System class can be declared without any overrides
    public class MySystem1 : BaseSystem { }
    
    // A custom System class with all possible overrides
    public class MySystem2 : BaseSystem {
        public      override string Name => "MySystem2 - custom name";
        
        protected   override void   OnUpdateGroupBegin(Tick tick) { }
        protected   override void   OnUpdateGroupEnd(Tick tick)   { }
        public      override void   Update(Tick tick)             { }
    }
}
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_QuerySystem
    {
        [Test]
        public static void Test_QuerySystem_ToString()
        {
            var query0 = new TestQuerySystem0();
            var query1 = new TestQuerySystem1();
            var query2 = new TestQuerySystem2();
            var query3 = new TestQuerySystem3();
            var query4 = new TestQuerySystem4();
            var query5 = new TestQuerySystem5();
            
            AreEqual("TestQuerySystem0 - []",                                                       query0.ToString());
            AreEqual("TestQuerySystem1 - [Position]",                                               query1.ToString());
            AreEqual("TestQuerySystem2 - [Position, Scale3]",                                       query2.ToString());
            AreEqual("TestQuerySystem3 - [Position, Scale3, Rotation]",                             query3.ToString());
            AreEqual("TestQuerySystem4 - [Position, Scale3, Rotation, MyComponent5]",               query4.ToString());
            AreEqual("TestQuerySystem5 - [Position, Scale3, Rotation, MyComponent5, MyComponent6]", query5.ToString());
        }
        
        [Test]
        public static void Test_System_Enabled()
        {
            var store   = new EntityStore();
            var entity  = store.CreateEntity(new Position(1,2,3));
            
            var root    = new SystemRoot(store);
            var query1  = new TestSystem1();
            var group   = new TestGroup();

            root.Add(query1);
            root.Add(group);
            
            var tick = new UpdateTick(42, 0);
            root.Update(tick);
            AreEqual(new Position(2,2,3),   entity.Position);
            AreEqual(1,                     group.beginCalled);
            AreEqual(1,                     group.endCalled);
            
            query1.Enabled = false;
            group.Enabled  = false;
            root.Update(tick);
            AreEqual(new Position(2,2,3),   entity.Position);
            AreEqual(1,                     group.beginCalled);
            AreEqual(1,                     group.endCalled);
        }
    }
    
    internal class TestQuerySystem0 : QuerySystem {
        protected override void OnUpdate() { }
    }
    
    internal class TestQuerySystem1 : QuerySystem<Position> {
        protected override void OnUpdate() { }
    }
    internal class TestQuerySystem2 : QuerySystem<Position,Scale3> {
        protected override void OnUpdate() { }
    }
    internal class TestQuerySystem3 : QuerySystem<Position,Scale3,Rotation> {
        protected override void OnUpdate() { }
    }
    internal class TestQuerySystem4 : QuerySystem<Position,Scale3,Rotation,MyComponent5> {
        protected override void OnUpdate() { }
    }
    internal class TestQuerySystem5 : QuerySystem<Position,Scale3,Rotation,MyComponent5,MyComponent6> {
        protected override void OnUpdate() { }
    }
}
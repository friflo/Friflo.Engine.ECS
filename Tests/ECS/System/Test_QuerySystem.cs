// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_QuerySystem
    {
        // [Test]
        public static void Test_Systems_Tick()
        {
            var query1 = new TestQuerySystem1();
            var query2 = new TestQuerySystem2();
            var query3 = new TestQuerySystem3();
            var query4 = new TestQuerySystem4();
            var query5 = new TestQuerySystem5();
            
            AreEqual("TestQuerySystem1 - Components: [Position]", query1.ToString());
            AreEqual("TestQuerySystem2 - Components: [Position, Scale3]", query2.ToString());
            AreEqual("TestQuerySystem3 - Components: [Position, Scale3, Rotation]", query3.ToString());
            AreEqual("TestQuerySystem4 - Components: [Position, Scale3, Rotation, MyComponent5]", query4.ToString());
            AreEqual("TestQuerySystem5 - Components: [Position, Scale3, Rotation, MyComponent5, MyComponent6]", query5.ToString());
        }
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
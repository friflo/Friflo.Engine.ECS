using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators;

public partial class MyExample
{
    [Query]
    void MoveExample(ref Position position, Entity entity) {
        AreEqual(1, entity.Id);
        position.x = 1;
    }
    
    [Query]
    static void MoveExample2(ref MyComponent1 myComponent1, MyComponent2 myComponent2, int someValue, in float inValue, ref string refValue) {
        myComponent1.a = someValue;
    } 
}

public static class Test_QueryGenerator
{
    [Test]
    public static void Test_QueryGenerator_generate()
    {
        var tester = new MyExample();
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position(), new MyComponent1(), new MyComponent2());
        {
            var query = tester.MoveExampleQuery(store);
            AreEqual(1, entity.GetComponent<Position>().x);
            AreEqual(1, query.Count);
        }
        {
            var str = "abc";
            var query = MyExample.MoveExample2Query(store, 123, 456, ref str);
            AreEqual(123, entity.GetComponent<MyComponent1>().a);
            AreEqual(1, query.Count);
        }
    }
}
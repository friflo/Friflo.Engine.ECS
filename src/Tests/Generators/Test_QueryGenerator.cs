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
        AreEqual(0, myComponent2.b);
        AreEqual(123, someValue);
        AreEqual(456, inValue);
        refValue = "xyz";
    }
    
    [Query]
    [AllComponents<MyComponent1>]
    [AnyComponents<MyComponent2, MyComponent3>]
    [WithoutAllComponents<MyComponent4>]
    [WithoutAnyComponents<MyComponent5, MyComponent6>]
    [AllTags<TestTag>]
    [AnyTags<TestTag2, TestTag3>]
    [WithoutAllTags<TestTag4>]
    [WithoutAnyTags<TestTag5, TestTag6>]
    void TestFilters(ref Position position) {
        position.x = 1;
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
            AreEqual("xyz", str);
        }
    }
    
    [Test]
    public static void Test_QueryGenerator_filters()
    {
        var tester = new MyExample(); 
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position(), new MyComponent1(), new MyComponent2(), Tags.Get<TestTag, TestTag2>());

        var query = tester.TestFiltersQuery(store);
        AreEqual(1, entity.GetComponent<Position>().x);
        AreEqual(1, query.Count);
    }
}
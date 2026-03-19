using System;
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
    private static void MoveExample2(in MyComponent1 inComponent, MyComponent2 myComponent2, int someValue, in float inValue, ref string refValue, DateTime dateTime) {
        AreEqual(0, inComponent.a);
        AreEqual(0, myComponent2.b);
        AreEqual(123, someValue);
        AreEqual(456, inValue);
        AreEqual(new DateTime(2026,03,18), dateTime);
        AreEqual("abc", refValue);
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
            var query = MyExample.MoveExample2Query(store, 123, 456, ref str, new DateTime(2026,03,18));
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
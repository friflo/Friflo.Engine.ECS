using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;


// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Query;

public partial class MyExample
{
    [Query]
    void MoveExample(ref Position position, Entity entity) {        
        Assert.That(entity.Id, Is.EqualTo(1)); 
        position.x = 1;
    }
    
    [Query]
    static void DuplicateMethod(Position position) { }

    // Generates method with duplicate name but different signature
    [Query]
    static void DuplicateMethod(Position position, float x, float y) { }
    
    // Commented method will result in compiler error:
    //     Error CS0111 : Type 'MyExample' already defines a member called 'DuplicateMethodQuery' with the same parameter types
    // [Query]
    // static void DuplicateMethod(EntityName name) { }

    [Query]
    private static void MoveExample2(in MyComponent1 inComponent, MyComponent2 myComponent2, int someValue, in float inValue, ref string refValue, DateTime dateTime) {
        Assert.That(inComponent.a, Is.EqualTo(0));
        Assert.That(myComponent2.b, Is.EqualTo(0));
        Assert.That(someValue, Is.EqualTo(123));
        Assert.That(inValue, Is.EqualTo(456));
        Assert.That(dateTime, Is.EqualTo(new DateTime(2026,03,18)));
        Assert.That(refValue, Is.EqualTo("abc"));
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
            Assert.That(entity.GetComponent<Position>().x, Is.EqualTo(1));
            Assert.That(query.Count, Is.EqualTo(1));
        }
        {
            var str = "abc";
            var query = MyExample.MoveExample2Query(store, 123, 456, ref str, new DateTime(2026,03,18));
            Assert.That(query.Count, Is.EqualTo(1));
            Assert.That(str, Is.EqualTo("xyz"));
        }
    }
    
    [Test]
    public static void Test_QueryGenerator_filters()
    {
        var tester = new MyExample(); 
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position(), new MyComponent1(), new MyComponent2(), Tags.Get<TestTag, TestTag2>());

        var query = tester.TestFiltersQuery(store);
        Assert.That(entity.GetComponent<Position>().x, Is.EqualTo(1));
        Assert.That(query.Count, Is.EqualTo(1));
    }
}
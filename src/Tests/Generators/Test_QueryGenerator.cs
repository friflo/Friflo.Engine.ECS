using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable CheckNamespace
namespace GeneratedCode;


public partial class MyExample
{
    [Query] // This triggers the generator
    void MoveExample(ref Position position) { position.x = 1; }
    
    [Query] // This triggers the generator
    static void MoveExample2(ref MyComponent1 myComponent1, int someValue) { myComponent1.a = someValue; } 
}

public static class Test_QueryGenerator
{
    [Test]
    public static void Test_Entity_new_EntityStore_Perf()
    {
        var tester = new MyExample();
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position(), new MyComponent1());
        
        tester.MoveExampleQuery(store);
        AreEqual(1, entity.GetComponent<Position>().x);
        
        MyExample.MoveExample2Query(store, 123);
        AreEqual(123, entity.GetComponent<MyComponent1>().a);
    }
}
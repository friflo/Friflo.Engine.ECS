using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable CheckNamespace
namespace GeneratedCode;


public partial class MyExample
{
    [Query] // This triggers the generator
    void MoveExample(ref Position position) { }
    
    [Query] // This triggers the generator
    void MoveExample2(ref MyComponent1 myComponent1, int deltaTime) { } 
}

public static class Test_QueryGenerator
{
    [Test]
    public static void Test_Entity_new_EntityStore_Perf()
    {
        var tester = new MyExample();
        var store = new EntityStore();
        tester.MoveExampleQuery(store);
    }
}